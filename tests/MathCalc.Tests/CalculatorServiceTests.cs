namespace MathCalc.Tests;

using FluentAssertions;
using MathCalc.Application;
using MathCalc.Domain;
using Xunit;

public class CalculatorServiceTests
{
    private readonly CalculatorService _service;

    public CalculatorServiceTests()
    {
        _service = new CalculatorService(new OperationRegistry());
    }

    [Fact]
    public void Calculate_ValidAdd_ReturnsSuccessResponse()
    {
        var response = _service.Calculate(new CalculationRequest("add", 3m, 4m));
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().Be(7m);
    }

    [Fact]
    public void Calculate_UnknownOperation_ReturnsFailure()
    {
        var response = _service.Calculate(new CalculationRequest("power", 2m, 3m));
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("power");
    }

    [Fact]
    public void Calculate_DivideByZero_ReturnsFailure()
    {
        var response = _service.Calculate(new CalculationRequest("divide", 5m, 0m));
        response.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Calculate_OperationNameCaseInsensitive()
    {
        var response = _service.Calculate(new CalculationRequest("ADD", 1m, 1m));
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().Be(2m);
    }
}
