using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

interface IVisitor
{
    void Visit(ComplexNode node);
    T VisitLeaf<T>(LeafNode<T> node);

    void Action(Node node);
    // void VisitLeaf(LeafNode<string> node);
    // void VisitLeaf(LeafNode<Double> node);
    // void VisitLeaf(LeafNode<Boolean> node);
}
