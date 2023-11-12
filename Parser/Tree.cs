using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.Parser
{
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
            return visitor.Visit(this);
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