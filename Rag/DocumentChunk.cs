namespace OllamaPlayground.Rag;

public record DocumentChunk(
    string Text,
    string SourceFile,
    int ChunkIndex,
    float[] Embedding
);
