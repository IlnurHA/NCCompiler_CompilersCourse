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
        ModifiablePrimaryGettingSize,
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
        Unary,
        Return,
        RoutineDeclarationWithTypeAndParams,
        RoutineDeclarationWithParams
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

        public abstract void Accept(IVisitor visitor);
    }

    internal class ComplexNode : Node
    {
        public Node?[] Children { get; }

        public ComplexNode(NodeTag nodeTag, params Node?[] nodes) : base(nodeTag)
        {
            Children = nodes;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    internal class LeafNode<T> : Node
    {
        public T Value { get; }

        public LeafNode(NodeTag nodeTag, T value) : base(nodeTag)
        {
            Value = value;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitLeaf(this);
        }
    }
}