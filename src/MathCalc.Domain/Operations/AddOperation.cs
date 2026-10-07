namespace MathCalc.Domain.Operations;

public sealed class AddOperation : IOperation
{
    public string Name => "add";

    public CalculationResult Execute(decimal left, decimal right)
        => CalculationResult.Success(left + right);
}
