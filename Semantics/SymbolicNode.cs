using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

public class SymbolicNode
{
    public MyType MyType { get; set; }

    public string? Name { get; set; }
    // SymbolicNode TypeNode
    // TypeNode -> children ()

    public Dictionary<string, SymbolicNode>? StructFields { get; set; }
    public List<SymbolicNode>? ArrayElements { get; set; }
    public Dictionary<string, SymbolicNode>? FuncArguments { get; set; }
    public SymbolicNode? FuncReturn { get; set; }
    public object? Value { get; set; }

    public List<SymbolicNode> Children { get; set; }

    public SymbolicNode? CompoundType { get; set; }


    public SymbolicNode(MyType myType, List<SymbolicNode>? children = null, string? name = null,
        Dictionary<string, SymbolicNode>? structFields = null, List<SymbolicNode>? arrayElements = null,
        Dictionary<string, SymbolicNode>? funcArguments = null, SymbolicNode? funcReturn = null, object? value = null, SymbolicNode? compoundType = null)
    {
        MyType = myType;
        Name = name;
        StructFields = structFields;
        ArrayElements = arrayElements;
        FuncArguments = funcArguments;
        FuncReturn = funcReturn;
        Value = value;
        Children = children ?? new List<SymbolicNode>();
        CompoundType = compoundType;
    }
}