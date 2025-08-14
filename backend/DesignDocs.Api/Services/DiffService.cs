using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DesignDocs.Api.Services;

public class DiffService
{
    public SideBySideDiffModel Diff(string oldText, string newText)
    {
        var builder = new SideBySideDiffBuilder(new Differ());
        return builder.BuildDiffModel(oldText, newText);
    }
}
