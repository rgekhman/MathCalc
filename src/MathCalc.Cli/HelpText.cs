namespace MathCalc.Cli;

public static class HelpText
{
    public static string Get() => """
        MathCalc — simple math calculator

        Usage:
          mathcalc <operation> <left> <right>   Single calculation
          mathcalc                               Interactive REPL mode
          mathcalc --help | -h                  Show this help

        Operations: add, subtract, multiply, divide

        Examples:
          mathcalc add 10 5          → Result: 15
          mathcalc divide 9.9 3      → Result: 3.3
          mathcalc divide 5 0        → Error: Division by zero is not allowed.
        """;
}
