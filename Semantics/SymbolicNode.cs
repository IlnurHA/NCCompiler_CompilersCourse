﻿using System.Collections;
using System.Linq.Expressions;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

public abstract class SymbolicNode
{
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

    public TypeNode ConvertTo(TypeNode toTypeNode)
    {
        if (toTypeNode.IsTheSame(this)) return this;
        return (toTypeNode.MyType, MyType) switch
        {
            (MyType.Integer, MyType.Real) => toTypeNode,
            (MyType.Integer, MyType.Boolean) => toTypeNode,
            (MyType.Real, MyType.Integer) => toTypeNode,
            (MyType.Real, MyType.Boolean) => toTypeNode,
            (MyType.Boolean, MyType.Integer) => toTypeNode,
            _ => throw new Exception($"Can't convert from type {MyType} to {toTypeNode.MyType}")
        };
    }

    public bool IsConvertibleTo(TypeNode toTypeNode)
    {
        try
        {
            ConvertTo(toTypeNode);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public TypeNode GetFinalTypeNode()
    {
        if (this is not UserDefinedTypeNode userDefinedTypeNode) return this;
        return userDefinedTypeNode.GetFinalTypeNode();
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
        if (anotherObject is not ArrayTypeNode tempObj) return false;
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

    public new bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject is not StructTypeNode tempObj) return false;
        if (StructFields.Count != tempObj.StructFields.Count) return false;
        foreach (var field in StructFields)
        {
            if (!tempObj.StructFields.ContainsKey(field.Key)) return false;
            if (!field.Value.IsTheSame(tempObj.StructFields[field.Key])) return false;
        }

        return true;
    }
}

public class UserDefinedTypeNode : TypeNode
{
    public TypeNode Type { get; set; }
    public string name { get; set; }
    public new MyType MyType { get; set; } = MyType.DeclaredType;

    public new bool IsTheSame(TypeNode anotherObject)
    {
        return Type.IsTheSame(anotherObject);
    }

    public new TypeNode GetFinalTypeNode()
    {
        switch (Type)
        {
            case ArrayTypeNode arrayTypeNode:
                return arrayTypeNode;
            case StructTypeNode structTypeNode:
                return structTypeNode;
            case UserDefinedTypeNode userDefinedTypeNode:
                return userDefinedTypeNode.GetFinalTypeNode();
            case { } simpleTypeNode:
                return simpleTypeNode;
        }

        throw new Exception("Got null type node");
    }
}

public class ValueNode : SymbolicNode
{
    public new Object? Value { get; set; }
    public TypeNode Type { get; set; }

    public ValueNode(TypeNode type, object? value = null)
    {
        Value = value;
        Type = type;
    }

    public ValueNode()
    {
        Value = null;
        Type = new TypeNode(MyType.Undefined);
    }

    public ValueNode GetFinalValueNode()
    {
        if (Type is not UserDefinedTypeNode userDefinedTypeNode) return this;
        var finalType = userDefinedTypeNode.GetFinalTypeNode();
        Type = finalType;
        return this;
    }
}

public class ConstNode : ValueNode
{
    public ConstNode(TypeNode typeNode, object value) : base(typeNode, value)
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

    public VarNode GetFinalVarNode()
    {
        if (Type is not UserDefinedTypeNode userDefinedTypeNode) return this;
        var finalType = userDefinedTypeNode.GetFinalTypeNode();
        Type = finalType;
        return this;
    }

    public VarNode()
    {
    }
}

public class StatementNode : TypedSymbolicNode
{
}

public class BreakNode : StatementNode
{
}

public class AssertNode : StatementNode
{
    private ValueNode leftExpression;
    private ValueNode rightExpression;

    public AssertNode(ValueNode left, ValueNode right)
    {
        leftExpression = left;
        rightExpression = right;
    }
}

public class ReturnNode : StatementNode
{
    private ValueNode? _returnValue;

    public ReturnNode(ValueNode returnValue)
    {
        Type = returnValue.Type;
        _returnValue = returnValue;
    }

    public ReturnNode()
    {
        Type = new TypeNode(MyType.Undefined);
    }
}

public class DeclarationNode : StatementNode
{
    public VarNode Variable { get; set; }

    public DeclarationNode(VarNode varNode, ValueNode? value)
    {
        Variable = varNode;
        Variable.Value = value;
        Variable.IsInitialized = value != null;
    }
}

public class TypeDeclarationNode : StatementNode
{
    public VarNode Variable { get; set; }

    public TypeDeclarationNode(VarNode varNode, TypeNode? value)
    {
        Variable = varNode;
        Variable.Value = value;
        Variable.IsInitialized = value != null;
    }
}

public class AssignmentNode : StatementNode
{
    public VarNode Variable { get; set; }

    public AssignmentNode(VarNode varNode, ValueNode value)
    {
        Variable = varNode;
        Variable.Value = value;
        Variable.IsInitialized = true;
    }
}

public class BodyNode : TypedSymbolicNode
{
    public List<StatementNode> Statements { get; set; }

