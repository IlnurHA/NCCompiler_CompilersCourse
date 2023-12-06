using NCCompiler_CompilersCourse.Semantics;
using QUT.Gppg;

namespace NCCompiler_CompilersCourse.Parser;

public enum NodeTag
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
    EmptyArrayConst,
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
    Print,
}

public abstract class Node
{
    public static ComplexNode MakeComplexNode(NodeTag nodeTag, LexLocation lexLocation, params Node[] children)
    {
        return new ComplexNode(nodeTag, lexLocation, children);
    }

    readonly NodeTag _tag;
    public NodeTag Tag => _tag;

    // public NodeTag Tag { get; set; }

    public LexLocation LexLocation { get; }

    protected Node(NodeTag tag, LexLocation lexLocation)
    {
        _tag = tag;
        LexLocation = lexLocation;
    }

    public abstract SymbolicNode Accept(IVisitor visitor);
}

public class ComplexNode : Node
{
    public Node?[] Children { get; }

    public ComplexNode(NodeTag nodeTag, LexLocation lexLocation, params Node?[] nodes) : base(nodeTag, lexLocation)
    {
        Children = nodes;
    }

    public override SymbolicNode Accept(IVisitor visitor)
    {
        // TODO - should call the correct method based on the tag
        switch (Tag)
        {
            case NodeTag.ProgramRoutineDeclaration or NodeTag.ProgramSimpleDeclaration:
                return visitor.ProgramVisit(this);
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
            case NodeTag.BodyStatement or NodeTag.BodySimpleDeclaration:
            case NodeTag.ArrayType or NodeTag.ArrayTypeWithoutSize:
            case NodeTag.ForLoop or NodeTag.ForeachLoop or NodeTag.WhileLoop: 
            case NodeTag.IfStatement or NodeTag.IfElseStatement:
            case NodeTag.Range or NodeTag.RangeReverse:
            case NodeTag.RecordType or NodeTag.VariableDeclarations:
            case NodeTag.Assignment:
            case NodeTag.Cast:
                return visitor.StatementVisit(this);
            case NodeTag.RoutineDeclaration or NodeTag.RoutineDeclarationWithParams:
            case NodeTag.RoutineDeclarationWithTypeAndParams or NodeTag.RoutineDeclarationWithType:
            case NodeTag.ParameterDeclaration:
            case NodeTag.ParametersContinuous:
            case NodeTag.RoutineCall:
            case NodeTag.ExpressionsContinuous:
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
            case NodeTag.ArrayConst or NodeTag.EmptyArrayConst:
                return visitor.ExpressionVisit(this);
            case NodeTag.Print:
                // TODO - implement print visitor
                throw new Exception("Not implemented");
            default:
                throw new Exception($"Unexpected NodeTag {Tag} in the visit function");
        }
    }
}

public class LeafNode<T> : Node
{
    public T Value { get; }

    public LeafNode(NodeTag nodeTag, LexLocation lexLocation, T value) : base(nodeTag, lexLocation)
    {
        Value = value;
    }

    public override SymbolicNode Accept(IVisitor visitor)
    {
        return visitor.VisitLeaf(this);
    }
}