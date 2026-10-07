namespace MathCalc.Application;

public interface ICalculatorService
{
    CalculationResponse Calculate(CalculationRequest request);
}
