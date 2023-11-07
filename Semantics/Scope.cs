namespace NCCompiler_CompilersCourse.Semantics;

public class Scope
{
    public Dictionary<string, SymbolicNode> Variables { get; set; } = new();

    public Scope(Dictionary<string, SymbolicNode>? variables = null)
    {
        if (variables != null)
        {
            Variables = variables;
        }
    }

    public SymbolicNode? FindVariable(string name)
    {
        return Variables.TryGetValue(name, out var variable) ? variable : null;
    }
    
    public Boolean HasVariable(string name)
    {
        return Variables.TryGetValue(name, out var variable);
    }

    public void AddVariable(SymbolicNode node)
    {
        if (node.Name == null)
        {
            throw new Exception("Trying to save not variable as a variable");
        }

        if (Variables.TryGetValue(node.Name, out var variable))
        {
            if (variable.MyType != node.MyType)
            {
                throw new Exception("Cannot define variable second time with different type");
            }

            Variables[node.Name] = node;
        }
        else Variables.Add(node.Name, node);
    }
    
    
}