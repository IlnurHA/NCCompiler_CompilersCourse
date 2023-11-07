using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

interface IVisitor
{
    SymbolicNode Visit(ComplexNode node);
    SymbolicNode VisitLeaf<T>(LeafNode<T> node);
}
