namespace MathCalc.Domain;

public sealed class CalculationResult
{
    public bool IsSuccess { get; }
    public decimal Value  { get; }
    public string Error   { get; }

    private CalculationResult(bool isSuccess, decimal value, string error)
    {
        IsSuccess = isSuccess;
        Value     = value;
        Error     = error;
    }

    public static CalculationResult Success(decimal value) => new(true, value, string.Empty);
    public static CalculationResult Failure(string error)  => new(false, 0m, error);
}
