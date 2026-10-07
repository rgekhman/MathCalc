namespace MathCalc.Domain;

using MathCalc.Domain.Operations;

public sealed class OperationRegistry
{
    private readonly Dictionary<string, IOperation> _operations;

    public OperationRegistry()
    {
        _operations = new Dictionary<string, IOperation>(StringComparer.OrdinalIgnoreCase);
        Register(new AddOperation());
        Register(new SubtractOperation());
        Register(new MultiplyOperation());
        Register(new DivideOperation());
    }

    private void Register(IOperation op) => _operations[op.Name] = op;

    public bool TryGet(string name, out IOperation operation)
        => _operations.TryGetValue(name, out operation!);

    public IReadOnlyCollection<string> SupportedOperations => _operations.Keys;
}
