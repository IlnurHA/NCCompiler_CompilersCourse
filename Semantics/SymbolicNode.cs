﻿using System.Collections;
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
    public TypeNode TypeNode { get; set; } = new TypeNode(MyType.Undefined);

    public TypedSymbolicNode()
    {
    }

    public TypedSymbolicNode(TypeNode typeNode)
    {
        TypeNode = typeNode;
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

    public bool IsTheSame(Object anotherObject)
    {
        if (anotherObject.GetType() != typeof(TypeNode)) return false;
        return MyType == ((TypeNode) anotherObject).MyType;
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

    public new bool IsTheSame(Object anotherObject)
    {
        if (anotherObject.GetType() != typeof(ArrayTypeNode)) return false;
        var tempObj = (ArrayTypeNode) anotherObject;
        return MyType == tempObj.MyType && ElementTypeNode.IsTheSame(tempObj) && Size == tempObj.Size;
    }
}

public class StructTypeNode : TypeNode
{
    public Dictionary<string, VarNode> StructFields { get; set; }
    public new string Name { get; set; }
    public new MyType MyType { get; set; } = MyType.CompoundType;

    public StructTypeNode(Dictionary<string, VarNode> structFields, string name)
    {
        Name = name;
        StructFields = structFields;
    } 
}

public class UserDefinedTypeNode : TypeNode
{
    public TypeNode Type { get; set; }
    public string name { get; set; }
    public new MyType MyType { get; set; } = MyType.DeclaredType;
}

public class ValueNode : SymbolicNode
{
    public new Object? Value { get; set; }
    public TypeNode Type { get; set; }

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

public class VarNode : ValueNode
{
    public new string Name { get; set; }
    public bool IsInitialized { get; set; } = false;

    public VarNode(string name)
    {
        Name = name;
    }
}

public class StatementNode : SymbolicNode
{
    public TypeNode Type { get; set; }
}

public class BodyNode : SymbolicNode
{
    public TypeNode Type { get; set; }
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
}

public class GetByIndexNode : TypedSymbolicNode
{
    public ArrayVarNode ArrayVarNode { get; set; }
    public ValueNode Index { get; set; }

    public GetByIndexNode(ArrayVarNode varNode, ValueNode index) : base(((ArrayTypeNode) varNode.Type).ElementTypeNode)
    {
        ArrayVarNode = varNode;
        Index = index;
    }
}

public class StructVarNode : VarNode
{
    public Dictionary<string, VarNode> Fields { get; set; }
    public StructVarNode(string name, Dictionary<string, VarNode> fields, StructTypeNode structTypeNode) : base(name)
    {
        Fields = fields;
        Type = structTypeNode;
    }

    public VarNode GetField(string fieldName)
    {
        if (!Fields.ContainsKey(fieldName)) throw new Exception("in ");
        return Fields[fieldName];
    }
}
public class GetFieldNode : TypedSymbolicNode
{
    public StructVarNode StructVarNode { get; set; }
    public string FieldName { get; set; }

    GetFieldNode(StructVarNode structVarNode, string fieldName) : base(structVarNode.GetField(fieldName).Type)
    {
        StructVarNode = structVarNode;
        FieldName = fieldName;
    }
}