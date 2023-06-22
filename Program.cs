using ConsoleApp5.Data;
using ConsoleApp5.Extensions;
#pragma warning disable CS8321

const string rootFolder = @"C:\Code\YourHeavyRepository";

const int maxDepth = 6;
const int minLinesCountToUnfoldNode = 80_000;
const int minLinesCountToShowNode = 5_000;
const int maxNestedNodes = 10;

var fileExtensionsToLookup = new List<string>
{
    "*.cs",
    //"*.vb",
    //"*.js",
    //"*.css",
    //"*.ps1",
    //"*.sql"
};

Dictionary<string, int> fileLinesCountCache = new();

int GetFileLinesCount(string filePath) =>
    fileLinesCountCache.TryGetValue(filePath, out int count)
        ? count
        : fileLinesCountCache[filePath] = File.ReadAllLines(filePath).Length;

IEnumerable<FolderNode> GetFolderLinesCountNodes(string targetFolder, List<string> fileExtensions) =>
    fileExtensions
        .SelectMany(fe => Directory
            .EnumerateFiles(
                targetFolder,
                fe,
                SearchOption.AllDirectories))
        .Select(f => (
            filePath: f,
            linesCount: GetFileLinesCount(f)
        ))
        .Select(o => (
            o.linesCount,
            folder: Path
                .GetDirectoryName(o.filePath)
                !.Replace(targetFolder, "")
                .Trim('\\')
                .Split('\\')[0]
        ))
        .GroupBy(o => o.folder)
        .Select(g => (
            folder: g.Key,
            linesCount: g.Select(o => o.linesCount).Sum()
        ))
        .Where(o => o.linesCount > minLinesCountToShowNode)
        .Select(o => (
            folder:
                string.IsNullOrEmpty(o.folder)
                    ? "__self"
                    : o.folder,
            o.linesCount,
            nested:
                string.IsNullOrEmpty(o.folder) || o.linesCount < minLinesCountToUnfoldNode
                    ? Array.Empty<FolderNode>()
                    : GetFolderLinesCountNodes(Path.Combine(targetFolder, o.folder), fileExtensions),
            extensions: 
                GetFileExtensionsStat(Path.Combine(targetFolder, o.folder), fileExtensions)
        ))
        .OrderByDescending(o => o.linesCount)
        .Take(maxNestedNodes)
        .Select(o => new FolderNode(o));

string GetTreeReportLine((int indent, FolderNode node) nodeInfo)
{
    string indentationPrefix =
        Enumerable
            .Range(0, nodeInfo.indent)
            .Select(_ => " |")
            .JoinAsString();

    return $"{indentationPrefix}-{nodeInfo.node.Folder} ({nodeInfo.node.LinesCount.AsCount()})";
}

string GetMdReportLine((int indent, FolderNode node) nodeInfo)
{
    var extensionsStat = nodeInfo.node.Extensions.ToDictionary(o => $"*{o.Extension}");

    string extensionsReport =
        fileExtensionsToLookup
            .Select(fe => 
                extensionsStat.TryGetValue(fe, out var val) 
                    ? $"{val.Size.AsSize()}" 
                    : "")
            .JoinAsString("|");

    string indentationPrefix =
        Enumerable
            .Range(0, nodeInfo.indent)
            .Select(_ => "| ")
            .JoinAsString();
    
    string indentationSuffix =
        Enumerable
            .Range(0, maxDepth - nodeInfo.indent - 2)
            .Select(_ => "| ")
            .JoinAsString();

    return $"| {indentationPrefix}|{nodeInfo.node.Folder}| {indentationSuffix}| {nodeInfo.node.LinesCount.AsCount()} |{extensionsReport}|";
}

IEnumerable<ExtensionInfo> GetFileExtensionsStat(string folder, List<string> extensionsToLookup = null) =>
    (extensionsToLookup == null 
        ? Directory
            .EnumerateFiles(
                folder,
                "*.*",
                SearchOption.AllDirectories)
        : extensionsToLookup
            .SelectMany(fe => Directory
                .EnumerateFiles(
                    folder,
                    fe,
                    SearchOption.AllDirectories)))
        .Select(s => (
            extension: Path.GetExtension(s),
            size: new FileInfo(s).Length
        ))
        .GroupBy(o => o.extension)
        .Select(g => (
            extension: g.Key,
            size: g.Select(o => o.size).Sum()
        ))
        .OrderBy(o => o.extension)
        .Select(o => new ExtensionInfo(o));


Console.WriteLine($"Start lines of code statistics calculation for files with ({fileExtensionsToLookup.JoinAsString(", ")}) extensions in '{rootFolder}' folder...\n");

Console.WriteLine($"Folders having less than {minLinesCountToUnfoldNode.AsCount()} total lines of code aren't unfold.");
Console.WriteLine($"Folders having less than {minLinesCountToShowNode.AsCount()} total lines of code aren't shown.\n");

const string reportMd = "report.md";

using FileStream stream = File.Create(reportMd);
using StreamWriter streamWriter = new(stream);

IEnumerable<(int indent, FolderNode node)> nodes = 
    GetFolderLinesCountNodes(rootFolder, fileExtensionsToLookup)
        .SelectMany(o => o.TraverseNodes(maxDepth));

void WriteMdHeader()
{
    string columnNames =
        Enumerable
            .Range(0, maxDepth + 1)
            .Select(_ => ".") // empty column names before the Lines count
            .Concat("Lines count")
            .Concat(
                fileExtensionsToLookup
                    .Select(fe => $"\\{fe}")) // escape * symbol
            .JoinAsString("|");
    streamWriter.WriteLine($"|{columnNames}|");

    string delimiter =
        Enumerable
            .Range(0, maxDepth + 2 + fileExtensionsToLookup.Count)
            .Select(_ => " - ")
            .JoinAsString("|");
    streamWriter.WriteLine($"|{delimiter}|");
}

WriteMdHeader();

foreach ((int indent, FolderNode node) node in nodes)
{
    // write both tree and MD reports in a single pass
    streamWriter.WriteLine(GetMdReportLine(node));
    Console.WriteLine(GetTreeReportLine(node));
}

Console.WriteLine($"Done. See also '{reportMd}' file.");

/*/

// file extensions report
const long minFileSize = 1000_000;

Console.WriteLine("Start file extension statistics calculation.");

var allFileExtensions = GetFileExtensionsStat(rootFolder);

string fileExtensionsReport =
    allFileExtensions
        .Where(o => o.Size > minFileSize)
        .Select(o => $"{o.Extension}({o.Size.AsSize()})")
        .JoinAsString("\n");

Console.WriteLine($"File extensions (>{minFileSize.AsSize()} per extension):\n{fileExtensionsReport}");
//*/