# Implementation Plan — MathCalc CLI (.NET 8)

## Overview

A .NET 8 CLI calculator with three concerns kept strictly separated:
- **Domain** — the four operations and all business rules (no I/O, no framework deps)
- **Application** — orchestrates a single calculation request; translates domain results into a response DTO
- **Cli** — entry point: parses args, runs REPL or single-shot mode, prints to console

No static "helper" classes. Every operation is an object that implements a shared interface, registered in a simple in-process registry. The Application layer never touches `Console`; the Cli layer never does math.

---

## Step 1 — Scaffold the solution and projects

Run these commands from `c:\_projects\0004. tests\003. AI\KIRO\001\MathCalc\`:

```powershell
# Create the solution
dotnet new sln -n MathCalc

# Source projects
dotnet new classlib -n MathCalc.Domain      -o src/MathCalc.Domain      -f net8.0
dotnet new classlib -n MathCalc.Application -o src/MathCalc.Application -f net8.0
dotnet new console  -n MathCalc.Cli         -o src/MathCalc.Cli         -f net8.0

# Test project
dotnet new xunit -n MathCalc.Tests -o tests/MathCalc.Tests -f net8.0

# Add all projects to the solution
dotnet sln add src/MathCalc.Domain/MathCalc.Domain.csproj
dotnet sln add src/MathCalc.Application/MathCalc.Application.csproj
dotnet sln add src/MathCalc.Cli/MathCalc.Cli.csproj
dotnet sln add tests/MathCalc.Tests/MathCalc.Tests.csproj

# Wire up project references
dotnet add src/MathCalc.Application/MathCalc.Application.csproj reference src/MathCalc.Domain/MathCalc.Domain.csproj
dotnet add src/MathCalc.Cli/MathCalc.Cli.csproj             reference src/MathCalc.Application/MathCalc.Application.csproj
dotnet add src/MathCalc.Cli/MathCalc.Cli.csproj             reference src/MathCalc.Domain/MathCalc.Domain.csproj

dotnet add tests/MathCalc.Tests/MathCalc.Tests.csproj reference src/MathCalc.Domain/MathCalc.Domain.csproj
dotnet add tests/MathCalc.Tests/MathCalc.Tests.csproj reference src/MathCalc.Application/MathCalc.Application.csproj

# Add FluentAssertions to the test project
dotnet add tests/MathCalc.Tests/MathCalc.Tests.csproj package FluentAssertions
```

Delete the boilerplate `Class1.cs` files generated in Domain and Application.

**Verify:** `dotnet build` from the solution root — all four projects compile with zero errors.

---

## Step 2 — Domain layer

### Files to create

| Path | Purpose |
|------|---------|
| `src/MathCalc.Domain/IOperation.cs` | Contract every math operation must satisfy |
| `src/MathCalc.Domain/CalculationResult.cs` | Discriminated-union–style result (success + value, or error + message) |
| `src/MathCalc.Domain/Operations/AddOperation.cs` | Addition |
| `src/MathCalc.Domain/Operations/SubtractOperation.cs` | Subtraction |
| `src/MathCalc.Domain/Operations/MultiplyOperation.cs` | Multiplication |
| `src/MathCalc.Domain/Operations/DivideOperation.cs` | Division (guards divide-by-zero) |
| `src/MathCalc.Domain/OperationRegistry.cs` | Maps string name → IOperation; rejects unknown names |

### Key signatures

```csharp
// IOperation.cs
namespace MathCalc.Domain;

public interface IOperation
{
    string Name { get; }          // "add", "subtract", "multiply", "divide"
    CalculationResult Execute(decimal left, decimal right);
}

// CalculationResult.cs
namespace MathCalc.Domain;

public sealed class CalculationResult
{
    public bool IsSuccess { get; }
    public decimal Value  { get; }   // valid only when IsSuccess == true
    public string Error   { get; }   // valid only when IsSuccess == false

