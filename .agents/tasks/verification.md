# Verification Report — MathCalc CLI

Generated: iteration 1

---

## 1. Build Output

```
dotnet build MathCalc.slnx --configuration Release
```

```
Restore complete (0.3s)
MathCalc.Domain net8.0 succeeded → src\MathCalc.Domain\bin\Release\net8.0\MathCalc.Domain.dll
MathCalc.Application net8.0 succeeded → src\MathCalc.Application\bin\Release\net8.0\MathCalc.Application.dll
MathCalc.Cli net8.0 succeeded → src\MathCalc.Cli\bin\Release\net8.0\MathCalc.Cli.dll
MathCalc.Tests net8.0 succeeded → tests\MathCalc.Tests\bin\Release\net8.0\MathCalc.Tests.dll
Build succeeded in 1.0s
```

**Result: ✅ 0 errors, 0 warnings**

---

## 2. Test Results

```
dotnet test MathCalc.slnx --logger "console;verbosity=normal"
```

```
Passed MathCalc.Tests.CalculatorServiceTests.Calculate_ValidAdd_ReturnsSuccessResponse
Passed MathCalc.Tests.CommandLineParserTests.Parse_TooFewArgs_ReturnsError
Passed MathCalc.Tests.OperationsTests.Add_TwoPositiveDecimals_ReturnsSum
Passed MathCalc.Tests.OperationsTests.Subtract_ResultIsNegative
Passed MathCalc.Tests.OperationsTests.Multiply_LargeNumbers_NoOverflow
Passed MathCalc.Tests.OperationsTests.Multiply_TwoDecimals_ReturnsProduct
Passed MathCalc.Tests.OperationsTests.Add_NegativeNumbers_ReturnsCorrectSum
Passed MathCalc.Tests.CalculatorServiceTests.Calculate_UnknownOperation_ReturnsFailure
Passed MathCalc.Tests.OperationsTests.Divide_ExactDivision_ReturnsQuotient
Passed MathCalc.Tests.CalculatorServiceTests.Calculate_DivideByZero_ReturnsFailure
Passed MathCalc.Tests.OperationsTests.Multiply_ByZero_ReturnsZero
Passed MathCalc.Tests.CalculatorServiceTests.Calculate_OperationNameCaseInsensitive
Passed MathCalc.Tests.OperationsTests.Subtract_ReturnsCorrectDifference
Passed MathCalc.Tests.OperationsTests.Divide_NegativeDividend_ReturnsNegative
Passed MathCalc.Tests.OperationsTests.Divide_DecimalResult_ReturnsDecimal
Passed MathCalc.Tests.OperationsTests.Divide_ByZero_ReturnsFailure
Passed MathCalc.Tests.CommandLineParserTests.Parse_ValidArgs_ReturnsSingleShot
Passed MathCalc.Tests.CommandLineParserTests.Parse_NonNumericOperand_ReturnsError
Passed MathCalc.Tests.CommandLineParserTests.Parse_ShortHelpFlag_ReturnsHelp
Passed MathCalc.Tests.CommandLineParserTests.Parse_DecimalArgs_ReturnsSingleShot
Passed MathCalc.Tests.CommandLineParserTests.Parse_NoArgs_ReturnsRepl
Passed MathCalc.Tests.CommandLineParserTests.Parse_HelpFlag_ReturnsHelp
Passed MathCalc.Tests.CommandLineParserTests.Parse_TooManyArgs_ReturnsError

Test Run Successful.
Total tests: 23
     Passed: 23
 Total time: 1.9235 Seconds
```

**Result: ✅ 23/23 tests passed**

---

## 3. CLI Output

### `dotnet run --project src/MathCalc.Cli -- add 10 5`

```
Result: 15
```

**Result: ✅ Matches expected output**

---

### `dotnet run --project src/MathCalc.Cli -- divide 10 0`

```
Error: Division by zero is not allowed.
Exit code: 1
```

**Result: ✅ Clear error message, no crash, non-zero exit code**

---

### `dotnet run --project src/MathCalc.Cli -- divide 9.9 3`

```
Result: 3.3
```

**Result: ✅ Correct decimal result**

---

### `dotnet run --project src/MathCalc.Cli -- --help`

```
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
```

**Result: ✅ Help text displayed**

---

### `dotnet run --project src/MathCalc.Cli -- badop 1 2`

```
Error: Unknown operation 'badop'. Supported: add, subtract, multiply, divide
Exit: 1
```

**Result: ✅ Meaningful error, non-zero exit code**

---

### `dotnet run --project src/MathCalc.Cli -- add abc 2`

```
Error: 'abc' is not a valid number for the left operand.
Run with --help for usage.
Exit: 1
```

**Result: ✅ Input validation error, non-zero exit code**

---

## Summary

| Check | Status |
|-------|--------|
| `dotnet build` — 0 errors | ✅ |
| `dotnet test` — all 23 pass | ✅ |
| `add 10 5` → `Result: 15` | ✅ |
| `divide 10 0` → clear error, exit 1 | ✅ |
| `divide 9.9 3` → `Result: 3.3` | ✅ |
| `--help` → usage text | ✅ |
| Unknown operation → error, exit 1 | ✅ |
| Non-numeric operand → error, exit 1 | ✅ |
