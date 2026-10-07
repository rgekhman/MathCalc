namespace MathCalc.Application;

public sealed record CalculationRequest(string OperationName, decimal Left, decimal Right);
