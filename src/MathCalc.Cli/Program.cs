using MathCalc.Application;
using MathCalc.Cli;
using MathCalc.Domain;

var registry = new OperationRegistry();
var service  = new CalculatorService(registry);

switch (CommandLineParser.Parse(args))
{
    case ParseResult.Help:
        Console.WriteLine(HelpText.Get());
        return 0;

    case ParseResult.Error e:
        Console.Error.WriteLine($"Error: {e.Message}");
        Console.Error.WriteLine("Run with --help for usage.");
        return 1;

    case ParseResult.SingleShot s:
        var resp = service.Calculate(s.Request);
        if (resp.IsSuccess)
        {
            Console.WriteLine($"Result: {resp.Value}");
            return 0;
        }
        else
        {
            Console.Error.WriteLine($"Error: {resp.ErrorMessage}");
            return 1;
        }

    case ParseResult.Repl:
        new ReplRunner(service).Run();
        return 0;
}

return 0;
