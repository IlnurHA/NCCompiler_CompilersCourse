using System.Collections;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

public abstract class SymbolicNode
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

    public SymbolicNode()
    {
        Children = new List<SymbolicNode>();
    }

    public bool Equals(SymbolicNode? obj)
    {
        if (obj == null) return false;
        return MyType == obj.MyType
               && ((StructFields == null && obj.StructFields == null) ||
                   (StructFields != null && StructFields!.Equals(obj.StructFields)))
               && ((ArrayElements == null && obj.ArrayElements == null) ||
                   (ArrayElements != null && ArrayElements!.Equals(obj.ArrayElements)))
               && ((FuncArguments == null && obj.FuncArguments == null) ||
                   (FuncArguments != null && FuncArguments!.Equals(obj.FuncArguments)))
               && ((FuncReturn != null && FuncReturn.Equals(obj.FuncReturn)) ||
                   (FuncReturn == null && obj.FuncReturn == null))
               && ((CompoundType != null && CompoundType.Equals(obj.CompoundType)) ||
                   (CompoundType == null && obj.CompoundType == null))
               && IsInitialized == obj.IsInitialized
               && ((ArraySize == null && obj.ArraySize == null) ||
                   (ArraySize != null && ArraySize!.Equals(obj.ArraySize)));
    }

    // public abstract SymbolicNode Accept(IVisitor visitor);
}

public class TypedSymbolicNode : SymbolicNode
{
    public TypeNode Type { get; set; } = new TypeNode(MyType.Undefined);

    public TypedSymbolicNode()
    {
    }

    public TypedSymbolicNode(TypeNode typeNode)
    {
        Type = typeNode;
    }
}

public class TypeNode : SymbolicNode
{
    public new MyType MyType { get; set; }

    public TypeNode(MyType myType)
    {
        MyType = myType;
    }

    public TypeNode()
    {
        MyType = MyType.Undefined;
    }

    public bool IsTheSame(TypeNode anotherObject)
    {
        return MyType == anotherObject.MyType;
    }
}

public class ArrayTypeNode : TypeNode
{
    public TypeNode ElementTypeNode { get; }
    public int? Size { get; set; }

    public ArrayTypeNode(TypeNode elementTypeNode, int size) : base(MyType.CompoundType)
    {
        ElementTypeNode = elementTypeNode;
        Size = size;
    }

    public ArrayTypeNode(TypeNode elementTypeNode) : base(MyType.CompoundType)
    {
        ElementTypeNode = elementTypeNode;
    }

    public new bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject.GetType() != typeof(ArrayTypeNode)) return false;
        var tempObj = (ArrayTypeNode) anotherObject;
        return MyType == tempObj.MyType && ElementTypeNode.IsTheSame(tempObj) && Size == tempObj.Size;
    }
}

public class StructTypeNode : TypeNode
{
    public Dictionary<string, TypeNode> StructFields { get; set; }

    public StructTypeNode(Dictionary<string, TypeNode> structFields) : base(MyType.CompoundType)
    {
        StructFields = structFields;
    }
}

public class UserDefinedTypeNode : TypeNode
{
    public TypeNode Type { get; set; }
    public string name { get; set; }
    public new MyType MyType { get; set; } = MyType.DeclaredType;

    public bool IsTheSame(TypeNode anotherObject)
    {
        return Type.IsTheSame(anotherObject);
    }
}

public class ValueNode : SymbolicNode
{
    public new Object? Value { get; set; }
    public TypeNode Type { get; set; }

    public bool isSubVar { get; set; } = false;
    public CompoundGettingNode? Child { get; set; } = null;

    public ValueNode(Object? value, TypeNode type)
    {
        Value = value;
        Type = type;
    }

    public ValueNode()
    {
        Value = null;
        Type = new TypeNode(MyType.Undefined);
    }
}

public class ConstNode : ValueNode
{
    public ConstNode(Object value, TypeNode typeNode) : base(value, typeNode)
    {
    }
}

public class VarNode : ValueNode
{
    public new string? Name { get; set; } = null;
    public bool IsInitialized { get; set; } = false;

    public VarNode(string name)
    {
        Name = name;
    }
    public VarNode() {}
}

public class StatementNode : TypedSymbolicNode
{
}

public class BodyNode : TypedSymbolicNode
{
    public List<StatementNode> Statements { get; set; }
}

public class StatementWithBodyNode : StatementNode
{
    public BodyNode Body { get; set; }
}

public class OperationNode : SymbolicNode
{
    public OperationType OperationType { get; set; }
    public List<ValueNode> operands { get; set; } = new List<ValueNode>();

    public OperationNode(OperationType operationType)
    {
        OperationType = operationType;
    }
}

