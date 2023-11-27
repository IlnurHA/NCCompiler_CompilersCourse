using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationScope
{
    private Dictionary<string, CodeGenerationVariable> Variables { get; set; } = new();
    private readonly string _hash;
    private int _lastId;
    private int _specialCounter;

    public CodeGenerationScope(string hash)
    {
        _hash = hash;
    }
    
    public void AddVariable(string name, TypeNode type)
    {
        CodeGenerationVariable variable = new CodeGenerationVariable(name, type, _lastId);
        if (Variables.TryGetValue(variable.GetName(), out _))
        {
            throw new Exception($"Cannot define variable second time: {variable.GetName()}");
        }
        Variables.Add(variable.GetName(), variable);
        _lastId += 1;
    }

    public void AddSpecialVariable(TypeNode type)
    {
        string name = $"{_hash}_{_specialCounter}";
        AddVariable(name, type);
        _specialCounter += 1;
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