    public BodyNode(List<StatementNode> statements, TypeNode typeNode)
    {
        Statements = statements;
    }
}

public class StatementWithBodyNode : StatementNode
{
    public BodyNode Body { get; set; }
}

public class OperationNode : ValueNode
{
    public OperationType OperationType { get; set; }
    public List<ValueNode> Operands { get; set; } = new List<ValueNode>();

    public OperationNode(OperationType operationType)
    {
        OperationType = operationType;
    }

    public OperationNode(OperationType operationType, List<ValueNode> operands, TypeNode typeNode) : base(
        type: typeNode)
    {
        OperationType = operationType;
        Operands = operands;
    }
}

public class ArrayVarNode : VarNode
{
    public List<ValueNode> Elements = new List<ValueNode>();

    public ArrayVarNode(string name, TypeNode elementTypeNode, int size) : base(name)
    {
        Elements.EnsureCapacity(size);
        Type = new ArrayTypeNode(elementTypeNode.GetFinalTypeNode(), size);
    }

    public ArrayVarNode(string name, TypeNode elementTypeNode, List<ValueNode> values) : base(name)
    {
        Elements = values;
        Type = new ArrayTypeNode(elementTypeNode.GetFinalTypeNode(), values.Count);
    }

    // For function parameters with arbitrary number of elements
    public ArrayVarNode(string name, TypeNode elementTypeNode) : base(name)
    {
        Type = new ArrayTypeNode(elementTypeNode.GetFinalTypeNode());
    }

    public ArrayVarNode(ArrayTypeNode elementTypeNode)
    {
        Type = elementTypeNode.GetFinalTypeNode();
    }
}

public class GetByIndexNode : ValueNode
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

    public StructVarNode(Dictionary<string, VarNode> fields, StructTypeNode structTypeNode)
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

public class GetFieldNode : ValueNode
{
    public StructVarNode StructVarNode { get; set; }
    public string FieldName { get; set; }
    public VarNode? FieldNode { get; set; }

    public GetFieldNode(StructVarNode structVarNode, string fieldName) : base(structVarNode.GetField(fieldName).Type)
    {
        StructVarNode = structVarNode;
        FieldName = fieldName;
    }

    public GetFieldNode(StructVarNode structVarNode, VarNode fieldNode) : base(structVarNode.GetField(fieldNode.Name!)
        .Type)
    {
        StructVarNode = structVarNode;
        FieldName = fieldNode.Name!;
        FieldNode = fieldNode;
    }
}

public class ArrayFunctions : ValueNode
{
    public ArrayVarNode Array { get; set; }

    public ArrayFunctions(ArrayVarNode arrayVarNode, TypeNode typeNode) : base(typeNode)
    {
        Array = arrayVarNode;
    }
}

public class SortedArrayNode : ArrayFunctions
{
    public SortedArrayNode(ArrayVarNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
}

public class ArraySizeNode : ArrayFunctions
{
    public ArraySizeNode(ArrayVarNode arrayVarNode) : base(arrayVarNode, new TypeNode(MyType.Integer))
    {
    }
}

public class ReversedArrayNode : ArrayFunctions
{
    public ReversedArrayNode(ArrayVarNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
}

public class ParameterNode : TypedSymbolicNode
{
    public VarNode Variable { get; set; }

    public ParameterNode(VarNode variable, TypeNode typeNode) : base(typeNode)
    {
        Variable = variable;
    }
}

public class ParametersNode : SymbolicNode
{
    public List<ParameterNode> Parameters { get; set; } = new List<ParameterNode>();

    public ParametersNode(List<ParameterNode> parameters)
    {
        Parameters = parameters;
    }

    public void AddParameter(ParameterNode parameterNode)
    {
        Parameters.Add(parameterNode);
    }
}

public class FunctionDeclNode : VarNode
{
    public VarNode FunctionName { get; set; }
    public BodyNode Body { get; set; }
    public ParametersNode? Parameters { get; set; }
    public TypeNode? ReturnType { get; set; }

    // For full declaration of function
    public FunctionDeclNode(VarNode functionName, ParametersNode? parameters, TypeNode? returnType, BodyNode body)
    {
        FunctionName = functionName;
        Body = body;
        Parameters = parameters;
        ReturnType = returnType;
    }
}

public class ExpressionNode : ValueNode
{
}

public class ExpressionsNode : SymbolicNode
{
    public List<ValueNode> Expressions { get; }

    public ExpressionsNode(List<ValueNode> expressions)
    {
        Expressions = expressions;
    }

    public ExpressionsNode()
    {
        Expressions = new List<ValueNode>();
    }

    public void AddExpression(ExpressionNode expressionNode)
    {
        Expressions.Add(expressionNode);
    }
}

public class RoutineCallNode : ValueNode
{
    public FunctionDeclNode Function { get; set; }
    public ExpressionsNode? Expressions { get; set; }

    public RoutineCallNode(FunctionDeclNode function, ExpressionsNode expressions) : base(
        function.ReturnType ?? new TypeNode(MyType.Undefined))
    {
        Function = function;
        Expressions = expressions;
    }
}