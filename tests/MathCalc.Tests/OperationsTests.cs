namespace MathCalc.Tests;

using FluentAssertions;
using MathCalc.Domain.Operations;
using Xunit;

public class OperationsTests
{
    [Fact]
    public void Add_TwoPositiveDecimals_ReturnsSum()
    {
        var op = new AddOperation();
        var result = op.Execute(10.5m, 4.5m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(15.0m);
    }

    [Fact]
    public void Add_NegativeNumbers_ReturnsCorrectSum()
    {
        var op = new AddOperation();
        var result = op.Execute(-3m, -7m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-10m);
    }

    [Fact]
    public void Subtract_ReturnsCorrectDifference()
    {
        var op = new SubtractOperation();
        var result = op.Execute(10m, 3m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7m);
    }

    [Fact]
    public void Subtract_ResultIsNegative()
    {
        var op = new SubtractOperation();
        var result = op.Execute(3m, 10m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-7m);
    }

    [Fact]
    public void Multiply_TwoDecimals_ReturnsProduct()
    {
        var op = new MultiplyOperation();
        var result = op.Execute(2.5m, 4m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10m);
    }

    [Fact]
    public void Multiply_ByZero_ReturnsZero()
    {
        var op = new MultiplyOperation();
        var result = op.Execute(99m, 0m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0m);
    }

    [Fact]
    public void Divide_ExactDivision_ReturnsQuotient()
    {
        var op = new DivideOperation();
        var result = op.Execute(9m, 3m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(3m);
    }

    [Fact]
    public void Divide_DecimalResult_ReturnsDecimal()
    {
        var op = new DivideOperation();
        var result = op.Execute(10m, 3m);
        result.IsSuccess.Should().BeTrue();
        // 10/3 = 3.3333... (28 decimal places for decimal type)
        result.Value.Should().BeApproximately(3.3333m, 0.0001m);
    }

    [Fact]
    public void Divide_ByZero_ReturnsFailure()
    {
        var op = new DivideOperation();
        var result = op.Execute(5m, 0m);
        result.IsSuccess.Should().BeFalse();
        result.Error.ToLower().Should().Contain("zero");
    }

    [Fact]
    public void Divide_NegativeDividend_ReturnsNegative()
    {
        var op = new DivideOperation();
        var result = op.Execute(-10m, 2m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-5m);
    }

    [Fact]
    public void Multiply_LargeNumbers_NoOverflow()
    {
        var op = new MultiplyOperation();
        var result = op.Execute(1e14m, 1e14m);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1e28m);
    }
}
