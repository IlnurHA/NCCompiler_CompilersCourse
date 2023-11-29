using System.Security.Cryptography;
using System.Text;
using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationScopeStack
{
    private readonly string _hash;
    
    public CodeGenerationScope GetLastScope()
    {
        return Scopes[^1];
    }
    private List<CodeGenerationScope> Scopes { get; set; } = new (){new CodeGenerationScope("global")};
    
    private int _structCounter = -1;

    public int GetStructCounter()
    {
        _structCounter += 1;
        return _structCounter;
    }

    public CodeGenerationScope GetGlobalScope()
    {
        return Scopes[0];
    }

    public CodeGenerationVariable? GetByStructType(StructTypeNode structTypeNode)
    {
        for (int i = Scopes.Count - 1; i >= 0; i++)
        {
            var scope = Scopes[i];
            foreach (var (_, codeGenerationVariable) in scope.Arguments)
            {
                if (codeGenerationVariable.Type.IsTheSame(structTypeNode)) return codeGenerationVariable;
            }
            
            foreach (var (_, codeGenerationVariable) in scope.LocalVariables)
            {
                if (codeGenerationVariable.Type.IsTheSame(structTypeNode)) return codeGenerationVariable;
            }
        }

        return null;
    }
    
    public CodeGenerationScopeStack()
    {
        _hash = Guid.NewGuid().ToString()[..8];
    }
    public void AddVariableInLastScope(string name, TypeNode type)
    {
        Scopes[^1].AddVariable(name, type);
    }
    
    public string AddSpecialVariableInLastScope(TypeNode type)
    {
        return Scopes[^1].AddSpecialVariable(type);
    }
    
    public void AddArgumentInLastScope(string name, TypeNode type)
    {
        Scopes[^1].AddArgument(name, type);
    }
    
    public void CreateNewScope(SemanticsScope.ScopeContext scopeContext)
    {
        Scopes.Add(new CodeGenerationScope(_hash));
    }

    public void DeleteLastScope()
    {
        Scopes.RemoveAt(Scopes.Count - 1);
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

    public CodeGenerationVariable? GetArgumentInLastScope(string name)
    {
        return Scopes[^1].GetArgument(name);
    }
}