    private CalculationResult(bool isSuccess, decimal value, string error) { ... }

    public static CalculationResult Success(decimal value) => new(true, value, string.Empty);
    public static CalculationResult Failure(string error)  => new(false, 0m, error);
}

// OperationRegistry.cs
namespace MathCalc.Domain;

public sealed class OperationRegistry
{
    private readonly Dictionary<string, IOperation> _operations;

    public OperationRegistry()   // registers all four operations in constructor
    {
        _operations = new(StringComparer.OrdinalIgnoreCase);
        Register(new Operations.AddOperation());
        Register(new Operations.SubtractOperation());
        Register(new Operations.MultiplyOperation());
        Register(new Operations.DivideOperation());
    }

    private void Register(IOperation op) => _operations[op.Name] = op;

    public bool TryGet(string name, out IOperation operation)
        => _operations.TryGetValue(name, out operation!);

    public IReadOnlyCollection<string> SupportedOperations => _operations.Keys;
}
```

DivideOperation.Execute must return `CalculationResult.Failure("Division by zero is not allowed.")` when `right == 0m` instead of throwing.

**Verify:** `dotnet build src/MathCalc.Domain/` — compiles cleanly.

---

## Step 3 — Application layer

### Files to create

| Path | Purpose |
|------|---------|
| `src/MathCalc.Application/CalculationRequest.cs` | Input DTO: operation name + two decimal operands |
| `src/MathCalc.Application/CalculationResponse.cs` | Output DTO: IsSuccess, Value, ErrorMessage |
| `src/MathCalc.Application/ICalculatorService.cs` | Service interface |
| `src/MathCalc.Application/CalculatorService.cs` | Resolves the operation via OperationRegistry, delegates, maps result → response |

### Key signatures

```csharp
// CalculationRequest.cs
namespace MathCalc.Application;

public sealed record CalculationRequest(string OperationName, decimal Left, decimal Right);

// CalculationResponse.cs
namespace MathCalc.Application;

public sealed record CalculationResponse(bool IsSuccess, decimal Value, string ErrorMessage);

// ICalculatorService.cs
namespace MathCalc.Application;

public interface ICalculatorService
{
    CalculationResponse Calculate(CalculationRequest request);
}

// CalculatorService.cs
namespace MathCalc.Application;
using MathCalc.Domain;

public sealed class CalculatorService : ICalculatorService
{
    private readonly OperationRegistry _registry;

    public CalculatorService(OperationRegistry registry) { _registry = registry; }

    public CalculationResponse Calculate(CalculationRequest request)
    {
        if (!_registry.TryGet(request.OperationName, out var operation))
            return new CalculationResponse(false, 0m,
                $"Unknown operation '{request.OperationName}'. Supported: {string.Join(", ", _registry.SupportedOperations)}");

        var result = operation.Execute(request.Left, request.Right);
        return result.IsSuccess
            ? new CalculationResponse(true, result.Value, string.Empty)
            : new CalculationResponse(false, 0m, result.Error);
    }
}
```

**Verify:** `dotnet build src/MathCalc.Application/` — compiles cleanly.

---

## Step 4 — CLI layer

### Files to create

| Path | Purpose |
|------|---------|
| `src/MathCalc.Cli/Program.cs` | Entry point; routes to single-shot or REPL |
| `src/MathCalc.Cli/CommandLineParser.cs` | Parses `string[]` args into a `ParseResult` |
| `src/MathCalc.Cli/ParseResult.cs` | Discriminated union: SingleShot(request), Repl, Help, Error(message) |
| `src/MathCalc.Cli/ReplRunner.cs` | Interactive loop; reads lines, prints results, exits on `exit`/`quit`/Ctrl-C |
| `src/MathCalc.Cli/HelpText.cs` | Returns the multi-line help string |

### Key signatures

```csharp
// ParseResult.cs
namespace MathCalc.Cli;
using MathCalc.Application;

