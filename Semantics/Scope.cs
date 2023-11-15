namespace NCCompiler_CompilersCourse.Semantics;

public class Scope : IDisposable
{
    public enum ScopeContext
    {
        Global,
        Loop,
        Routine,
        IfStatement
    }

    private Dictionary<string, (VarNode, int)> Variables { get; set; } = new();
    public ScopeContext ScopeContextVar { get; }
    public string? ScopeContextName { get; }

    public Scope(ScopeContext scopeContext = ScopeContext.Global, string? scopeContextName = null)
    {
        ScopeContextVar = scopeContext;
        ScopeContextName = scopeContextName;
    }

    public bool IsFree(string name)
    {
        return !Variables.TryGetValue(name, out _);
    }

    public VarNode? FindVariable(string name)
    {
        var isFound = Variables.TryGetValue(name, out var result);
        if (isFound)
        {
            Variables[name] = (result.Item1, result.Item2 + 1);
            return result.Item1;
        }

        return null;
    }

    public void AddVariable(VarNode node)
    {
        if (node.Name == null)
        {
            throw new Exception("Trying to save not variable as a variable");
        }

        if (Variables.TryGetValue(node.Name, out _))
        {
            throw new Exception($"Cannot define variable second time: {node.Name}");
        }
        Variables.Add(node.Name, (node, 0));
    }

    public void UpdateValue(string name, ValueNode value)
    {
        var variable = FindVariable(name);
        if (variable is null) throw new Exception("No variable with this name");
        if (!CheckTypes(variable.Type, value.Type))
            throw new Exception(
                $"Trying to update with different types: {name} - type1 {variable.Value!.GetType()}, type2 {value.GetType()}");

        variable.Value = value.Value;
    }

    public List<VarNode> GetUnusedVariables()
    {
        List<VarNode> unusedVariables = new List<VarNode>(Variables.Count);
        foreach (var variable in Variables)
        {
            if (variable.Value.Item2 == 0) unusedVariables.Add(variable.Value.Item1);
        }

        return unusedVariables;
    }

    private bool CheckTypes(TypeNode valueA, TypeNode valueB)
    {
        return valueA == valueB;
    }

    public void Dispose()
    {
    }
}