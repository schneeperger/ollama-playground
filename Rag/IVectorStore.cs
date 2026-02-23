namespace OllamaPlayground.Rag;

public interface IVectorStore
{
    void Add(DocumentChunk chunk);
    int Count { get; }
    IEnumerable<DocumentChunk> Search(float[] queryVector, int topK = 3);
}
