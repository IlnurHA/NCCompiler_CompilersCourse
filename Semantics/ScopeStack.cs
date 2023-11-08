namespace NCCompiler_CompilersCourse.Semantics;

public class ScopeStack
{
    public List<Scope> Scopes { get; set; } = new ();

    public ScopeStack()
    {
        Scopes.Add(new Scope());
    }

    public void NewScope(Scope.ScopeContext scopeContext)
    {
        Scopes.Add(new Scope(scopeContext: scopeContext));
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
    
    public bool HasVariable(string name)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            var result = Scopes[i].FindVariable(name);
            if (result != null)
            {
                return true;
            }
        }
        return false;
    }
    
    

    public void AddVariable(SymbolicNode node)
    {
        Scopes[^1].AddVariable(node);
    }

    public Scope GetLastScope()
    {
        return Scopes.Last();
    }

    public bool HasRoutineScope()
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            if (Scopes[i].ScopeContextVar == Scope.ScopeContext.Routine)
            {
                return true;
            }
        }

        return false;
    }

    public (Scope, int) GetLastRoutineScope()
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            if (Scopes[i].ScopeContextVar == Scope.ScopeContext.Routine)
            {
                return (Scopes[i], i);
            }
        }

        throw new Exception("No routine scopes yet");
    }

    public bool HasLoopScope()
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            if (Scopes[i].ScopeContextVar == Scope.ScopeContext.Loop)
            {
                return true;
            }
        }

        return false;
    }

    public void PopUntilLastRoutineScope()
    {
        var (scope, index) = GetLastRoutineScope();

        Scopes = Scopes.GetRange(0, index + 1);
    }
}