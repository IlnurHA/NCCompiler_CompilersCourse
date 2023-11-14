﻿using System.Linq.Expressions;
using System.Numerics;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    public ScopeStack ScopeStack { get; set; } = new();

    public SymbolicNode Visit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.ModifiablePrimaryGettingField:
                var modPrimField = node.Children[0]!.Accept(this);
                var idField = node.Children[1]!.Accept(this);
                return new GetFieldNode((StructVarNode)modPrimField, (VarNode)idField).GetValueNode();
            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                var arrFromArr = node.Children[0]!.Accept(this);
                var indexFromArr = node.Children[1]!.Accept(this);
                return new GetByIndexNode((ArrayVarNode)arrFromArr, (ValueNode)indexFromArr).GetValueNode();
            case NodeTag.ArrayGetSorted:
                var arrGetSorted = node.Children[0]!.Accept(this);
                if (arrGetSorted.GetType() != typeof(ArrayVarNode))
                    throw new Exception($"Should have got 'ArrayVarNode', got '{arrGetSorted}' instead");
                return new SortedArrayNode((ArrayVarNode)arrGetSorted);
            case NodeTag.ArrayGetSize:
                var arrGetSize = node.Children[0]!.Accept(this);
                return new ArraySizeNode((ArrayVarNode)arrGetSize);
            case NodeTag.ArrayGetReversed:
                var arrGetReversed = node.Children[0]!.Accept(this);
                return new ReversedArrayNode((ArrayVarNode)arrGetReversed);
            case NodeTag.VariableDeclarationFull:
            case NodeTag.VariableDeclarationIdenType:
            case NodeTag.VariableDeclarationIdenExpr:
                VarNode variableIdentifier = (VarNode)node.Children[0]!.Accept(this);
                TypeNode? variableType = null;
                ValueNode? value = null;
                switch (node.Tag)
                {
                    case NodeTag.VariableDeclarationFull:
                        variableType = (TypeNode)node.Children[1]!.Accept(this);
                        value = (ValueNode)node.Children[2]!.Accept(this);
                        break;
                    case NodeTag.VariableDeclarationIdenType:
                        variableType = (TypeNode)node.Children[1]!.Accept(this);
                        break;
                    case NodeTag.VariableDeclarationIdenExpr:
                        value = (ValueNode)node.Children[1]!.Accept(this);
                        break;
                }
                
                using (var scope = ScopeStack.GetLastScope())
                {
                    if (variableIdentifier.Name == null || !scope.IsFree(variableIdentifier.Name)) 
                        throw new Exception($"The variable with name {variableIdentifier.Name} already exists in this scope!");
                    if (value != null)
                    {
                        variableIdentifier = variableIdentifier.GetFinalVarNode();
                        value = value.GetFinalValueNode();
                        if (variableType != null && _isConvertible(variableType, value.Type))
                            throw new Exception($"Unexpected type of value for variable. Given type: {value.Type}");
                        variableIdentifier.Value = value;
                        scope.AddVariable(variableIdentifier);
                    }
                }
                
                return new DeclarationNode(variableIdentifier, value);
            case NodeTag.TypeDeclaration:
                VarNode typeIdentifier = (VarNode)node.Children[0]!.Accept(this);
                TypeNode typeSynonym = ((TypeNode)node.Children[1]!.Accept(this)).GetFinalTypeNode();
                typeIdentifier.Value = typeSynonym;
                using (var scope = ScopeStack.GetLastScope())
                {
                    if (typeIdentifier.Name == null || !scope.IsFree(typeIdentifier.Name))
                        throw new Exception(
                            $"The user type with name {typeIdentifier.Name} already exists in this scope!");
                    scope.AddVariable(typeIdentifier);
                }

                return new TypeDeclarationNode(typeIdentifier, typeSynonym);
            
            case NodeTag.Break:
                return new BreakNode();
            case NodeTag.Assert:
                ValueNode? leftAssertExpression = (ValueNode) node.Children[0]!.Accept(this);
                ValueNode? rightAssertExpression = (ValueNode) node.Children[1]!.Accept(this);
                return new AssertNode(leftAssertExpression, rightAssertExpression);
            case NodeTag.Return:
                ValueNode returnValue = (ValueNode) node.Children[0]!.Accept(this);
                return new ReturnNode(returnValue);
            
        }

        throw new Exception("Unimplemented");
    }

    // public SymbolicNode VisitWithCast(ComplexNode node)
    // {
    //     switch (node.Tag)
    //     {
    //         case NodeTag.Assignment:
    //             
    //     }
    // }

    public SymbolicNode VisitLeaf<T>(LeafNode<T> node)
    {
        switch (node.Tag)
        {
            case NodeTag.IntegerLiteral:
                return new ConstNode(node.Value!, new TypeNode(MyType.Integer));
            case NodeTag.RealLiteral:
                return new ConstNode(node.Value!, new TypeNode(MyType.Real));
            case NodeTag.Identifier:
                return new VarNode((node.Value! as string)!);
            case NodeTag.PrimitiveType:
                return new TypeNode(_getPrimitiveType((node.Value! as string)!));
            case NodeTag.Unary:
                return new OperationNode((node.Value! as string)! == "-"
                    ? OperationType.UnaryMinus
                    : OperationType.UnaryPlus);
            default:
                throw new Exception($"Unexpected node tag for visiting Leaf node {node.Tag}");
        }
    }

    private bool _isConvertible(TypeNode var1, TypeNode var2)
    {
        if (var1.IsTheSame(var2)) return true;
        return (var1.MyType, var2.MyType) switch
        {
            (MyType.Integer, MyType.Real) => true,
            (MyType.Integer, MyType.Boolean) => true,
            (MyType.Real, MyType.Real) => true,
            (MyType.Real, MyType.Integer) => true,
            (MyType.Real, MyType.Boolean) => true,
            (MyType.Boolean, MyType.Integer) => true,
            _ => false
        };
    }

    private MyType _getPrimitiveType(string primitiveType)
    {
        return primitiveType switch
        {
            "integer" => MyType.Integer,
            "boolean" => MyType.Boolean,
            "real" => MyType.Real,
            _ => throw new Exception($"Unexpected type of primitive type {primitiveType}")
        };
    }
}