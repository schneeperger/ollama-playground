namespace OllamaPlayground.Rag;

public interface IDocumentLoader
{
    IEnumerable<DocumentChunk> LoadFolder(string folderPath);
}
