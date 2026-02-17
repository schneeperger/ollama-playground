namespace OllamaPlayground.Rag;

public class VectorStore
{
    private readonly List<DocumentChunk> _chunks = [];

    public void Add(DocumentChunk chunk) => _chunks.Add(chunk);

    public int Count => _chunks.Count;

    public IEnumerable<DocumentChunk> Search(float[] queryVector, int topK = 3)
    {
        return _chunks
            .Select(chunk => (chunk, score: DotProduct(chunk.Embedding, queryVector)))
            .OrderByDescending(x => x.score)
            .Take(topK)
            .Select(x => x.chunk);
    }

    // Dot product works as cosine similarity when both vectors are L2-normalized,
    // which Ollama guarantees for all embedding models.
    private static float DotProduct(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new InvalidOperationException(
                $"Vector dimension mismatch: {a.Length} vs {b.Length}");

        float sum = 0f;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];

        return sum;
    }
}
