using DesignDocs.Api.Entities;
using System.Security.Cryptography;
using System.Text;

namespace DesignDocs.Api.Services;

public class AnchorService
{
    public string ComputeHash(string text)
    {
        using var sha = SHA1.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }

    public bool VerifyAnchor(CommentAnchor anchor, string blockText)
    {
        var hash = ComputeHash(blockText);
        if (hash == anchor.BlockHash)
            return true;
        if (blockText.Contains(anchor.TextQuote))
            return true;
        return false;
    }
}
