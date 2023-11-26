namespace NCCompiler_CompilersCourse.Semantics;

public class SemanticsScope : IDisposable
{
    public enum ScopeContext
    {
        Global,
        Loop,
        Routine,
        IfStatement
    }

    private Dictionary<string, (VarNode, int)> Variables { get; set; } = new();
    private Dictionary<string, (TypeNode, int)> DefinedTypes { get; } = new();
    public ScopeContext ScopeContextVar { get; }
    public string? ScopeContextName { get; }

    public SemanticsScope(ScopeContext scopeContext = ScopeContext.Global, string? scopeContextName = null)
    {
        ScopeContextVar = scopeContext;
        ScopeContextName = scopeContextName;
    }

    public bool IsFree(string name)
    {
        return !(Variables.TryGetValue(name, out _) || DefinedTypes.TryGetValue(name, out _));
    }
    
    public SymbolicNode? FindVariable(string name)
    {
        var isFound = Variables.TryGetValue(name, out var result);
        if (isFound)
        {
            Variables[name] = (result.Item1, result.Item2 + 1);
            return result.Item1;
        }

        isFound = DefinedTypes.TryGetValue(name, out var resultType);
        if (isFound)
        {
            DefinedTypes[name] = (resultType.Item1, resultType.Item2 + 1);
            return resultType.Item1;
        }

        return null;
    }

    public void AddVariable(VarNode node)
    {
        if (node.Name == null)
        {
            throw new Exception("Trying to save not variable as a variable");
        }

        if (Variables.TryGetValue(node.Name, out _) || DefinedTypes.TryGetValue(node.Name, out _))
        {
            throw new Exception($"Cannot define variable second time: {node.Name}");
        }
        Variables.Add(node.Name, (node, 0));
    }
    
    public void AddType(UserDefinedTypeNode node)
    {
        if (node.Name == null)
        {
            throw new Exception("Trying to save not variable as a variable");
        }

        if (Variables.TryGetValue(node.Name, out _) || DefinedTypes.TryGetValue(node.Name, out _))
        {
            throw new Exception($"Cannot define variable second time: {node.Name}");
        }
        DefinedTypes.Add(node.Name, (node, 0));
    }

    public void UpdateValue(string name, ValueNode value)
    {
        var variableBuffer = FindVariable(name);
        switch (variableBuffer)
        {
            case null:
                throw new Exception("No variable with this name");
            case TypeNode:
                throw new Exception("Trying to update value of type alias");
        }

        var variable = (VarNode) variableBuffer;

        if (!value.Type.IsConvertibleTo(variable.Type))
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

    public List<TypeNode> GetUnusedTypes()
    {
        List<TypeNode> unusedVariables = new List<TypeNode>(Variables.Count);
        foreach (var (_, (typeNode, counter)) in DefinedTypes)
        {
            if (counter == 0) unusedVariables.Add(typeNode);
        }

        return unusedVariables;
    }

    public void Dispose()
    {
    }
}