public class ArrayVarNode : VarNode
{
    public List<ValueNode> Elements = new List<ValueNode>();

    public ArrayVarNode(string name, TypeNode elementTypeNode, int size) : base(name)
    {
        Elements.EnsureCapacity(size);
        Type = new ArrayTypeNode(elementTypeNode, size);
    }

    public ArrayVarNode(string name, TypeNode elementTypeNode, List<ValueNode> values) : base(name)
    {
        Elements = values;
        Type = new ArrayTypeNode(elementTypeNode, values.Count);
    }

    // For function parameters with arbitrary number of elements
    public ArrayVarNode(string name, TypeNode elementTypeNode) : base(name)
    {
        Type = new ArrayTypeNode(elementTypeNode);
    }

    public ArrayVarNode(ArrayTypeNode elementTypeNode) : base(null)
    {
        Type = elementTypeNode;
    }
}

public class CompoundGettingNode : TypedSymbolicNode
{
    public CompoundGettingNode(TypeNode typeNode) : base(typeNode)
    {
    }

    public ValueNode GetValueNodeFromType(TypeNode typeNode)
    {
        switch (typeNode)
        {
            case ArrayTypeNode arrayTypeNode:
                return new ArrayVarNode(arrayTypeNode);
            case StructTypeNode structTypeNode:
                return new StructVarNode(structTypeNode.StructFields, structTypeNode); // TODO getValueNode for each element in dictionary
            case UserDefinedTypeNode userDefinedTypeNode:
                return GetValueNodeFromType(userDefinedTypeNode);
            case { } simpleTypeNode:
                return new ValueNode(null, simpleTypeNode);
        }

        throw new Exception("Got null type node");
    }

    public ValueNode GetValueNode()
    {
        throw new Exception("This function is not supposed to be called");
    }
}

public class GetByIndexNode : CompoundGettingNode
{
    public ArrayVarNode ArrayVarNode { get; set; }
    public ValueNode Index { get; set; }

    public GetByIndexNode(ArrayVarNode varNode, ValueNode index) : base(((ArrayTypeNode) varNode.Type).ElementTypeNode)
    {
        ArrayVarNode = varNode;
        Index = index;
    }

    public new ValueNode GetValueNode()
    {
        var node = GetValueNodeFromType(ArrayVarNode.Type);
        node.Child = this;
        return node;
    }
}

public class StructVarNode : VarNode
{
    public Dictionary<string, VarNode> Fields { get; set; }

    public StructVarNode(Dictionary<string, VarNode> fields, StructTypeNode structTypeNode) : base()
    {
        Fields = fields;
        Type = structTypeNode;
    }

    public VarNode GetField(string fieldName)
    {
        if (!Fields.ContainsKey(fieldName))
            throw new Exception($"Trying to get undefined field {fieldName} from {Name} struct");
        return Fields[fieldName];
    }
}

public class GetFieldNode : CompoundGettingNode
{
    public StructVarNode StructVarNode { get; set; }
    public string FieldName { get; set; }
    public VarNode? FieldNode { get; set; } = null;

    public GetFieldNode(StructVarNode structVarNode, string fieldName) : base(structVarNode.GetField(fieldName).Type)
    {
        StructVarNode = structVarNode;
        FieldName = fieldName;
    }
    
    public GetFieldNode(StructVarNode structVarNode, VarNode fieldNode) : base(structVarNode.GetField(fieldNode.Name!).Type)
    {
        StructVarNode = structVarNode;
        FieldName = fieldNode.Name!;
        FieldNode = fieldNode;
    }

    public new ValueNode GetValueNode()
    {
        var node = StructVarNode!.GetField(FieldName!);
        node.Child = this;
        node.isSubVar = true;
        return node;
    }
}


public class ArrayFunctions : TypedSymbolicNode
{
    public ArrayVarNode Array { get; set; }
    public ArrayFunctions(ArrayVarNode arrayVarNode)
    {
        Array = arrayVarNode;
    }
}
public class SortedArrayNode : ArrayFunctions
{
    public SortedArrayNode(ArrayVarNode arrayVarNode) : base(arrayVarNode)
    {
        Type = arrayVarNode.Type;
    }
}

public class ArraySizeNode : ArrayFunctions
{

    public ArraySizeNode(ArrayVarNode arrayVarNode) : base(arrayVarNode)
    {
        Type = new TypeNode(MyType.Integer); 
    }

}

public class ReversedArrayNode : ArrayFunctions
{
    public ReversedArrayNode(ArrayVarNode arrayVarNode) : base(arrayVarNode)
    {
        Type = arrayVarNode.Type; 
    }
}