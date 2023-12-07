using System.Diagnostics;
using DotNetGraph.Attributes;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NCCompiler_CompilersCourse.CodeGeneration;
using QUT.Gppg;

namespace NCCompiler_CompilersCourse.Semantics;

public abstract class SymbolicNode
{
    public LexLocation? LexLocation { get; set; }
    public abstract void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue);

    public abstract DotNode BuildGraphNode(DotGraph graph);
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

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        throw new Exception("Cannot draw typed symbolic node!");
    }
}

public class TypeNode : SymbolicNode
{
    public virtual MyType MyType { get; set; }

    public TypeNode(MyType myType)
    {
        MyType = myType;
    }

    public TypeNode()
    {
        MyType = MyType.Undefined;
    }

    public virtual bool IsTheSame(TypeNode anotherObject)
    {
        return MyType == anotherObject.MyType;
    }

    public TypeNode ConvertTo(TypeNode toTypeNode)
    {
        if (this is UserDefinedTypeNode definedTypeNode) return definedTypeNode.Type.ConvertTo(toTypeNode);
        if (toTypeNode is UserDefinedTypeNode userDefinedTypeNode) return ConvertTo(userDefinedTypeNode.Type);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitTypeNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var typeNode = new DotNode()
            .WithShape(DotNodeShape.Ellipse)
            .WithLabel($"TypeNode: {MyType}")
            .WithIdentifier($"TypeNode {MyType} {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        graph.Add(typeNode);
        return typeNode;
    }
}

public class ArrayTypeNode : TypeNode
{
    public TypeNode ElementTypeNode { get; }
    public ValueNode? Size { get; set; }

    public ArrayTypeNode(TypeNode elementTypeNode, ValueNode size) : base(MyType.CompoundType)
    {
        ElementTypeNode = elementTypeNode;
        Size = size;
    }

    public ArrayTypeNode(TypeNode elementTypeNode) : base(MyType.CompoundType)
    {
        ElementTypeNode = elementTypeNode;
    }

    public override bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject is not ArrayTypeNode tempObj) return false;
        return MyType == tempObj.MyType && ElementTypeNode.IsTheSame(tempObj.ElementTypeNode) &&
               (Size == null || tempObj.Size == null ||
                (Size is not null && tempObj.Size is not null &&
                 (Size.Value is null && tempObj.Size.Value is null || 
                  (Size.Value is not null && Size.Value.Equals(tempObj.Size.Value)))));
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitArrayTypeNode(this, commands);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var arrayTypeNode = new DotNode()
            .WithShape(DotNodeShape.Ellipse)
            .WithLabel($"ArrayType")
            .WithIdentifier($"ArrayType {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var elementTypeNode = ElementTypeNode.BuildGraphNode(graph);
        var elementTypeEdge = new DotEdge()
            .From(arrayTypeNode)
            .To(elementTypeNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithLabel("ElementType")
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(elementTypeEdge);
        if (Size != null)
        {
            var sizeNode = Size.BuildGraphNode(graph);
            var sizeEdge = new DotEdge()
                .From(arrayTypeNode)
                .To(sizeNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Dashed)
                .WithLabel("Size")
                .WithPenWidth(1.5);
            graph.Add(sizeEdge);
        }
        graph.Add(arrayTypeNode);
        return arrayTypeNode;
    }
}

public class StructTypeNode : TypeNode
{
    public Dictionary<string, TypeNode> StructFields { get; set; }
    public Dictionary<string, VarNode> DefaultValues { get; set; } = new Dictionary<string, VarNode>();

    public StructTypeNode(Dictionary<string, TypeNode> structFields) : base(MyType.CompoundType)
    {
        StructFields = structFields;
    }

    public override bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject is not StructTypeNode tempObj) return false;
        if (StructFields.Count != tempObj.StructFields.Count) return false;
        foreach (var field in StructFields)
        {
            if (!tempObj.StructFields.ContainsKey(field.Key)) return false;
            if (!field.Value.IsTheSame(tempObj.StructFields[field.Key])) return false;
        }
        
        // foreach (var (key, varNode) in DefaultValues)
        // {
        //     if (!tempObj.DefaultValues.ContainsKey(key)) return false;
        //     if (varNode.Value != tempObj.DefaultValues[key].Value) return false;
        // }

        return true;
    }
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitStructTypeNode(this, commands);
    }
}

public class UserDefinedTypeNode : TypeNode
{
    public TypeNode Type { get; set; }
    public string Name { get; set; }
    public override MyType MyType { get; set; } = MyType.DeclaredType;

    public override bool IsTheSame(TypeNode anotherObject)
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        throw new UnreachableException();
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

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }

    public virtual bool IsTheSameValue(object anotherObject)
    {
        throw new UnreachableException();
    }
    
    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var valueNode = new DotNode()
            .WithShape(DotNodeShape.Circle)
            .WithLabel($"ValueNode: {Type.MyType}")
            .WithIdentifier($"ValueNode {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        graph.Add(valueNode);
        return valueNode;
    }
}

