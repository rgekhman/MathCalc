# MathCalc .NET 8 CLI Calculator

This change delivers a layered CLI calculator with four arithmetic operations, a single-shot invocation mode, a REPL, and help text. The architecture follows a strict three-tier boundary: Domain owns all math and result types, Application orchestrates via `CalculatorService`, and the CLI layer handles all I/O. The implementation matches the plan precisely and all 23 tests pass clean against a Release build.

Watch for: the `ReplRunner` swallows errors from `Console.ReadLine` via a bare `catch (Exception)` rather than catching the specific `IOException` that signals a closed stdin — confirmed. The `SupportedOperations` collection on `OperationRegistry` is backed by `Dictionary.Keys`, which has no guaranteed order, so the error message listing supported ops is non-deterministic — confirmed.

**Verdict**: APPROVED

---

## High-level view

The three-layer boundary holds without exception. Domain has no I/O, Application has no console touches, and the CLI layer contains no math. `CommandLineParser` is a `static` class, which is appropriate here because it is a pure function with no state — this is the one place a static class is used and it carries no behavioral risk.

The `OperationRegistry` constructs and wires all four operations in its constructor, giving the CLI a simple two-line bootstrap with no DI container. The `ICalculatorService` interface on the Application layer means the REPL takes the interface rather than the concrete class, keeping that path testable in isolation.

Two small issues exist: the bare exception catch in `ReplRunner` is broader than needed, and the dictionary-key ordering for the supported-ops error message is unspecified. Neither is blocking for a CLI tool at this scope, but they are noted below.

The 23-test suite covers all four operations, key edge cases (division by zero, negatives, decimals, large numbers), and the parser's branching logic. No `Console` calls appear anywhere in the test files.

---

<details>
<summary>Issues (2)</summary>

1. **Broad exception catch in ReplRunner** — `catch (Exception)` on `Console.ReadLine` will silently swallow unexpected errors beyond a closed stdin. Replace with `catch (IOException)` to let genuine programming errors surface.
2. **Non-deterministic supported-ops ordering** — `OperationRegistry.SupportedOperations` returns `Dictionary.Keys`, which has no defined iteration order. The unknown-operation error message lists supported ops from this collection, so the order can vary between runs. If consistency matters (e.g., for snapshot tests or user-facing reproducibility), switch to a `SortedDictionary` or materialize a sorted list.

</details>

---

<details>
<summary>Details</summary>

## Broad exception catch on Console.ReadLine

In `ReplRunner.Run`, the `Console.ReadLine()` call is wrapped in a `try/catch (Exception)` that breaks the loop on any exception. The intent is clearly to handle the case where stdin is closed (e.g., piped input reaching EOF). The correct exception type for that scenario is `IOException`. As written, if a bug elsewhere — a `NullReferenceException`, an `OutOfMemoryException` — happens to propagate up through this path, the REPL will silently exit rather than crash with a visible stack trace. The fix is one word: `catch (IOException)`.

## Non-deterministic supported-ops error message

`OperationRegistry.SupportedOperations` is `Dictionary<string, IOperation>.Keys`. `Dictionary<TKey, TValue>` in .NET makes no ordering guarantees on its key collection. The error message for an unknown operation assembles the list from this collection:

```
Unknown operation 'power'. Supported: add, subtract, multiply, divide
```

The four operation names will appear in some order, but it isn't guaranteed to be the insertion order across all runtimes, GC states, or future .NET versions. For a human-facing error this is a minor cosmetic issue; for snapshot-tested output it would be a flaky failure. Switching `_operations` to `SortedDictionary<string, IOperation>` would pin the order alphabetically at zero cost.

## Layer boundaries

Domain contains only `IOperation`, `CalculationResult`, the four operation classes, and `OperationRegistry`. No `System.Console`, no DTOs from Application or CLI appear here. Application references Domain but not CLI. The CLI references both, wires them in `Program.cs`, and all user-visible output is confined to `Program.cs` and `ReplRunner`. The boundary is exactly what the plan specified, with no drift.

## Division by zero handling

`DivideOperation.Execute` checks `right == 0m` before dividing and returns `CalculationResult.Failure("Division by zero is not allowed.")`. The error propagates as a `CalculationResponse` with `IsSuccess == false` through `CalculatorService` to the caller. The CLI prints to stderr and exits with code 1 in single-shot mode; the REPL prints to stdout (not stderr) in interactive mode, which is appropriate behavior for a read-eval-print context.

## Test coverage

All 23 tests target Domain operations and Application service via their public APIs — no `Console` appears in any test file. The `Divide_DecimalResult_ReturnsDecimal` test uses `BeApproximately` with a tolerance of `0.0001m` rather than an exact equality check, which correctly accounts for the recurring decimal 10/3. The `Multiply_LargeNumbers_NoOverflow` test exercises `decimal`'s 28-digit precision boundary with `1e14m * 1e14m`. Parser tests cover all branching: no-args REPL, help flags, valid three-arg invocation, decimal operands, too-few args, too-many args, and non-numeric operand. The `ReplRunner` itself has no unit tests — the plan did not call for them and the component is thin enough that the integration coverage from manual runs in the verification report is adequate.

</details>

---

<details>
<summary>File map</summary>

| File | What changed |
|------|-------------|
| `src/MathCalc.Domain/IOperation.cs` | New: operation contract |
| `src/MathCalc.Domain/CalculationResult.cs` | New: discriminated-union result type |
| `src/MathCalc.Domain/OperationRegistry.cs` | New: name→operation map, OrdinalIgnoreCase |
| `src/MathCalc.Domain/Operations/AddOperation.cs` | New: addition |
| `src/MathCalc.Domain/Operations/SubtractOperation.cs` | New: subtraction |
| `src/MathCalc.Domain/Operations/MultiplyOperation.cs` | New: multiplication |
| `src/MathCalc.Domain/Operations/DivideOperation.cs` | New: division with zero guard |
| `src/MathCalc.Application/CalculationRequest.cs` | New: input DTO |
| `src/MathCalc.Application/CalculationResponse.cs` | New: output DTO |
| `src/MathCalc.Application/ICalculatorService.cs` | New: service interface |
| `src/MathCalc.Application/CalculatorService.cs` | New: orchestrates registry + operation |
| `src/MathCalc.Cli/Program.cs` | New: entry point, mode dispatch |
| `src/MathCalc.Cli/ParseResult.cs` | New: discriminated union for parse outcomes |
| `src/MathCalc.Cli/CommandLineParser.cs` | New: parses string[] args |
| `src/MathCalc.Cli/HelpText.cs` | New: raw string literal help text |
| `src/MathCalc.Cli/ReplRunner.cs` | New: interactive loop |
| `tests/MathCalc.Tests/OperationsTests.cs` | New: 11 operation tests |
| `tests/MathCalc.Tests/CalculatorServiceTests.cs` | New: 4 service tests |
| `tests/MathCalc.Tests/CommandLineParserTests.cs` | New: 8 parser tests |

Full diff: `git diff` from `c:\_projects\0004. tests\003. AI\KIRO\001\MathCalc\`

</details>
