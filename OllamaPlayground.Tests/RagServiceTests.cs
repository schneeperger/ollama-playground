using FluentAssertions;
using Moq;
using OllamaClient;
using OllamaPlayground.Rag;

namespace OllamaPlayground.Tests;

public class RagServiceTests
{
    private readonly Mock<IOllamaService> _ollamaMock = new();
    private readonly Mock<IDocumentLoader> _loaderMock = new();
    private readonly Mock<IVectorStore> _storeMock = new();
    private readonly RagService _sut;

    public RagServiceTests()
    {
        _sut = new RagService(_ollamaMock.Object, _loaderMock.Object, _storeMock.Object);
    }

    // ── IndexDocumentsAsync ─────────────────────────────────────────

    [Fact]
    public async Task IndexDocumentsAsync_ShouldLoadChunksFromFolder()
    {
        _loaderMock.Setup(l => l.LoadFolder("docs"))
            .Returns(Enumerable.Empty<DocumentChunk>());

        await _sut.IndexDocumentsAsync("docs");

        _loaderMock.Verify(l => l.LoadFolder("docs"), Times.Once);
    }

    [Fact]
    public async Task IndexDocumentsAsync_ShouldEmbedEachChunk()
    {
        var chunks = new List<DocumentChunk>
        {
            new("text1", "file1.txt", 0, []),
            new("text2", "file2.txt", 0, []),
        };

        _loaderMock.Setup(l => l.LoadFolder("docs")).Returns(chunks);
        _ollamaMock.Setup(o => o.EmbedAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new float[] { 1f, 0f });

        await _sut.IndexDocumentsAsync("docs");

        _ollamaMock.Verify(o => o.EmbedAsync("text1", null), Times.Once);
        _ollamaMock.Verify(o => o.EmbedAsync("text2", null), Times.Once);
    }

    [Fact]
    public async Task IndexDocumentsAsync_ShouldAddEmbeddedChunksToStore()
    {
        var chunks = new List<DocumentChunk>
        {
            new("hello", "a.txt", 0, []),
        };
        var embedding = new float[] { 0.5f, 0.5f };

        _loaderMock.Setup(l => l.LoadFolder("docs")).Returns(chunks);
        _ollamaMock.Setup(o => o.EmbedAsync("hello", null)).ReturnsAsync(embedding);

        await _sut.IndexDocumentsAsync("docs");

        _storeMock.Verify(s => s.Add(It.Is<DocumentChunk>(c =>
            c.Text == "hello" &&
            c.SourceFile == "a.txt" &&
            c.Embedding == embedding)), Times.Once);
    }

    [Fact]
    public async Task IndexDocumentsAsync_WhenNoChunks_ShouldNotCallEmbed()
    {
        _loaderMock.Setup(l => l.LoadFolder("empty")).Returns(Enumerable.Empty<DocumentChunk>());

        await _sut.IndexDocumentsAsync("empty");

        _ollamaMock.Verify(o => o.EmbedAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
        _storeMock.Verify(s => s.Add(It.IsAny<DocumentChunk>()), Times.Never);
    }

    // ── BuildContextAsync ───────────────────────────────────────────

    [Fact]
    public async Task BuildContextAsync_ShouldEmbedTheQuery()
    {
        _ollamaMock.Setup(o => o.EmbedAsync("my question", null))
            .ReturnsAsync(new float[] { 1f, 0f });
        _storeMock.Setup(s => s.Search(It.IsAny<float[]>(), 3))
            .Returns(Enumerable.Empty<DocumentChunk>());

        await _sut.BuildContextAsync("my question");

        _ollamaMock.Verify(o => o.EmbedAsync("my question", null), Times.Once);
    }

    [Fact]
    public async Task BuildContextAsync_ShouldPassTopKToStore()
    {
        var queryVec = new float[] { 1f };
        _ollamaMock.Setup(o => o.EmbedAsync(It.IsAny<string>(), null)).ReturnsAsync(queryVec);
        _storeMock.Setup(s => s.Search(queryVec, 5))
            .Returns(Enumerable.Empty<DocumentChunk>());

        await _sut.BuildContextAsync("q", topK: 5);

        _storeMock.Verify(s => s.Search(queryVec, 5), Times.Once);
    }

    [Fact]
    public async Task BuildContextAsync_WhenNoRelevantChunks_ShouldReturnEmpty()
    {
        _ollamaMock.Setup(o => o.EmbedAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new float[] { 1f });
        _storeMock.Setup(s => s.Search(It.IsAny<float[]>(), It.IsAny<int>()))
            .Returns(Enumerable.Empty<DocumentChunk>());

        var result = await _sut.BuildContextAsync("anything");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildContextAsync_ShouldIncludeChunkTextInResult()
    {
        var chunks = new List<DocumentChunk>
        {
            new("relevant info", "doc.txt", 0, [1f]),
        };

        _ollamaMock.Setup(o => o.EmbedAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new float[] { 1f });
        _storeMock.Setup(s => s.Search(It.IsAny<float[]>(), It.IsAny<int>()))
            .Returns(chunks);

        var result = await _sut.BuildContextAsync("query");

        result.Should().Contain("relevant info");
    }

    [Fact]
    public async Task BuildContextAsync_ShouldIncludeSourceMetadata()
    {
        var chunks = new List<DocumentChunk>
        {
            new("data", "notes.md", 2, [1f]),
        };

        _ollamaMock.Setup(o => o.EmbedAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new float[] { 1f });
        _storeMock.Setup(s => s.Search(It.IsAny<float[]>(), It.IsAny<int>()))
            .Returns(chunks);

        var result = await _sut.BuildContextAsync("query");

        result.Should().Contain("[Source: notes.md, chunk 2]");
    }

    [Fact]
    public async Task BuildContextAsync_WithMultipleChunks_ShouldIncludeAll()
    {
        var chunks = new List<DocumentChunk>
        {
            new("first chunk", "a.txt", 0, [1f]),
            new("second chunk", "b.txt", 1, [1f]),
        };

        _ollamaMock.Setup(o => o.EmbedAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new float[] { 1f });
        _storeMock.Setup(s => s.Search(It.IsAny<float[]>(), It.IsAny<int>()))
            .Returns(chunks);

        var result = await _sut.BuildContextAsync("query");

        result.Should().Contain("first chunk");
        result.Should().Contain("second chunk");
        result.Should().Contain("[Source: a.txt, chunk 0]");
        result.Should().Contain("[Source: b.txt, chunk 1]");
    }

    [Fact]
    public async Task BuildContextAsync_ShouldStartWithHeader()
    {
        var chunks = new List<DocumentChunk>
        {
            new("data", "f.txt", 0, [1f]),
        };

        _ollamaMock.Setup(o => o.EmbedAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new float[] { 1f });
        _storeMock.Setup(s => s.Search(It.IsAny<float[]>(), It.IsAny<int>()))
            .Returns(chunks);

        var result = await _sut.BuildContextAsync("query");

        result.Should().StartWith("Relevant context from your knowledge base:");
    }
}
