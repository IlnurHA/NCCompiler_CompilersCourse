namespace NCCompiler_CompilersCourse.Semantics;

public class Scope
{
    public enum ScopeContext
    {
        Global,
        Loop,
        Routine,
        IfStatement
    }
    
    public Dictionary<string, SymbolicNode> Variables { get; set; } = new();
    public ScopeContext ScopeContextVar { get; }
    public string? ScopeContextName { get; } 
    
    public Scope(Dictionary<string, SymbolicNode>? variables = null, ScopeContext scopeContext = ScopeContext.Global, string? scopeContextName = null)
    {
        if (variables != null)
        {
            Variables = variables;
        }

        ScopeContextVar = scopeContext;
        ScopeContextName = scopeContextName;
    }

    public SymbolicNode? FindVariable(string name)
    {
        return Variables.TryGetValue(name, out var variable) ? variable : null;
    }

    public void AddVariable(SymbolicNode node)
    {
        if (node.Name == null)
        {
            throw new Exception("Trying to save not variable as a variable");
        }

        if (Variables.TryGetValue(node.Name, out var variable))
        {
            if (variable.MyType != node.MyType || (variable.CompoundType != null && !variable.CompoundType.Equals(node.CompoundType)))
            {
                throw new Exception("Cannot define variable second time with different type");
            }

            Variables[node.Name] = node;
        }
        else Variables.Add(node.Name, node);
    }
}