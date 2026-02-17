using OllamaClient;

namespace OllamaPlayground.Rag;

public class RagService
{
    private readonly IOllamaService _ollama;
    private readonly VectorStore _store;
    private readonly DocumentLoader _loader;

    public RagService(IOllamaService ollama, DocumentLoader loader, VectorStore store)
    {
        _ollama = ollama;
        _loader = loader;
        _store = store;
    }

    public async Task IndexDocumentsAsync(string documentsFolder)
    {
        Console.WriteLine($"[RAG] Indexing documents from: {documentsFolder}");

        var chunks = _loader.LoadFolder(documentsFolder).ToList();
        Console.WriteLine($"[RAG] Found {chunks.Count} chunks to embed...");

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = await _ollama.EmbedAsync(chunk.Text);

            var indexedChunk = chunk with { Embedding = embedding };
            _store.Add(indexedChunk);

            if ((i + 1) % 10 == 0 || i == chunks.Count - 1)
                Console.WriteLine($"[RAG]   Embedded {i + 1}/{chunks.Count} chunks");
        }

        Console.WriteLine($"[RAG] Indexing complete. {_store.Count} chunks in memory.\n");
    }

    public async Task<string> BuildContextAsync(string query, int topK = 3)
    {
        var queryEmbedding = await _ollama.EmbedAsync(query);
        var relevantChunks = _store.Search(queryEmbedding, topK).ToList();

        if (relevantChunks.Count == 0)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Relevant context from your knowledge base:");
        sb.AppendLine();

        for (int i = 0; i < relevantChunks.Count; i++)
        {
            var chunk = relevantChunks[i];
            sb.AppendLine($"[Source: {chunk.SourceFile}, chunk {chunk.ChunkIndex}]");
            sb.AppendLine(chunk.Text);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
