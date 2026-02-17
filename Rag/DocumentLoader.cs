namespace OllamaPlayground.Rag;

public class DocumentLoader
{
    private readonly int _chunkSize;
    private readonly int _chunkOverlap;

    public DocumentLoader(int chunkSize = 150, int chunkOverlap = 20)
    {
        _chunkSize = chunkSize;
        _chunkOverlap = chunkOverlap;
    }

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

        int index = 0;
        int chunkIndex = 0;
        int step = _chunkSize - _chunkOverlap;

        while (index < words.Length)
        {
            var chunkWords = words.Skip(index).Take(_chunkSize);
            var chunkText = string.Join(" ", chunkWords);

            yield return new DocumentChunk(chunkText, fileName, chunkIndex, []);

            index += step;
            chunkIndex++;
        }
    }
}
