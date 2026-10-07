namespace MathCalc.Domain.Operations;

public sealed class DivideOperation : IOperation
{
    public string Name => "divide";

    public CalculationResult Execute(decimal left, decimal right)
    {
        if (right == 0m)
            return CalculationResult.Failure("Division by zero is not allowed.");

        return CalculationResult.Success(left / right);
    }
}
