namespace MathCalc.Domain.Operations;

public sealed class SubtractOperation : IOperation
{
    public string Name => "subtract";

    public CalculationResult Execute(decimal left, decimal right)
        => CalculationResult.Success(left - right);
}