public class ConstNode : ValueNode
{
    public ConstNode(TypeNode typeNode, object value) : base(typeNode, value)
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitConstNode(this, commands);
    }

    public override bool IsTheSameValue(object anotherObject)
    {
        if (anotherObject is not ConstNode constNode) return false;
        return constNode.Value == Value;
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var constantNode = new DotNode()
            .WithShape(DotNodeShape.Ellipse)
            .WithLabel($"ConstantNode")
            .WithIdentifier($"ConstantNode {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var typeNode = Type.BuildGraphNode(graph);
        var typeEdge = new DotEdge()
            .From(constantNode)
            .To(typeNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(typeEdge);
        var valueNode = new DotNode()
            .WithShape(DotNodeShape.Circle)
            .WithLabel($"{Value}")
            .WithIdentifier($"Value {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        graph.Add(valueNode);
        var valueEdge = new DotEdge()
            .From(constantNode)
            .To(valueNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(valueEdge);
        constantNode.SetAttribute("Value", new DotLabelAttribute($"{Value}"));
        graph.Add(constantNode);
        return constantNode;
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

    // public VarNode GetFinalVarNode()
    // {
    //     if (Type is not UserDefinedTypeNode userDefinedTypeNode) return this;
    //     var finalType = userDefinedTypeNode.GetFinalTypeNode();
    //     Type = finalType;
    //     return this;
    // }

    public VarNode()
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitVarNode(this, queue);
    }
    
    public void AcceptStructField(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitStructFieldNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        DotNode variableNode = new DotNode()
            .WithShape(DotNodeShape.Box)
            .WithIdentifier($"Variable {Name} {Guid.NewGuid()}")
            .WithLabel($"Variable {Name}")
            .WithFillColor(DotColor.Coral)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        graph.Add(variableNode);
        return variableNode;
    }
}

public class PrimitiveVarNode : VarNode
{
    public new string Name { get; }

    public PrimitiveVarNode(string name)
    {
        Name = name;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitPrimitiveVarNode(this, queue);
    }
}

public class StatementNode : TypedSymbolicNode
{
    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var statementNode = new DotNode()
            .WithShape(DotNodeShape.Circle)
            .WithLabel($"Statement")
            .WithIdentifier($"Statement {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        graph.Add(statementNode);
        return statementNode;
    }
}

public class BreakNode : StatementNode
{
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitBreakNode(this, commands);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var breakNode =  new DotNode()
            .WithShape(DotNodeShape.Rectangle)
            .WithIdentifier($"Break {Guid.NewGuid()}")
            .WithLabel($"Break")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithPenWidth(1.5);
        graph.Add(breakNode);
        return breakNode;
    }
}

public class AssertNode : StatementNode
{
    public ValueNode LeftExpression { get; }
    public ValueNode RightExpression { get; }

    public AssertNode(ValueNode left, ValueNode right)
    {
        LeftExpression = left;
        RightExpression = right;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitAssertNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var assertNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"Assert")
            .WithIdentifier($"Assert {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var leftExpressionNode = LeftExpression.BuildGraphNode(graph);
        var edgeLeft = new DotEdge()
            .From(assertNode)
            .To(leftExpressionNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Solid)
            .WithPenWidth(1.5);
        graph.Add(edgeLeft);
        var rightExpressionNode = RightExpression.BuildGraphNode(graph);
        var edgeRight = new DotEdge()
            .From(assertNode)
            .To(rightExpressionNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Solid)
            .WithPenWidth(1.5);
        graph.Add(edgeRight);
        graph.Add(assertNode);
        return assertNode;
    }
}

public class ReturnNode : StatementNode
{
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var returnNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"Return: {Type.MyType}")
            .WithIdentifier($"Return {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        graph.Add(returnNode);
        return returnNode;
    }
}

public class DeclarationNode : StatementNode
{
    public VarNode Variable { get; set; }
    public ValueNode? DeclarationValue { get; set; }

    public static VarNode GetAppropriateVarNode(PrimitiveVarNode primitiveVarNode, TypeNode type, ValueNode? value)
    {
        switch (type)
        {
            case UserDefinedTypeNode userDefinedTypeNode:
                return GetAppropriateVarNode(primitiveVarNode, userDefinedTypeNode.GetFinalTypeNode(), value);
            case ArrayTypeNode arrayTypeNode:
                return new ArrayVarNode(arrayTypeNode)
                {
                    Value = value,
                    Name = primitiveVarNode.Name,
                };
            case StructTypeNode structTypeNode:
                var newNode = StructVarNode.FromType(structTypeNode);
                newNode.Name = primitiveVarNode.Name!;
                if (value is not null) newNode.Value = value;
                return newNode;
            case { } typeNode:
                return new VarNode(primitiveVarNode.Name!)
                    {Type = typeNode, Value = value, IsInitialized = value is not null};
        }

        throw new Exception($"Got unexpected type node {type.GetType()}");
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var declarationNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"Declaration")
            .WithIdentifier($"Declaration {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var variableNode = Variable.BuildGraphNode(graph);
        var variableEdge = new DotEdge()
            .From(declarationNode)
            .To(variableNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        if (DeclarationValue != null)
        {
            var valueNode = DeclarationValue.BuildGraphNode(graph);
            graph.Add(variableEdge);
            var valueEdge = new DotEdge()
                .From(declarationNode)
                .To(valueNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Dashed)
                .WithPenWidth(1.5);
            graph.Add(valueEdge);
        }
        graph.Add(declarationNode);
        return declarationNode;
    }
}

public class FullVariableDeclaration : DeclarationNode
{
    public FullVariableDeclaration(PrimitiveVarNode primitiveVarNode, TypeNode type, ValueNode value)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, type, value);
        DeclarationValue = value;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitFullVariableDeclaration(this, queue);
    }
}

