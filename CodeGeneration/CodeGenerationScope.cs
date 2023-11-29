using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationScope
{
    public Dictionary<string, CodeGenerationVariable> Arguments { get; set; } = new();
    public Dictionary<string, CodeGenerationVariable> LocalVariables { get; } = new();
    private readonly string _hash;

    private int _lastLocalId;
    private int _lastArgumentId = 1;
    private int _specialCounter;

    public CodeGenerationScope(string hash)
    {
        _hash = hash;
    }

    private void AddAny(Dictionary<string, CodeGenerationVariable> dictionary, string name, TypeNode type, int lastId)
    {
        CodeGenerationVariable variable = new CodeGenerationVariable(name, type, lastId);
        if (LocalVariables.TryGetValue(variable.GetName(), out _))
        {
            throw new Exception($"Cannot define variable second time: {variable.GetName()}");
        }

        dictionary.Add(variable.GetName(), variable);
    }

    public void AddArgument(string name, TypeNode type)
    {
        AddAny(Arguments, name, type, _lastArgumentId);
        _lastArgumentId += 1;
    }

    public void AddVariable(string name, TypeNode type)
    {
        AddAny(LocalVariables, name, type, _lastLocalId);
        _lastLocalId += 1;
    }

    public string AddSpecialVariable(TypeNode type)
    {
        string name = $"{_hash}_{_specialCounter}";
        AddVariable(name, type);
        _specialCounter += 1;
        return name;
    }

    public bool HasArgument(string name)
    {
        return Arguments.ContainsKey(name);
    }

    public bool HasVariable(string name)
    {
        return LocalVariables.ContainsKey(name);
    }

    public CodeGenerationVariable? GetArgument(string name)
    {
        if (Arguments.TryGetValue(name, out var variable)) return variable;
        return null;
    }

    public CodeGenerationVariable? GetVariable(string name)
    {
        if (LocalVariables.TryGetValue(name, out var variable)) return variable;
        return null;
    }
}