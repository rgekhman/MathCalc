namespace MathCalc.Domain;

public interface IOperation
{
    string Name { get; }
    CalculationResult Execute(decimal left, decimal right);
}
