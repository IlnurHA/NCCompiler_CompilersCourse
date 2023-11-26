namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationScope
{
    private Dictionary<string, CodeGenerationVariable> Variables { get; set; } = new();
    
    public void AddVariable(CodeGenerationVariable variable)
    {
        if (Variables.TryGetValue(variable.GetName(), out _))
        {
            throw new Exception($"Cannot define variable second time: {variable.GetName()}");
        }
        Variables.Add(variable.GetName(), variable);
    }
    
    public bool HasVariable(string name)
    {
        return Variables.ContainsKey(name);
    }

    public CodeGenerationVariable? GetVariable(string name)
    {
        if (Variables.TryGetValue(name, out var variable)) return variable;
        return null;
    }
    
}