public class TypeVariableDeclaration : DeclarationNode
{
    public TypeVariableDeclaration(PrimitiveVarNode primitiveVarNode, TypeNode type)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, type, null);
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitTypeVariableDeclaration(this, queue);
    }
}

public class ValueVariableDeclaration : DeclarationNode
{
    public ValueVariableDeclaration(PrimitiveVarNode primitiveVarNode, ValueNode value)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, value.Type, value);
        DeclarationValue = value;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitValueVariableDeclaration(this, queue);
    }
}

public class TypeDeclarationNode : StatementNode
{
    public UserDefinedTypeNode DeclaredType { get; }

    public TypeDeclarationNode(PrimitiveVarNode varNode, TypeNode type)
    {
        DeclaredType = new UserDefinedTypeNode {Name = varNode.Name, Type = type};
    }
}

public class AssignmentNode : StatementNode
{
    public ValueNode Variable { get; set; }
    public ValueNode AssignmentValue { get; }

    public AssignmentNode(ValueNode varNode, ValueNode value)
    {
        Variable = varNode;
        Variable.Value = value;
        AssignmentValue = value;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitAssignmentNode(this, queue);
    }
    
    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var assignmentNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"Assignment")
            .WithIdentifier($"Assignment {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var variableNode = Variable.BuildGraphNode(graph);
        var variableEdge = new DotEdge()
            .From(assignmentNode)
            .To(variableNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        var valueNode = AssignmentValue.BuildGraphNode(graph);
        graph.Add(variableEdge);
        var valueEdge = new DotEdge()
            .From(assignmentNode)
            .To(valueNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(valueEdge);
        graph.Add(assignmentNode);
        return assignmentNode;
    }
}

public class BodyNode : TypedSymbolicNode
{
    public List<StatementNode> Statements { get; set; }

    public BodyNode(List<StatementNode> statements, TypeNode typeNode)
    {
        Type = typeNode;
        Statements = statements;
    }

    public BodyNode()
    {
        Statements = new List<StatementNode>();
        Type = new TypeNode(MyType.Undefined);
    }

    public void AddStatement(StatementNode statement)
    {
        Statements.Add(statement);
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitBodyNode(this, queue);
    }

    public void Filter(HashSet<string> unusedVariables)
    {
        List<StatementNode> usedBodyStatements = new();
        foreach (var statement in Statements)
        {
            if (statement is DeclarationNode || statement.GetType().IsSubclassOf(typeof(DeclarationNode)))
            {
                if (!unusedVariables.Contains(((DeclarationNode) statement).Variable.Name!))
                    usedBodyStatements.Add(statement);
            }
            else
                usedBodyStatements.Add(statement);
            if (statement is ReturnNode) break;
        }

        Statements = usedBodyStatements;
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var bodyNode =  new DotNode()
            .WithShape(DotNodeShape.Rectangle)
            .WithIdentifier($"Body {Guid.NewGuid()}")
            .WithLabel($"Body")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithPenWidth(1.5);
        foreach (var statement in Statements)
        {
            DotNode statementNode = statement.BuildGraphNode(graph);
            var edge = new DotEdge()
                .From(bodyNode)
                .To(statementNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Solid)
                .WithPenWidth(1.5);
            graph.Add(edge);
        }
        graph.Add(bodyNode);
        return bodyNode;
    }
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitOperationNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var operationNode = new DotNode()
            .WithShape(DotNodeShape.Rectangle)
            .WithLabel($"Operation {OperationType}")
            .WithIdentifier($"Operation {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var operandsNode = new DotNode()
            .WithShape(DotNodeShape.Ellipse)
            .WithLabel($"Operands")
            .WithIdentifier($"Operands {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        graph.Add(operandsNode);
        var operandsEdge = new DotEdge()
            .From(operationNode)
            .To(operandsNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(operandsEdge);
        foreach (var operand in Operands)
        {
            var operandNode = operand.BuildGraphNode(graph);
            var operandEdge = new DotEdge()
                .From(operandsNode)
                .To(operandNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Solid)
                .WithPenWidth(1.5);
            graph.Add(operandEdge);
        }
        graph.Add(operationNode);
        return operationNode;
    }
}

public class ArrayVarNode : VarNode
{
    public new ArrayTypeNode Type { get; }
    public List<ValueNode> Elements = new();

    public ArrayVarNode(string name, TypeNode elementTypeNode, ValueNode size) : base(name)
    {
        Type = new ArrayTypeNode(elementTypeNode, size);
        base.Type = Type;
    }

    public ArrayVarNode(string name, TypeNode elementTypeNode, List<ValueNode> values) : base(name)
    {
        Elements = values;
        Type = new ArrayTypeNode(elementTypeNode.GetFinalTypeNode(),
            new ValueNode(new TypeNode(MyType.Integer), values.Count));
        base.Type = Type;
    }

    // For function parameters with arbitrary number of elements
    public ArrayVarNode(string name, TypeNode elementTypeNode) : base(name)
    {
        Type = new ArrayTypeNode(elementTypeNode.GetFinalTypeNode());
        base.Type = Type;
    }

    public ArrayVarNode(ArrayTypeNode elementTypeNode)
    {
        Type = elementTypeNode;
        base.Type = Type;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayVarNode(this, queue);
    }
    
    public void AcceptByValue(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayVarByValueNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var arrayVarNode = new DotNode()
            .WithShape(DotNodeShape.Ellipse)
            .WithLabel($"ArrayVar {Name}")
            .WithIdentifier($"ArrayVar {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var arrayTypeNode = Type.BuildGraphNode(graph);
        var arrayTypeEdge = new DotEdge()
            .From(arrayVarNode)
            .To(arrayTypeNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithLabel("ArrayType")
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(arrayTypeEdge);
        if (Elements.Count != 0)
        {
            var elementsNode = new DotNode()
                .WithShape(DotNodeShape.Ellipse)
                .WithLabel($"Elements")
                .WithIdentifier($"Elements {Guid.NewGuid()}")
                .WithFillColor(DotColor.Black)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotNodeStyle.Solid)
                .WithWidth(0.5)
                .WithHeight(0.5)
                .WithPenWidth(1.5);
            graph.Add(elementsNode);
            var elementsEdge = new DotEdge()
                .From(arrayVarNode)
                .To(elementsNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Dashed)
                .WithPenWidth(1.5);
            graph.Add(elementsEdge);
            foreach (var element in Elements)
            {
                var elementNode = element.BuildGraphNode(graph);
                var elementEdge = new DotEdge()
                    .From(elementsNode)
                    .To(elementNode)
                    .WithArrowHead(DotEdgeArrowType.Box)
                    .WithArrowTail(DotEdgeArrowType.Diamond)
                    .WithColor(DotColor.Red)
                    .WithFontColor(DotColor.Black)
                    .WithStyle(DotEdgeStyle.Dashed)
                    .WithPenWidth(1.5);
                graph.Add(elementEdge);
            }
        }
        graph.Add(arrayVarNode);
        return arrayVarNode;
    }
}

public class GetByIndexNode : ValueNode
{
    public ValueNode ArrayVarNode { get; set; }
    public ValueNode Index { get; set; }
    
    public TypeNode GetTypeNode()
    {
        if (ArrayVarNode is ArrayVarNode arrayVarNode) return arrayVarNode.Type.ElementTypeNode;
        if (ArrayVarNode is GetByIndexNode getByIndexNode)
            return ((ArrayTypeNode) getByIndexNode.GetTypeNode()).ElementTypeNode;
        if (ArrayVarNode is GetFieldNode getFieldNode)
            return ((ArrayTypeNode) getFieldNode.GetTypeNode()).ElementTypeNode;
        throw new Exception($"Unexpected type: {ArrayVarNode.GetType()}");
    }

    public GetByIndexNode(ValueNode varNode, ValueNode index)
    {
        ArrayVarNode = varNode;
        Index = index;
        Type = GetTypeNode();
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitGetByIndexNode(this, queue);
    }

    public void AcceptSetByIndex(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitSetByIndex(this, queue);
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

    public static StructVarNode FromType(StructTypeNode structTypeNode)
    {
        var newDict = new Dictionary<string, VarNode>();

        foreach (var (key, value) in structTypeNode.StructFields)
        {
            newDict[key] = value switch
            {
                ArrayTypeNode arrayTypeNode => new ArrayVarNode(arrayTypeNode),
                StructTypeNode structTypeNode2 => FromType(structTypeNode2),
                { } node => new VarNode
                {
                    IsInitialized = false,
                    Type = node,
                }
            };
        }

        return new StructVarNode(newDict, structTypeNode);
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitStructVarNode(this, queue);
    }
    
    public void AcceptByValue(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitStructVarByValueNode(this, queue);
    }
}

public class GetFieldNode : VarNode
{
    public ValueNode StructVarNode { get; set; }
    public string FieldName { get; set; }
    public VarNode? FieldNode { get; set; }

    public TypeNode GetTypeNode()
    {
        if (StructVarNode is StructVarNode structVarNode) return structVarNode.GetField(FieldName).Type.GetFinalTypeNode();
        if (StructVarNode is GetFieldNode getFieldNode)
            return ((StructTypeNode) getFieldNode.GetTypeNode().GetFinalTypeNode()).StructFields[FieldName];
        if (StructVarNode is GetByIndexNode getByIndexNode)
            return ((StructTypeNode) getByIndexNode.GetTypeNode().GetFinalTypeNode()).StructFields[FieldName];
        throw new Exception($"Unexpected type: {StructVarNode.GetType()}");
    }

    public GetFieldNode(StructVarNode structVarNode, string fieldName) : base(structVarNode.Name!)
    {
        StructVarNode = structVarNode;
        FieldName = fieldName;
        Type = structVarNode.GetField(fieldName).Type;
    }

    public GetFieldNode(ValueNode structVarNode, PrimitiveVarNode fieldNode)
    {
        StructVarNode = structVarNode;
        FieldName = fieldNode.Name;
        FieldNode = fieldNode;
        Type = GetTypeNode();
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitGetFieldNode(this, queue);
    }

    public void AcceptSetField(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitSetFieldNode(this, queue);
    }
}

public class ArrayFunctions : ValueNode
{
    public ValueNode Array { get; set; }

    public ArrayFunctions(ValueNode arrayVarNode, TypeNode typeNode) : base(typeNode)
    {
        Array = arrayVarNode;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayFunctions(this, queue);
    }
}

public class SortedArrayNode : ArrayFunctions
{
    public SortedArrayNode(ValueNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitSortedArrayNode(this, queue);
    }
}

public class ArraySizeNode : ArrayFunctions
{
    public ArraySizeNode(ValueNode arrayVarNode) : base(arrayVarNode, new TypeNode(MyType.Integer))
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArraySizeNode(this, queue);
    }
}

public class ReversedArrayNode : ArrayFunctions
{
    public ReversedArrayNode(ValueNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitReversedArrayNode(this, queue);
    }
}

public class ParameterNode : TypedSymbolicNode
{
    public VarNode Variable { get; set; }

    public ParameterNode(VarNode variable, TypeNode typeNode) : base(typeNode)
    {
        Variable = variable;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        DotNode parameterNode = Variable.BuildGraphNode(graph);
        DotNode typeNode = Type.BuildGraphNode(graph);
        var typeEdge = new DotEdge()
            .From(parameterNode)
            .To(typeNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(typeEdge);
        graph.Add(parameterNode);
        return parameterNode;
    }
}

public class ParametersNode : SymbolicNode
{
    public List<ParameterNode> Parameters { get; set; }

    public ParametersNode(List<ParameterNode> parameters)
    {
        Parameters = parameters;
    }

    public void AddParameter(ParameterNode parameterNode)
    {
        Parameters.Add(parameterNode);
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitParametersNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        DotNode parametersNode = new DotNode()
            .WithShape(DotNodeShape.Box)
            .WithLabel("Parameters")
            .WithIdentifier($"Parameters {Guid.NewGuid()}")
            .WithFillColor(DotColor.Coral)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        foreach (var parameter in Parameters)
        {
            DotNode parameterNode = parameter.BuildGraphNode(graph);
            var edge = new DotEdge()
                .From(parametersNode)
                .To(parameterNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Solid)
                .WithPenWidth(1.5);
            graph.Add(edge);
        }
        graph.Add(parametersNode);
        return parametersNode;
    }

    public void Filter(HashSet<string> unusedVariables)
    {
        Parameters = Parameters.Where(parameter => !unusedVariables.Contains(parameter.Variable.Name!)).ToList();
    }
}

public class RoutineDeclarationNode : VarNode
{
    public PrimitiveVarNode FunctionName { get; }
    public BodyNode Body { get; }
    public ParametersNode? Parameters { get; }
    public TypeNode? ReturnType { get; }

    // For full declaration of function
    public RoutineDeclarationNode(PrimitiveVarNode functionName, ParametersNode? parameters, TypeNode? returnType, BodyNode body)
    {
        FunctionName = functionName;
        Body = body;
        Parameters = parameters;
        ReturnType = returnType;
        Name = FunctionName.Name;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitRoutineDeclarationNode(this, queue);
    }
    
    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var routineDeclarationNode =  new DotNode()
            .WithShape(DotNodeShape.Rectangle)
            .WithIdentifier($"Routine {FunctionName.Name}")
            .WithLabel($"Routine: {FunctionName.Name}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithPenWidth(1.5);
        DotNode routineDeclarationBody = Body.BuildGraphNode(graph);
        var edgeBody = new DotEdge()
            .From(routineDeclarationNode)
            .To(routineDeclarationBody)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Solid)
            .WithPenWidth(1.5);
        graph.Add(edgeBody);
        if (Parameters != null)
        {
            var routineParametersNode = Parameters!.BuildGraphNode(graph);
            routineParametersNode.SetAttribute("label", new DotLabelAttribute("Parameters"));
            var routineParametersEdge = new DotEdge()
                .From(routineDeclarationNode)
                .To(routineParametersNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Dashed)
                .WithPenWidth(1.5);
            graph.Add(routineParametersEdge);
        }

        if (ReturnType != null)
        {
            DotNode returnTypeNode = ReturnType!.BuildGraphNode(graph);
            returnTypeNode.SetAttribute("label", new DotLabelAttribute($"ReturnType: {Type.MyType}"));
            var returnTypeEdge = new DotEdge()
                .From(routineDeclarationNode)
                .To(returnTypeNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Dashed)
                .WithPenWidth(1.5);
            graph.Add(returnTypeEdge);
        }

        graph.Add(routineDeclarationNode);
        return routineDeclarationNode;
    }
}

public class ExpressionsNode : TypedSymbolicNode
{
    public List<ValueNode> Expressions { get; }

    public ExpressionsNode(List<ValueNode> expressions) : base(expressions[0].Type)
    {
        Expressions = expressions;
    }

    public ExpressionsNode()
    {
        Expressions = new List<ValueNode>();
    }

    public void AddExpression(ValueNode expressionNode)
    {
        Expressions.Add(expressionNode);
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitExpressionsNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var expressionsNode = new DotNode()
            .WithShape(DotNodeShape.Ellipse)
            .WithIdentifier($"Expressions {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        foreach (var expression in Expressions)
        {
            var expressionNode = expression.BuildGraphNode(graph);
            var returnTypeEdge = new DotEdge()
                .From(expressionsNode)
                .To(expressionNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Solid)
                .WithPenWidth(1.5);
            graph.Add(returnTypeEdge);
        }
        graph.Add(expressionsNode);
        return expressionsNode;
    }
}

public class PrintNode : StatementNode
{
    public ExpressionsNode? Expressions { get; set; }
    
    public PrintNode(ExpressionsNode expressions)
    {
        Expressions = expressions;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitPrintNode(this, queue);
    }
}

public class RoutineCallNode : ValueNode
{
    public RoutineDeclarationNode Routine { get; set; }
    public ExpressionsNode? Expressions { get; set; }

    public RoutineCallNode(RoutineDeclarationNode routine, ExpressionsNode expressions) : base(
        routine.ReturnType ?? new TypeNode(MyType.Undefined))
    {
        Routine = routine;
        Expressions = expressions;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitRoutineCallNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var routineCallNode = new DotNode()
            .WithShape(DotNodeShape.Ellipse)
            .WithLabel($"RoutineCall {Routine.Name}")
            .WithIdentifier($"RoutineCall {Routine.Name} {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var callArgumentsNode = Expressions!.BuildGraphNode(graph);
        callArgumentsNode.SetAttribute("label", new DotLabelAttribute("Arguments"));
        var callArgumentsEdge = new DotEdge()
            .From(routineCallNode)
            .To(callArgumentsNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(callArgumentsEdge);
        graph.Add(routineCallNode);
        return routineCallNode;
    }
}

public class RangeNode : SymbolicNode
{
    public ValueNode LeftBound { get; }
    public ValueNode RightBound { get; }

    public bool Reversed { get; }

    public RangeNode(ValueNode leftBound, ValueNode rightBound, bool reversed = false)
    {
        LeftBound = leftBound;
        RightBound = rightBound;
        Reversed = reversed;
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitRangeNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        DotNode rangeNode = new DotNode()
            .WithShape(DotNodeShape.Box)
            .WithLabel($"Range")
            .WithIdentifier($"Range {Guid.NewGuid()}")
            .WithFillColor(DotColor.Coral)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var leftBoundNode = LeftBound.BuildGraphNode(graph);
        leftBoundNode.SetAttribute("label", new DotLabelAttribute($"Left Bound: {LeftBound.Type.MyType}"));
        var leftBoundEdge = new DotEdge()
            .From(rangeNode)
            .To(leftBoundNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(leftBoundEdge);
        var rightBoundNode = RightBound.BuildGraphNode(graph);
        rightBoundNode.SetAttribute("label", new DotLabelAttribute($"Right Bound: {RightBound.Type.MyType}"));
        var rightBoundEdge = new DotEdge()
            .From(rangeNode)
            .To(rightBoundNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(rightBoundEdge);
        graph.Add(rangeNode);
        return rangeNode;
    }
}

public class ForLoopNode : StatementNode
{
    public VarNode IdName { get; }
    public RangeNode Range { get; }
    public BodyNode Body { get; }

    public ForLoopNode(VarNode idName, RangeNode range, BodyNode body)
    {
        IdName = idName;
        Range = range;
        Body = body;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitForLoopNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var forLoopNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"ForLoop")
            .WithIdentifier($"ForLoop {Guid.NewGuid()}")
            .WithFillColor(DotColor.Coral)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var identifierNode = IdName.BuildGraphNode(graph);
        var identifierEdge = new DotEdge()
            .From(forLoopNode)
            .To(identifierNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithLabel("Identifier")
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(identifierEdge);
        var rangeNode = Range.BuildGraphNode(graph);
        var rangeEdge = new DotEdge()
            .From(forLoopNode)
            .To(rangeNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(rangeEdge);
        var bodyNode = Body.BuildGraphNode(graph);
        var bodyEdge = new DotEdge()
            .From(forLoopNode)
            .To(bodyNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(bodyEdge);
        graph.Add(forLoopNode);
        return forLoopNode;
    }
}

public class ForEachLoopNode : StatementNode
{
    public VarNode IdName { get; }
    public ArrayVarNode Array { get; }
    public BodyNode Body { get; }

    public ForEachLoopNode(VarNode idName, ArrayVarNode array, BodyNode body)
    {
        IdName = idName;
        Array = array;
        Body = body;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitForEachLoopNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var forLoopNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"ForEachLoop")
            .WithIdentifier($"ForEachLoop {Guid.NewGuid()}")
            .WithFillColor(DotColor.Coral)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var identifierNode = IdName.BuildGraphNode(graph);
        var identifierEdge = new DotEdge()
            .From(forLoopNode)
            .To(identifierNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithLabel("Identifier")
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(identifierEdge);
        var arrayNode = Array.BuildGraphNode(graph);
        var arrayEdge = new DotEdge()
            .From(forLoopNode)
            .To(arrayNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(arrayEdge);
        var bodyNode = Body.BuildGraphNode(graph);
        var bodyEdge = new DotEdge()
            .From(forLoopNode)
            .To(bodyNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(bodyEdge);
        graph.Add(forLoopNode);
        return forLoopNode;
    }
}

public class WhileLoopNode : StatementNode
{
    public ValueNode Condition { get; }
    public BodyNode Body { get; }

    public WhileLoopNode(ValueNode condition, BodyNode body)
    {
        Condition = condition;
        Body = body;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitWhileLoopNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var whileLoopNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"WhileLoop")
            .WithIdentifier($"WhileLoop {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var conditionNode = Condition.BuildGraphNode(graph);
        conditionNode.SetAttribute("label", new DotLabelAttribute("Condition"));
        var conditionEdge = new DotEdge()
            .From(whileLoopNode)
            .To(conditionNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(conditionEdge);
        var bodyNode = Body.BuildGraphNode(graph);
        conditionNode.SetAttribute("label", new DotLabelAttribute("WhileBody"));
        var bodyEdge = new DotEdge()
            .From(whileLoopNode)
            .To(bodyNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(bodyEdge);
        graph.Add(whileLoopNode);
        return whileLoopNode;
    }
}

public class IfStatement : StatementNode
{
    public ValueNode Condition { get; }
    public BodyNode Body { get; }

    public IfStatement(ValueNode condition, BodyNode body)
    {
        Condition = condition;
        Body = body;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitIfStatement(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var ifNode = new DotNode()
            .WithShape(DotNodeShape.Diamond)
            .WithLabel($"If")
            .WithIdentifier($"If {Guid.NewGuid()}")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        var conditionNode = Condition.BuildGraphNode(graph);
        var conditionEdge = new DotEdge()
            .From(ifNode)
            .To(conditionNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithLabel("Condition")
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(conditionEdge);
        var bodyNode = Body.BuildGraphNode(graph);
        bodyNode.SetAttribute("label", new DotLabelAttribute("IfBody"));
        var bodyEdge = new DotEdge()
            .From(ifNode)
            .To(bodyNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(bodyEdge);
        graph.Add(ifNode);
        return ifNode;
    }
}

public class IfElseStatement : IfStatement
{
    public BodyNode BodyElse { get; }

    public IfElseStatement(ValueNode condition, BodyNode body, BodyNode bodyElse) : base(condition, body)
    {
        BodyElse = bodyElse;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitIfElseStatement(this, queue);
    }
}

public class ArrayConst : ValueNode
{
    public ExpressionsNode Expressions { get; }

    public ArrayConst(ExpressionsNode expressions) : base(expressions.Type)
    {
        Expressions = expressions;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayConst(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var arrayConstNode = base.BuildGraphNode(graph)
            .WithLabel($"ArrayConst");
        var elementsNode = Expressions.BuildGraphNode(graph)
                .WithLabel($"Elements");
        graph.Add(elementsNode);
        var elementsEdge = new DotEdge()
            .From(arrayConstNode)
            .To(elementsNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithPenWidth(1.5);
        graph.Add(elementsEdge);
        graph.Add(arrayConstNode);
        return arrayConstNode;
    }
}

public class ProgramNode : SymbolicNode
{
    public List<SymbolicNode> Declarations { get; } = new();
    private bool _hasMainFlag;

    public void AddDeclaration(SymbolicNode node)
    {
        Declarations.Add(node);
    }

    public bool HasMain()
    {
        return _hasMainFlag;
    }
    
    public void SetHasMain()
    {
        _hasMainFlag = true;
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitProgramNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var programNode = new DotNode()
            .WithShape(DotNodeShape.Box)
            .WithLabel("Program")
            .WithIdentifier("Program")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        foreach (var declaration in Declarations)
        {
            DotNode declarationNode = declaration.BuildGraphNode(graph);
            var edge = new DotEdge()
                .From(programNode)
                .To(declarationNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Solid)
                .WithPenWidth(1.5);
            graph.Add(edge);
        }
        graph.Add(programNode);
        return programNode;
    }

    public DotGraph BuildGraph()
    {
        var graph = new DotGraph().WithIdentifier("MyGraph");
        BuildGraphNode(graph);
        return graph;
    }
}

public class VariableDeclarations : SymbolicNode
{
    public Dictionary<string, VarNode> Declarations { get; }
    public List<DeclarationNode> DeclarationNodes { get; set; } = new List<DeclarationNode>();

    public VariableDeclarations(Dictionary<string, VarNode> declarations)
    {
        Declarations = declarations;
    }

    public void AddDeclaration(VarNode varNode)
    {
        if (Declarations.ContainsKey(varNode.Name!))
        {
            throw new Exception("Repeated fields");
        }

        Declarations[varNode.Name!] = varNode;
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitVariableDeclarations(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var variableDeclarationsNode = new DotNode()
            .WithShape(DotNodeShape.Box)
            .WithLabel($"Variable Declarations: {Guid.NewGuid()}")
            .WithIdentifier($"Variable Declarations")
            .WithFillColor(DotColor.Black)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotNodeStyle.Solid)
            .WithWidth(0.5)
            .WithHeight(0.5)
            .WithPenWidth(1.5);
        foreach (var declaration in Declarations)
        {
            DotNode variableDeclarationNode = declaration.Value.BuildGraphNode(graph); 
            variableDeclarationNode.SetAttribute("Variable name", new DotAttribute($"{declaration.Key}"));
            var edge = new DotEdge()
                .From(variableDeclarationsNode)
                .To(variableDeclarationNode)
                .WithArrowHead(DotEdgeArrowType.Box)
                .WithArrowTail(DotEdgeArrowType.Diamond)
                .WithColor(DotColor.Red)
                .WithFontColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Solid)
                .WithPenWidth(1.5);
            graph.Add(edge);
        }
        graph.Add(variableDeclarationsNode);
        return variableDeclarationsNode;
    }
}

public class EmptyReturnNode : ReturnNode
{
    public EmptyReturnNode()
    {
        Type = new TypeNode(MyType.Undefined);
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitEmptyReturnNode(this, queue);
    }
}

public class ValueReturnNode : ReturnNode
{
    public ValueNode Value { get; }

    public ValueReturnNode(ValueNode value)
    {
        Value = value;
        Type = value.Type;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitValueReturnNode(this, queue);
    }

    public override DotNode BuildGraphNode(DotGraph graph)
    {
        var returnNode = base.BuildGraphNode(graph);
        var valueNode = Value.BuildGraphNode(graph);
        var valueEdge = new DotEdge()
            .From(returnNode)
            .To(valueNode)
            .WithArrowHead(DotEdgeArrowType.Box)
            .WithArrowTail(DotEdgeArrowType.Diamond)
            .WithColor(DotColor.Red)
            .WithFontColor(DotColor.Black)
            .WithStyle(DotEdgeStyle.Dashed)
            .WithLabel("Return Value")
            .WithPenWidth(1.5);
        graph.Add(valueEdge);
        return returnNode;
    }
}

public class CastNode : ValueNode
{
    public CastNode(TypeNode type, ValueNode value) : base(type, value) { }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitCastNode(this, queue);
    }
}