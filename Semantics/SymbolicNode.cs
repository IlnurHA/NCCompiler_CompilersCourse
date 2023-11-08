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
    public SymbolicNode? ArraySize;
    public Dictionary<string, SymbolicNode>? FuncArguments { get; set; }
    public SymbolicNode? FuncReturn { get; set; }
    public object? Value { get; set; }

    public List<SymbolicNode> Children { get; set; }

    public SymbolicNode? CompoundType { get; set; }
    
    public bool? IsInitialized { get; set; }


    public SymbolicNode(MyType myType, List<SymbolicNode>? children = null, string? name = null,
        Dictionary<string, SymbolicNode>? structFields = null, List<SymbolicNode>? arrayElements = null,
        Dictionary<string, SymbolicNode>? funcArguments = null, SymbolicNode? funcReturn = null, object? value = null,
        SymbolicNode? compoundType = null, bool? isInitialized = null, SymbolicNode? arraySize = null)
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
        IsInitialized = isInitialized;
        ArraySize = arraySize;
    }

    public bool Equals(SymbolicNode? obj)
    {
        if (obj == null) return false;
        return MyType == obj.MyType
               && ((StructFields == null && obj.StructFields == null) || (StructFields != null && StructFields!.Equals(obj.StructFields)))
               && ((ArrayElements == null && obj.ArrayElements == null) || (ArrayElements != null && ArrayElements!.Equals(obj.ArrayElements)))
               && ((FuncArguments == null && obj.FuncArguments == null) || (FuncArguments != null && FuncArguments!.Equals(obj.FuncArguments)))
               && ((FuncReturn != null && FuncReturn.Equals(obj.FuncReturn)) ||
                   (FuncReturn == null && obj.FuncReturn == null))
               && ((CompoundType != null && CompoundType.Equals(obj.CompoundType)) ||
                   (CompoundType == null && obj.CompoundType == null))
               && IsInitialized == obj.IsInitialized
               && ((ArraySize == null && obj.ArraySize == null) || (ArraySize != null && ArraySize!.Equals(obj.ArraySize)));
    }
}