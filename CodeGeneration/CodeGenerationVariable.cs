using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationVariable
{
    private readonly string _name;
    public int Id { get; }
    public TypeNode Type { get; }
    public bool IsArgument { get; }

    public CodeGenerationVariable(string name, TypeNode typeNode, int id)
    {
        _name = name;
        Id = id;
        Type = typeNode;
    }
    
    private static string MyTypeToType(MyType myType)
    {
        return myType switch
        {
            MyType.Integer => "int32",
            MyType.Real => "float32",
            MyType.Boolean => "bool",
            var type => throw new Exception($"Error. Incorrect operand type {type}.")
        };
    }
    public static string NodeToType(TypeNode typeNode)
    {
        return typeNode switch
        {
            StructTypeNode => "", // TODO: Implement struct representation
            ArrayTypeNode arrayTypeNode => $"{NodeToType(arrayTypeNode.ElementTypeNode)}[]",
            { } type => MyTypeToType(type.MyType),
        };
    }

    public string GetName()
    {
        return _name;
    }
}