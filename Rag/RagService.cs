using OllamaClient;

namespace OllamaPlayground.Rag;

public class RagService(IOllamaService ollama, IDocumentLoader loader, IVectorStore store) : IRagService
{
    public async Task IndexDocumentsAsync(string documentsFolder)
    {
        Console.WriteLine($"[RAG] Indexing documents from: {documentsFolder}");

        var chunks = loader.LoadFolder(documentsFolder).ToList();
        Console.WriteLine($"[RAG] Found {chunks.Count} chunks to embed...");

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = await ollama.EmbedAsync(chunk.Text);

            var indexedChunk = chunk with { Embedding = embedding };
            store.Add(indexedChunk);

            if ((i + 1) % 10 == 0 || i == chunks.Count - 1)
                Console.WriteLine($"[RAG]   Embedded {i + 1}/{chunks.Count} chunks");
        }

        Console.WriteLine($"[RAG] Indexing complete. {store.Count} chunks in memory.\n");
    }

    public async Task<string> BuildContextAsync(string query, int topK = 3)
    {
        var queryEmbedding = await ollama.EmbedAsync(query);
        var relevantChunks = store.Search(queryEmbedding, topK).ToList();

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
