using FluentAssertions;
using OllamaPlayground.Rag;

namespace OllamaPlayground.Tests;

public class DocumentLoaderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DocumentLoader _sut;

    public DocumentLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docloader_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _sut = new DocumentLoader(chunkSize: 5, chunkOverlap: 1);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteFile(string name, string content)
        => File.WriteAllText(Path.Combine(_tempDir, name), content);

    // ── LoadFolder ──────────────────────────────────────────────────

    [Fact]
    public void LoadFolder_WhenEmpty_ShouldReturnNoChunks()
    {
        var result = _sut.LoadFolder(_tempDir).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void LoadFolder_ShouldOnlyLoadTxtFiles()
    {
        WriteFile("doc.txt", "hello world");
        WriteFile("doc.md", "should be ignored");
        WriteFile("doc.csv", "also ignored");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result.Should().AllSatisfy(c => c.SourceFile.Should().EndWith(".txt"));
    }

    [Fact]
    public void LoadFolder_SingleSmallFile_ShouldReturnOneChunk()
    {
        WriteFile("small.txt", "one two three");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result.Should().HaveCount(1);
        result[0].Text.Should().Be("one two three");
        result[0].SourceFile.Should().Be("small.txt");
        result[0].ChunkIndex.Should().Be(0);
    }

    [Fact]
    public void LoadFolder_ShouldSetEmptyEmbedding()
    {
        WriteFile("a.txt", "word");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result[0].Embedding.Should().BeEmpty();
    }

    // ── Chunking logic ──────────────────────────────────────────────

    [Fact]
    public void LoadFolder_LargeFile_ShouldSplitIntoMultipleChunks()
    {
        // chunkSize=5, chunkOverlap=1 → step=4
        // 10 words → chunks at index 0 (words 0-4), 4 (words 4-8), 8 (words 8-9)
        WriteFile("big.txt", "w1 w2 w3 w4 w5 w6 w7 w8 w9 w10");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result.Should().HaveCount(3);
    }

    [Fact]
    public void LoadFolder_Chunks_ShouldHaveSequentialIndices()
    {
        WriteFile("multi.txt", "a b c d e f g h i j");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result.Select(c => c.ChunkIndex).Should().BeInAscendingOrder();
        result[0].ChunkIndex.Should().Be(0);
    }

    [Fact]
    public void LoadFolder_Chunks_ShouldOverlap()
    {
        // chunkSize=5, step=4 → last word of chunk 0 = first word of chunk 1
        WriteFile("overlap.txt", "a b c d e f g h i j");

        var result = _sut.LoadFolder(_tempDir).ToList();
        var words0 = result[0].Text.Split(' ');
        var words1 = result[1].Text.Split(' ');

        // The last word of chunk 0 should be the first word of chunk 1
        words0.Last().Should().Be(words1.First());
    }

    [Fact]
    public void LoadFolder_ExactChunkSize_ShouldProduceOverlapChunk()
    {
        // chunkSize=5, step=4 → 5 words starts at index 0 and 4
        // chunk 0: "a b c d e", chunk 1: "e" (overlap tail)
        WriteFile("exact.txt", "a b c d e");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result.Should().HaveCount(2);
        result[0].Text.Should().Be("a b c d e");
        result[1].Text.Should().Be("e");
    }

    [Fact]
    public void LoadFolder_MultipleFiles_ShouldLoadAll()
    {
        WriteFile("one.txt", "hello");
        WriteFile("two.txt", "world");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result.Should().HaveCount(2);
        result.Select(c => c.SourceFile).Should().Contain("one.txt").And.Contain("two.txt");
    }

    [Fact]
    public void LoadFolder_ShouldHandleMultipleWhitespaceTypes()
    {
        WriteFile("ws.txt", "hello\tworld\nfoo\r\nbar");

        var result = _sut.LoadFolder(_tempDir).ToList();

        result[0].Text.Should().Be("hello world foo bar");
    }

    // ── Constructor defaults ────────────────────────────────────────

    [Fact]
    public void DefaultChunkSize_ShouldProduceLargerChunks()
    {
        var defaultLoader = new DocumentLoader(); // chunkSize=150, overlap=20
        var words = string.Join(" ", Enumerable.Range(1, 200).Select(i => $"w{i}"));
        WriteFile("default.txt", words);

        var result = defaultLoader.LoadFolder(_tempDir).ToList();

        // 200 words, chunkSize=150, step=130 → 2 chunks
        result.Should().HaveCount(2);
    }
}
