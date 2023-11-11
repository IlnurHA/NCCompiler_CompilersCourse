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


        }
    }





    public SymbolicNode UniversalVisit(Node node)
    {
        return node switch
        {
            ComplexNode complexNode => Visit(complexNode),
            LeafNode<string> leafNode => VisitLeaf(leafNode),
            LeafNode<double> leafNode => VisitLeaf(leafNode),
            LeafNode<int> leafNode => VisitLeaf(leafNode),
            LeafNode<bool> leafNode => VisitLeaf(leafNode),
            _ => throw new Exception($"Cannot find out Node type {node}")
        };
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
                return new OperationNode((node.Value! as string)! == "-" ? OperationType.UnaryMinus : OperationType.UnaryPlus);
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