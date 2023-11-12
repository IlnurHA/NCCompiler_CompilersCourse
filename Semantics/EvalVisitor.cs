using System.Linq.Expressions;
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
                return new GetFieldNode((StructVarNode) modPrimField, (VarNode) idField).GetValueNode();
            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                var arrFromArr = node.Children[0]!.Accept(this);
                var indexFromArr = node.Children[1]!.Accept(this);
                return new GetByIndexNode((ArrayVarNode) arrFromArr, (ValueNode) indexFromArr).GetValueNode();
            case NodeTag.ArrayGetSorted:
                var arrGetSorted = node.Children[0]!.Accept(this);
                if (arrGetSorted.GetType() != typeof(ArrayVarNode))
                    throw new Exception($"Should have got 'ArrayVarNode', got '{arrGetSorted}' instead");
                return new SortedArrayNode((ArrayVarNode) arrGetSorted);
            case NodeTag.ArrayGetSize:
                var arrGetSize = node.Children[0]!.Accept(this);
                return new ArraySizeNode((ArrayVarNode) arrGetSize);
            case NodeTag.ArrayGetReversed:
                var arrGetReversed = node.Children[0]!.Accept(this);
                return new ReversedArrayNode((ArrayVarNode) arrGetReversed);
        }

        throw new Exception("Unimplemented");
    }

    public SymbolicNode VisitLeaf<T>(LeafNode<T> node)
    {
        switch (node.Tag)
        {
            case NodeTag.IntegerLiteral:
                return new ValueNode(node.Value!, new TypeNode(MyType.Integer));
            case NodeTag.RealLiteral:
                return new ValueNode(node.Value!, new TypeNode(MyType.Real));
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