namespace OllamaPlayground.Rag;

public class DocumentLoader(int chunkSize = 150, int chunkOverlap = 20) : IDocumentLoader
{
    public IEnumerable<DocumentChunk> LoadFolder(string folderPath)
    {
        foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.txt"))
        {
            var text = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);

            foreach (var chunk in SplitIntoChunks(text, fileName))
                yield return chunk;
        }
    }

    private IEnumerable<DocumentChunk> SplitIntoChunks(string text, string fileName)
    {
        var words = text.Split(
            [' ', '\t', '\n', '\r'],
            StringSplitOptions.RemoveEmptyEntries);

        var index = 0;
        var chunkIndex = 0;
        var step = chunkSize - chunkOverlap;

        while (index < words.Length)
        {
            var chunkWords = words.Skip(index).Take(chunkSize);
            var chunkText = string.Join(" ", chunkWords);

            yield return new DocumentChunk(chunkText, fileName, chunkIndex, []);

            index += step;
            chunkIndex++;
        }
    }
}
