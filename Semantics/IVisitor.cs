using System.Numerics;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

interface IVisitor
{
    SymbolicNode ModifiablePrimaryVisit(ComplexNode node);
    SymbolicNode StatementVisit(ComplexNode node);
    SymbolicNode RoutineVisit(ComplexNode node);
    SymbolicNode ExpressionVisit(ComplexNode node);
    SymbolicNode VisitLeaf<T>(LeafNode<T> node);
}
