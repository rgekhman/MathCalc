namespace MathCalc.Cli;

using MathCalc.Application;

public static class CommandLineParser
{
    public static ParseResult Parse(string[] args)
    {
        if (args.Length == 0)
            return new ParseResult.Repl();

        if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
            return new ParseResult.Help();

        if (args.Length != 3)
            return new ParseResult.Error(
                $"Expected 3 arguments: <operation> <left> <right>, but got {args.Length}.");

        var operationName = args[0];
        if (!System.Text.RegularExpressions.Regex.IsMatch(operationName, @"^[a-zA-Z]+$"))
            return new ParseResult.Error(
                $"Operation name '{operationName}' is invalid. It must contain only letters.");

        if (!decimal.TryParse(args[1], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal left))
            return new ParseResult.Error(
                $"'{args[1]}' is not a valid number for the left operand.");

        if (!decimal.TryParse(args[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal right))
            return new ParseResult.Error(
                $"'{args[2]}' is not a valid number for the right operand.");

        return new ParseResult.SingleShot(new CalculationRequest(operationName, left, right));
    }
}
