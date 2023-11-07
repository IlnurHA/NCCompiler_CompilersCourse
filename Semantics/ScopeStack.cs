namespace NCCompiler_CompilersCourse.Semantics;

public class ScopeStack
{
    public List<Scope> Scopes { get; set; } = new ();
    
    public void NewScope()
    {
        Scopes.Add(new Scope());
    }

    public void DeleteScope()
    {
        Scopes.RemoveAt(Scopes.Count - 1);
    }

    public SymbolicNode FindVariable(string name)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            var result = Scopes[i].FindVariable(name);
            if (result != null)
            {
                return result;
            }
        }

        throw new Exception($"The variable {name} is not declared");
    }

    public void AddVariable(SymbolicNode node)
    {
        Scopes[^1].AddVariable(node);
    }
}