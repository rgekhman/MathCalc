namespace MathCalc.Cli;

using MathCalc.Application;

public sealed class ReplRunner
{
    private readonly ICalculatorService _service;

    public ReplRunner(ICalculatorService service)
    {
        _service = service;
    }

    public void Run()
    {
        Console.WriteLine("MathCalc REPL — type <op> <left> <right>, or 'exit' to quit.");

        while (true)
        {
            Console.Write("> ");

            string? line;
            try
            {
                line = Console.ReadLine();
            }
            catch (Exception)
            {
                break;
            }

            if (line is null)
                break;

            line = line.Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var parseResult = CommandLineParser.Parse(parts);

            switch (parseResult)
            {
                case ParseResult.SingleShot s:
                    var response = _service.Calculate(s.Request);
                    if (response.IsSuccess)
                        Console.WriteLine($"Result: {response.Value}");
                    else
                        Console.WriteLine($"Error: {response.ErrorMessage}");
                    break;

                case ParseResult.Error e:
                    Console.WriteLine($"Error: {e.Message}");
                    break;

                case ParseResult.Help:
                    Console.WriteLine(HelpText.Get());
                    break;

                case ParseResult.Repl:
                    // empty args inside REPL — just prompt again
                    break;
            }
        }
    }
}
