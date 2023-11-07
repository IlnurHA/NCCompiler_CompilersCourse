using System.Linq.Expressions;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    public void Visit(ComplexNode node)
    {
        throw new NotImplementedException();
        // достать детей
        // выполнить операцию с детьми (action) (вызовется visit детей, который вызовет action детей)
        // выполнить свою опрерацию
    }

    public T VisitLeaf<T>(LeafNode<T> node)
    {
        switch (node.Tag)
        {
            case NodeTag.IntegerLiteral:
            case NodeTag.RealLiteral:
            case NodeTag.BooleanLiteral:
            case NodeTag.Identifier:
            case NodeTag.Unary:
            case NodeTag.PrimitiveType:
                return node.Value;
            default:
                throw new Exception($"Incorrect tag:{node.Tag}");
        }
    }
}