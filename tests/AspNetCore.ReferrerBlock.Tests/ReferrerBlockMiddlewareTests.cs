using ReferrerBlock.Configuration;
using ReferrerBlock.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace AspNetCore.ReferrerBlock.Tests;

[TestClass]
public class ReferrerBlockMiddlewareTests
{
    private Mock<RequestDelegate> _nextMock;
    private Mock<ILogger<ReferrerBlockMiddleware>> _loggerMock;
    private ReferrerBlockOptions _options;
    private DefaultHttpContext _httpContext;

    [TestInitialize]
    public void Setup()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ReferrerBlockMiddleware>>();
        _options = new ReferrerBlockOptions();
        _httpContext = new DefaultHttpContext();
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        _httpContext.Request.Path = "/test";
    }

    #region Tests sans Referer

    [TestMethod]
    public async Task InvokeAsync_NoReferer_ShouldCallNext()
    {
        // Arrange
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_EmptyReferer_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = string.Empty;
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    #endregion

    #region Tests avec Referer valide (non bloqué)

    [TestMethod]
    public async Task InvokeAsync_ValidReferer_NotBlocked_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://www.google.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        Assert.AreNotEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_ValidReferer_AllowedTLD_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://example.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    #endregion

    #region Tests TLD bloqués

    [TestMethod]
    public async Task InvokeAsync_BlockedTLD_ICU_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://spam.icu";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
        _nextMock.Verify(next => next(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedTLD_XYZ_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://malicious.xyz";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
        _nextMock.Verify(next => next(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedTLD_BizId_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://spam.biz.id";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedTLD_CaseInsensitive_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://SPAM.ICU";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    #endregion

    #region Tests domaines bloqués

    [TestMethod]
    public async Task InvokeAsync_BlockedDomain_Exact_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://gazdp.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
        _nextMock.Verify(next => next(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedDomain_Subdomain_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://www.gazdp.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedDomain_DeepSubdomain_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://sub.example.gazdp.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedDomain_CaseInsensitive_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://GAZDP.COM";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    #endregion

    #region Tests patterns bloqués

    [TestMethod]
    public async Task InvokeAsync_BlockedPattern_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://ctysss.example.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
        _nextMock.Verify(next => next(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedPattern_InMiddle_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://shop-ctysss-online.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedPattern_CaseInsensitive_ShouldReturn410()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://CTYSSS.example.com";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, _httpContext.Response.StatusCode);
    }

    #endregion

    #region Tests d'erreurs

    [TestMethod]
    public async Task InvokeAsync_MalformedReferer_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "not-a-valid-uri";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Malformed referrer detected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de logging

    [TestMethod]
    public async Task InvokeAsync_BlockedReferer_ShouldLogWarning()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://spam.icu";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Referrer spam blocked")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de réponse

    [TestMethod]
    public async Task InvokeAsync_BlockedReferer_ShouldReturnHTMLResponse()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://spam.icu";
        _httpContext.Response.Body = new MemoryStream();
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        Assert.IsTrue(responseBody.Contains("<!DOCTYPE html>"));
        Assert.IsTrue(responseBody.Contains("<title>Gone</title>"));
    }

    [TestMethod]
    public async Task InvokeAsync_BlockedReferer_ShouldIncludeDelay()
    {
        // Arrange
        _httpContext.Request.Headers.Referer = "https://spam.icu";
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await middleware.InvokeAsync(_httpContext);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 100, "Le délai devrait être d'au moins 100ms");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 600, "Le délai ne devrait pas dépasser 500ms + marge");
    }

    #endregion

    #region Tests de scénarios réels

    [TestMethod]
    public async Task InvokeAsync_MultipleBlockedDomains_ShouldBlockAll()
    {
        // Arrange
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);
        var blockedUrls = new[]
        {
            "https://gazdp.com",
            "https://spam.icu",
            "https://ctysss-shop.com"
        };

        foreach (var url in blockedUrls)
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Referer = url;
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.AreEqual(StatusCodes.Status410Gone, context.Response.StatusCode, $"Failed for URL: {url}");
        }
    }

    [TestMethod]
    public async Task InvokeAsync_HTTPAndHTTPS_BothShouldBeBlocked()
    {
        // Arrange
        var middleware = new ReferrerBlockMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Referer = "http://spam.icu";
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var httpsContext = new DefaultHttpContext();
        httpsContext.Request.Headers.Referer = "https://spam.icu";
        httpsContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        await middleware.InvokeAsync(httpContext);
        await middleware.InvokeAsync(httpsContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status410Gone, httpContext.Response.StatusCode);
        Assert.AreEqual(StatusCodes.Status410Gone, httpsContext.Response.StatusCode);
    }

    #endregion
}