public abstract record ParseResult
{
    public sealed record SingleShot(CalculationRequest Request) : ParseResult;
    public sealed record Repl : ParseResult;
    public sealed record Help : ParseResult;
    public sealed record Error(string Message) : ParseResult;
}

// CommandLineParser.cs
namespace MathCalc.Cli;
using MathCalc.Application;

public static class CommandLineParser
{
    // Returns ParseResult.Help   when args contains "--help" or "-h"
    // Returns ParseResult.Repl   when args is empty
    // Returns ParseResult.Error  when arg count is wrong, operands aren't decimal,
    //                             or operation name is not a letter-only word
    // Returns ParseResult.SingleShot otherwise
    public static ParseResult Parse(string[] args) { ... }
}

// HelpText.cs
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
```

### Program.cs logic (pseudo-code)

```csharp
var registry = new OperationRegistry();
var service  = new CalculatorService(registry);

switch (CommandLineParser.Parse(args))
{
    case ParseResult.Help:
        Console.WriteLine(HelpText.Get());
        break;

    case ParseResult.Error e:
        Console.Error.WriteLine($"Error: {e.Message}");
        Console.Error.WriteLine("Run with --help for usage.");
        return 1;    // non-zero exit code

    case ParseResult.SingleShot s:
        var resp = service.Calculate(s.Request);
        if (resp.IsSuccess)
            Console.WriteLine($"Result: {resp.Value}");
        else
        {
            Console.Error.WriteLine($"Error: {resp.ErrorMessage}");
            return 1;
        }
        break;

    case ParseResult.Repl:
        new ReplRunner(service).Run();
        break;
}
return 0;
```

### ReplRunner loop

- Print banner: `"MathCalc REPL — type <op> <left> <right>, or 'exit' to quit."`
- Prompt: `"> "`
- On each line: split by whitespace → try Parse as single-shot args → print result or error
- Exit cleanly on `exit`, `quit`, empty line (EOF), or Ctrl-C

**Verify:** `dotnet build src/MathCalc.Cli/` — compiles cleanly.

---

## Step 5 — Unit tests

### File to create

`tests/MathCalc.Tests/OperationsTests.cs` — tests for Domain operations  
`tests/MathCalc.Tests/CalculatorServiceTests.cs` — tests for Application service  
`tests/MathCalc.Tests/CommandLineParserTests.cs` — tests for CLI parser (no console I/O)

### Test cases

#### OperationsTests.cs

| Test name | Scenario | Expected |
|-----------|----------|----------|
| `Add_TwoPositiveDecimals_ReturnsSum` | 10.5 + 4.5 | 15.0 |
| `Add_NegativeNumbers_ReturnsCorrectSum` | -3 + -7 | -10 |
| `Subtract_ReturnsCorrectDifference` | 10 - 3 | 7 |
| `Subtract_ResultIsNegative` | 3 - 10 | -7 |
| `Multiply_TwoDecimals_ReturnsProduct` | 2.5 * 4 | 10 |
| `Multiply_ByZero_ReturnsZero` | 99 * 0 | 0 |
| `Divide_ExactDivision_ReturnsQuotient` | 9 / 3 | 3 |
| `Divide_DecimalResult_ReturnsDecimal` | 10 / 3 | 3.333...recurring |
| `Divide_ByZero_ReturnsFailure` | 5 / 0 | IsSuccess==false, error contains "zero" |
| `Divide_NegativeDividend_ReturnsNegative` | -10 / 2 | -5 |
| `Multiply_LargeNumbers_NoOverflow` | 1e14m * 1e14m | 1e28m |

#### CalculatorServiceTests.cs

| Test name | Scenario | Expected |
|-----------|----------|----------|
| `Calculate_ValidAdd_ReturnsSuccessResponse` | "add", 3, 4 | IsSuccess, Value==7 |
| `Calculate_UnknownOperation_ReturnsFailure` | "power", 2, 3 | !IsSuccess, message mentions "power" |
| `Calculate_DivideByZero_ReturnsFailure` | "divide", 5, 0 | !IsSuccess |
| `Calculate_OperationNameCaseInsensitive` | "ADD", 1, 1 | IsSuccess, Value==2 |

#### CommandLineParserTests.cs

| Test name | Scenario | Expected |
|-----------|----------|----------|
| `Parse_NoArgs_ReturnsRepl` | `[]` | ParseResult.Repl |
| `Parse_HelpFlag_ReturnsHelp` | `["--help"]` | ParseResult.Help |
| `Parse_ShortHelpFlag_ReturnsHelp` | `["-h"]` | ParseResult.Help |
| `Parse_ValidArgs_ReturnsSingleShot` | `["add","10","5"]` | SingleShot, Left=10, Right=5 |
| `Parse_DecimalArgs_ReturnsSingleShot` | `["multiply","2.5","4"]` | SingleShot, Left=2.5m |
| `Parse_TooFewArgs_ReturnsError` | `["add","10"]` | ParseResult.Error |
| `Parse_NonNumericOperand_ReturnsError` | `["add","abc","5"]` | ParseResult.Error |
| `Parse_TooManyArgs_ReturnsError` | `["add","1","2","3"]` | ParseResult.Error |

**Verify:** `dotnet test` from solution root — all tests pass, output shows `Passed!`.

---

## Step 6 — Full solution verification

Run these commands in order from `c:\_projects\0004. tests\003. AI\KIRO\001\MathCalc\`:

```powershell
# Clean build
dotnet build --configuration Release

