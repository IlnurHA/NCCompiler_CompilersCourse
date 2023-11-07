using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

interface IVisitor
{
    void Visit(ComplexNode node);
    T VisitLeaf<T>(LeafNode<T> node);
}
