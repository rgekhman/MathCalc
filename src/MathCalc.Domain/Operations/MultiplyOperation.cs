namespace MathCalc.Domain.Operations;

public sealed class MultiplyOperation : IOperation
{
    public string Name => "multiply";

    public CalculationResult Execute(decimal left, decimal right)
        => CalculationResult.Success(left * right);
}
