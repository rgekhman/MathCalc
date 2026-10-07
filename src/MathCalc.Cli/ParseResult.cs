namespace MathCalc.Cli;

using MathCalc.Application;

public abstract record ParseResult
{
    public sealed record SingleShot(CalculationRequest Request) : ParseResult;
    public sealed record Repl : ParseResult;
    public sealed record Help : ParseResult;
    public sealed record Error(string Message) : ParseResult;
}
