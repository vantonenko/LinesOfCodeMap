namespace ConsoleApp5.Data;

internal class FolderNode
{
    public FolderNode((string folder, int linesCount, IEnumerable<FolderNode> nested, IEnumerable<ExtensionInfo> extensions) o)
    {
        Folder = o.folder;
        LinesCount = o.linesCount;
        Nested = o.nested;
        Extensions = o.extensions;
    }


    public string Folder { get; }
    public int LinesCount { get; }
    public IEnumerable<FolderNode> Nested { get; }
    public IEnumerable<ExtensionInfo> Extensions { get; }
}