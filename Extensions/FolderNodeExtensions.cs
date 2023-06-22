using ConsoleApp5.Data;

namespace ConsoleApp5.Extensions;

internal static class FolderNodeExtensions
{
    internal static IEnumerable<(int indent, FolderNode node)> TraverseNodes(this FolderNode node, int maxDepth = int.MaxValue, int indent = 0)
    {
        if (indent < maxDepth - 1)
        {
            yield return (indent, node);

            var nestedNodes =
                node
                    .Nested
                    .SelectMany(r => r.TraverseNodes(maxDepth, indent + 1));

            foreach (var nested in nestedNodes)
            {
                yield return nested;
            }
        }
    }
}