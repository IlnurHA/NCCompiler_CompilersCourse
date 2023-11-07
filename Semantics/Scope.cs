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

    public void AddVariable(SymbolicNode node)
    {
        if (node.Name == null)
        {
            throw new Exception("Trying to save not variable as a variable");
        }

        Variables.Add(node.Name, node);
    }
    
    
}