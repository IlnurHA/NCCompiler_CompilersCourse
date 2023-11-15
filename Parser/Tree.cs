using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.Parser;

internal enum NodeTag
{
    ProgramSimpleDeclaration,
    ProgramRoutineDeclaration,
    VariableDeclarationFull,
    VariableDeclarationIdenType,
    VariableDeclarationIdenExpr,
    TypeDeclaration,
    RoutineDeclaration,
    RoutineDeclarationWithType,
    ParametersContinuous,
    ParameterDeclaration,
    PrimitiveType,
    RecordType,
    VariableDeclarations,
    ArrayType,
    BodySimpleDeclaration,
    BodyStatement,
    Assignment,
    RoutineCall,
    ExpressionsContinuous,
    WhileLoop,
    ForLoop,
    Range,
    RangeReverse,
    ForeachLoop,
    IfStatement,
    IfElseStatement,
    Cast,
    ModifiablePrimaryGettingField,
    ModifiablePrimaryGettingValueFromArray,
    Assert,
    Identifier,
    IntegerLiteral,
    RealLiteral,
    BooleanLiteral,
    SignToInteger,
    SignToDouble,
    ArrayConst,
    And,
    Or,
    Xor,
    Ge,
    Gt,
    Le,
    Lt,
    Eq,
    Ne,
    Plus,
    Minus,
    Mul,
    Div,
    Rem,
    Unary,
    Return,
    RoutineDeclarationWithTypeAndParams,
    RoutineDeclarationWithParams,
    Break,
    ArrayTypeWithoutSize,
    NotExpression,
    ArrayGetSize,
    ArrayGetReversed,
    ArrayGetSorted,
}

internal abstract class Node
{
    public static ComplexNode MakeComplexNode(NodeTag nodeTag, params Node[] children)
    {
        return new ComplexNode(nodeTag, children);
    }

    readonly NodeTag _tag;
    public NodeTag Tag => _tag;

    // public NodeTag Tag { get; set; }

    protected Node(NodeTag tag)
    {
        _tag = tag;
    }

    public abstract SymbolicNode Accept(IVisitor visitor);
}

internal class ComplexNode : Node
{
    public Node?[] Children { get; }

    public ComplexNode(NodeTag nodeTag, params Node?[] nodes) : base(nodeTag)
    {
        Children = nodes;
    }

    public override SymbolicNode Accept(IVisitor visitor)
    {
        // TODO - should call the correct method based on the tag
        switch (Tag)
        {
            case NodeTag.ModifiablePrimaryGettingField:
            case NodeTag.ModifiablePrimaryGettingValueFromArray:
            case NodeTag.ArrayGetSorted:
            case NodeTag.ArrayGetSize:
            case NodeTag.ArrayGetReversed:
                return visitor.ModifiablePrimaryVisit(this);
            case NodeTag.VariableDeclarationFull:
            case NodeTag.VariableDeclarationIdenType:
            case NodeTag.VariableDeclarationIdenExpr:
            case NodeTag.TypeDeclaration:
            case NodeTag.Break:
            case NodeTag.Assert:
            case NodeTag.Return:
                return visitor.StatementVisit(this);
            case NodeTag.RoutineDeclarationWithTypeAndParams or NodeTag.RoutineDeclarationWithType:
            case NodeTag.ParameterDeclaration:
            case NodeTag.ParametersContinuous:
            case NodeTag.RoutineCall:
                return visitor.RoutineVisit(this);
            case NodeTag.And:
            case NodeTag.Or:
            case NodeTag.Xor:
            case NodeTag.Le:
            case NodeTag.Lt:
            case NodeTag.Ge:
            case NodeTag.Gt:
            case NodeTag.Eq:
            case NodeTag.Ne:
            case NodeTag.Plus:
            case NodeTag.Minus:
            case NodeTag.Mul:
            case NodeTag.Div:
            case NodeTag.Rem:
            case NodeTag.NotExpression:
            case NodeTag.SignToInteger:
            case NodeTag.SignToDouble:
                return visitor.ExpressionVisit(this);
            default:
                throw new Exception($"Unexpected NodeTag {Tag} in the visit function");
        }
    }

    internal class LeafNode<T> : Node
    {
        public T Value { get; }

        public LeafNode(NodeTag nodeTag, T value) : base(nodeTag)
        {
            Value = value;
        }

        public override SymbolicNode Accept(IVisitor visitor)
        {
            return visitor.VisitLeaf(this);
        }
    }
}