# All tests pass
dotnet test --configuration Release --logger "console;verbosity=normal"

# Single-shot mode
dotnet run --project src/MathCalc.Cli -- add 10 5
# Expected output:  Result: 15

dotnet run --project src/MathCalc.Cli -- divide 9.9 3
# Expected output:  Result: 3.3

dotnet run --project src/MathCalc.Cli -- divide 5 0
# Expected output (stderr):  Error: Division by zero is not allowed.
# Expected exit code: 1

dotnet run --project src/MathCalc.Cli -- --help
# Expected output: usage text

dotnet run --project src/MathCalc.Cli -- badop 1 2
# Expected output (stderr):  Error: Unknown operation 'badop'...
# Expected exit code: 1

dotnet run --project src/MathCalc.Cli -- add abc 2
# Expected output (stderr):  Error: ...not a valid number...
# Expected exit code: 1

# REPL mode — launch, type a line, verify output, type 'exit'
dotnet run --project src/MathCalc.Cli
```

---

## File tree summary

```
MathCalc/
├── MathCalc.sln
├── .agents/
│   └── tasks/
│       └── plan.md
├── src/
│   ├── MathCalc.Domain/
│   │   ├── MathCalc.Domain.csproj
│   │   ├── IOperation.cs
│   │   ├── CalculationResult.cs
│   │   ├── OperationRegistry.cs
│   │   └── Operations/
│   │       ├── AddOperation.cs
│   │       ├── SubtractOperation.cs
│   │       ├── MultiplyOperation.cs
│   │       └── DivideOperation.cs
│   ├── MathCalc.Application/
│   │   ├── MathCalc.Application.csproj
│   │   ├── CalculationRequest.cs
│   │   ├── CalculationResponse.cs
│   │   ├── ICalculatorService.cs
│   │   └── CalculatorService.cs
│   └── MathCalc.Cli/
│       ├── MathCalc.Cli.csproj
│       ├── Program.cs
│       ├── ParseResult.cs
│       ├── CommandLineParser.cs
│       ├── ReplRunner.cs
│       └── HelpText.cs
└── tests/
    └── MathCalc.Tests/
        ├── MathCalc.Tests.csproj
        ├── OperationsTests.cs
        ├── CalculatorServiceTests.cs
        └── CommandLineParserTests.cs
```
