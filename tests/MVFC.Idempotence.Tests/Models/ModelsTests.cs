namespace MVFC.Idempotence.Tests.Models;

public sealed class ModelsTests
{
    [Theory]
    [InlineData(199, false)]
    [InlineData(200, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    public void CachedModel_IsSuccess_BasedOnStatusCode(int statusCode, bool expected)
    {
        // Act
        var model = new CachedModel<string>(statusCode, "payload");

        // Assert
        model.IsSuccess.Should().Be(expected);
        model.IsFailure.Should().Be(!expected);
    }

    [Theory]
    [InlineData(199, false)]
    [InlineData(200, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    public void CachedResult_IsSuccess_BasedOnStatusCode(int statusCode, bool expected)
    {
        // Act
        var result = new CachedResult(statusCode, [1]);

        // Assert
        result.IsSuccess.Should().Be(expected);
        result.IsFailure.Should().Be(!expected);
    }
}
