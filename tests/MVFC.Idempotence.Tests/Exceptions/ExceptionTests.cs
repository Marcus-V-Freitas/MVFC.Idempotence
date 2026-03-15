namespace MVFC.Idempotence.Tests.Exceptions;

public sealed class ExceptionTests
{
    [Fact]
    public void Constructor_WithMessageAndStatusCode_SetsProperties()
    {
        // Act
        var ex = new IdempotencyException("Test message", 400);

        // Assert
        ex.Message.Should().Be("Test message");
        ex.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Constructor_WithStatusCodeOnly_SetsDefaultMessage()
    {
        // Act
        var ex = new IdempotencyException("Error", 500);

        // Assert
        ex.Message.Should().Be("Error");
        ex.StatusCode.Should().Be(500);
    }
}
