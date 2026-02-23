using FluentAssertions;
using OllamaPlayground.Rag;

namespace OllamaPlayground.Tests;

public class VectorStoreTests
{
    private readonly VectorStore _sut = new();

    private static DocumentChunk CreateChunk(string text, float[] embedding, int index = 0)
        => new(text, "test.txt", index, embedding);

    [Fact]
    public void Count_WhenEmpty_ShouldBeZero()
    {
        _sut.Count.Should().Be(0);
    }

    [Fact]
    public void Add_SingleChunk_ShouldIncrementCount()
    {
        _sut.Add(CreateChunk("hello", [1f, 0f, 0f]));

        _sut.Count.Should().Be(1);
    }

    [Fact]
    public void Add_MultipleChunks_ShouldTrackAll()
    {
        _sut.Add(CreateChunk("a", [1f, 0f]));
        _sut.Add(CreateChunk("b", [0f, 1f]));
        _sut.Add(CreateChunk("c", [1f, 1f]));

        _sut.Count.Should().Be(3);
    }

    [Fact]
    public void Search_ShouldReturnMostSimilarChunks()
    {
        // Arrange — normalized vectors along axes
        _sut.Add(CreateChunk("exact match", [1f, 0f, 0f], 0));
        _sut.Add(CreateChunk("orthogonal", [0f, 1f, 0f], 1));
        _sut.Add(CreateChunk("opposite", [-1f, 0f, 0f], 2));

        float[] query = [1f, 0f, 0f];

        // Act
        var results = _sut.Search(query, topK: 2).ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Text.Should().Be("exact match");
        results[1].Text.Should().Be("orthogonal");
    }

    [Fact]
    public void Search_ShouldRespectDefaultTopK()
    {
        for (int i = 0; i < 5; i++)
            _sut.Add(CreateChunk($"chunk{i}", [1f, 0f], i));

        var results = _sut.Search([1f, 0f]).ToList();

        results.Should().HaveCount(3); // default topK = 3
    }

    [Fact]
    public void Search_WhenTopKExceedsCount_ShouldReturnAll()
    {
        _sut.Add(CreateChunk("only", [1f, 0f]));

        var results = _sut.Search([1f, 0f], topK: 10).ToList();

        results.Should().HaveCount(1);
    }

    [Fact]
    public void Search_WhenEmpty_ShouldReturnEmpty()
    {
        var results = _sut.Search([1f, 0f], topK: 3).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Search_WithDimensionMismatch_ShouldThrow()
    {
        _sut.Add(CreateChunk("2d", [1f, 0f]));

        var act = () => _sut.Search([1f, 0f, 0f]).ToList();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*dimension mismatch*");
    }

    [Fact]
    public void Search_ShouldOrderByDescendingSimilarity()
    {
        _sut.Add(CreateChunk("low", [0.1f, 0.9f], 0));
        _sut.Add(CreateChunk("high", [0.9f, 0.1f], 1));
        _sut.Add(CreateChunk("mid", [0.5f, 0.5f], 2));

        var results = _sut.Search([1f, 0f], topK: 3).ToList();

        results[0].Text.Should().Be("high");
        results[1].Text.Should().Be("mid");
        results[2].Text.Should().Be("low");
    }
}
