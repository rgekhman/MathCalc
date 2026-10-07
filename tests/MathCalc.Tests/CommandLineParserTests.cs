namespace MathCalc.Tests;

using FluentAssertions;
using MathCalc.Cli;
using Xunit;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_NoArgs_ReturnsRepl()
    {
        var result = CommandLineParser.Parse([]);
        result.Should().BeOfType<ParseResult.Repl>();
    }

    [Fact]
    public void Parse_HelpFlag_ReturnsHelp()
    {
        var result = CommandLineParser.Parse(["--help"]);
        result.Should().BeOfType<ParseResult.Help>();
    }

    [Fact]
    public void Parse_ShortHelpFlag_ReturnsHelp()
    {
        var result = CommandLineParser.Parse(["-h"]);
        result.Should().BeOfType<ParseResult.Help>();
    }

    [Fact]
    public void Parse_ValidArgs_ReturnsSingleShot()
    {
        var result = CommandLineParser.Parse(["add", "10", "5"]);
        result.Should().BeOfType<ParseResult.SingleShot>()
            .Which.Request.Should().Match<MathCalc.Application.CalculationRequest>(
                r => r.OperationName == "add" && r.Left == 10m && r.Right == 5m);
    }

    [Fact]
    public void Parse_DecimalArgs_ReturnsSingleShot()
    {
        var result = CommandLineParser.Parse(["multiply", "2.5", "4"]);
        result.Should().BeOfType<ParseResult.SingleShot>()
            .Which.Request.Left.Should().Be(2.5m);
    }

    [Fact]
    public void Parse_TooFewArgs_ReturnsError()
    {
        var result = CommandLineParser.Parse(["add", "10"]);
        result.Should().BeOfType<ParseResult.Error>();
    }

    [Fact]
    public void Parse_NonNumericOperand_ReturnsError()
    {
        var result = CommandLineParser.Parse(["add", "abc", "5"]);
        result.Should().BeOfType<ParseResult.Error>();
    }

    [Fact]
    public void Parse_TooManyArgs_ReturnsError()
    {
        var result = CommandLineParser.Parse(["add", "1", "2", "3"]);
        result.Should().BeOfType<ParseResult.Error>();
    }
}
