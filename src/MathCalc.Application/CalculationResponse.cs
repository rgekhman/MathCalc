namespace MathCalc.Application;

public sealed record CalculationResponse(bool IsSuccess, decimal Value, string ErrorMessage);
