namespace ConsoleApp5.Data;

public class ExtensionInfo
{
    public long Size { get; }
    public string Extension { get; }

    public ExtensionInfo((string extension, long size) o)
    {
        Extension = o.extension;
        Size = o.size;
    }
}