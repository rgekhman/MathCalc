namespace MathCalc.Application;

using MathCalc.Domain;

public sealed class CalculatorService : ICalculatorService
{
    private readonly OperationRegistry _registry;

    public CalculatorService(OperationRegistry registry)
    {
        _registry = registry;
    }

    public CalculationResponse Calculate(CalculationRequest request)
    {
        if (!_registry.TryGet(request.OperationName, out var operation))
        {
            return new CalculationResponse(false, 0m,
                $"Unknown operation '{request.OperationName}'. Supported: {string.Join(", ", _registry.SupportedOperations)}");
        }

        var result = operation.Execute(request.Left, request.Right);
        return result.IsSuccess
            ? new CalculationResponse(true, result.Value, string.Empty)
            : new CalculationResponse(false, 0m, result.Error);
    }
}
