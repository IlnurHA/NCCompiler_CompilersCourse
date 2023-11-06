namespace NCCompiler_CompilersCourse.Parser
{
    internal enum NodeTag
    {
        Program,
        ProgramSimpleDeclaration,
        ProgramRoutineDeclaration,
        SimpleVarDeclaration,
        SimpleTypeDeclaration,
        SimpleDeclaration,
        VariableDeclarationFull,
        VariableDeclarationIdenType,
        VariableDeclarationIdenExpr,
        VariableDeclaration,
        TypeDeclaration,
        RoutineDeclaration,
        RoutineDeclarationWithType,
        Parameters,
        ParametersContinuous,
        ParameterDeclaration,
        Type,
        PrimitiveType,
        RecordType,
        VariableDeclarations,
        ArrayType,
        Body,
        BodySimpleDeclaration,
        BodyStatement,
        Statement,
        Assignment,
        RoutineCall,
        Expressions,
        ExpressionsContinuous,
        WhileLoop,
        ForLoop,
        Range,
        RangeReverse,
        ForeachLoop,
        IfStatement,
        IfElseStatement,
        Expression,
        Relation,
        Simple,
        Factor,
        Summand,
        Primary,
        Sign,
        Cast,
        ModifiablePrimary,
        ModifiablePrimaryGettingSize,
        ModifiablePrimaryWithoutSize,
        ModifiablePrimaryGettingField,
        ModifiablePrimaryGettingValueFromArray,
        Assert,
        Identifier,
        IntegerLiteral,
        RealLiteral,
        BooleanLiteral,
        SignToInteger,
        SignToDouble,
        NotInteger,
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
        Not,
        True,
        False,
        Error,
        Unary,
        Return,
    }

    internal abstract class Node
    {
        public static ComplexNode MakeComplexNode(NodeTag nodeTag, params Node[] children)
        {
            return new ComplexNode(nodeTag, children);
        }

        readonly NodeTag _tag;
        public NodeTag Tag => _tag;

        protected Node(NodeTag tag)
        {
            _tag = tag;
        }
    }

    internal class ComplexNode : Node
    {
        private readonly Node[] _children;

        public ComplexNode(NodeTag nodeTag, params Node[] nodes) : base(nodeTag)
        {
            _children = nodes;
        }
    }

    internal class LeafNode<T> : Node
    {
        private readonly T _value;
        
        public LeafNode(NodeTag nodeTag, T value) : base(nodeTag)
        {
            _value = value;
        }
    }
}