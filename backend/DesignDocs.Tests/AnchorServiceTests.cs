using DesignDocs.Api.Entities;
using DesignDocs.Api.Services;
using Xunit;

namespace DesignDocs.Tests;

public class AnchorServiceTests
{
    [Fact]
    public void VerifyAnchor_HashMatch_ReturnsTrue()
    {
        var svc = new AnchorService();
        var text = "hello world";
        var anchor = new CommentAnchor { BlockHash = svc.ComputeHash(text), TextQuote = "hello" };
        Assert.True(svc.VerifyAnchor(anchor, text));
    }

    [Fact]
    public void VerifyAnchor_TextNotFound_ReturnsFalse()
    {
        var svc = new AnchorService();
        var text = "hello world";
        var anchor = new CommentAnchor { BlockHash = "bad", TextQuote = "nope" };
        Assert.False(svc.VerifyAnchor(anchor, text));
    }
}
