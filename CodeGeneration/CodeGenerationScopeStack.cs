namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationScopeStack
{
    private List<CodeGenerationScope> Scopes { get; set; } = new ();
    
    public void AddVariableInLastScope(CodeGenerationVariable node)
    {
        Scopes[^1].AddVariable(node);
    }
    
    public bool HasVariableInLastScope(string name)
    {
        return Scopes[^1].HasVariable(name);
    }
    
    public CodeGenerationVariable? GetVariable(string name)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        {
            var result = Scopes[i].GetVariable(name);
            if (result != null) return result;
        }
        return null;
    }
}