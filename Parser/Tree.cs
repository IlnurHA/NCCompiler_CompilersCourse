﻿namespace NCCompiler_CompilersCourse.Parser
{
    internal enum NodeTag
    {
        Program,
        SimpleDeclaration,
        VariableDeclaration,
        TypeDeclaration,
        RoutineDeclaration,
        Parameters,
        ParameterDeclaration,
        Type,
        PrimitiveType,
        RecordType,
        VariableDeclarations,
        ArrayType,
        Body,
        Statement,
        Assignment,
        RoutineCall,
        Expressions,
        WhileLoop,
        ForLoop,
        Range,
        RangeReverse,
        ForeachLoop,
        IfStatement,
        Expression,
        Relation,
        Simple,
        Factor,
        Summand,
        Primary,
        Sign,
        Cast,
        ModifiablePrimary,
        ModifiablePrimaryWithoutSize,
        Assert,
        Identifier,
        IntegerLiteral,
        RealLiteral,
        BooleanLiteral,
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
    }

    internal abstract class Node
    {
        public static Node MakeBinary(NodeTag tag, Node lhs, Node rhs)
        {
            return new BinaryNode(tag, lhs, rhs);
        }

        public static Node MakeTernary(NodeTag tag, Node n1, Node n2, Node n3)
        {
            return new TernaryNode(tag, n1, n2, n3);
        }

        public static Node MakeUnary(NodeTag tag, Node child)
        {
            return new UnaryNode(tag, child);
        }
        
        public static Node MakeIdentifierLeaf(string identifier)
        {
            return new Leaf(identifier);
        }
        
        public static Node MakeIntLeaf(Int32 value)
        {
            return new Leaf(value);
        }

        public static Node MakeIntLeaf(string operation, Int32 value)
        {
            return operation == "+" ? new Leaf(value) : new Leaf(-value);
        }
        
        public static Node MakeDoubleLeaf(double value)
        {
            return new Leaf(value);
        }
        
        public static Node MakeDoubleLeaf(string operation, double value)
        {
            return operation == "+" ? new Leaf(value) : new Leaf(-value);
        }
        
        public static Node MakeBoolLeaf(Boolean value)
        {
            return new Leaf(value);
        }
        
        public static Node MakeUnaryOperationLeaf(String operation)
        {
            return new UnaryOperationLeaf(operation);
        }

        public static Node MakePrimitiveTypeLeaf(String primitiveType)
        {
            return new Leaf(primitiveType);
        }

        readonly NodeTag _tag;
        protected bool Active = false;
        public NodeTag Tag => _tag;

        protected Node(NodeTag tag)
        {
            this._tag = tag;
        }
        // public abstract Object Eval( Parser p );
        // public abstract string Unparse();

        public void Prolog()
        {
            // if (active)
            // throw new Parser.CircularEvalException();
            Active = true;
        }

        public void Epilog()
        {
            Active = false;
        }
    }

    internal class Leaf : Node
    {
        private readonly string _name;
        private readonly Object _value;

        internal Leaf(string name) : base(NodeTag.Identifier)
        {
            _name = name;
        }

        internal Leaf(Boolean value) : base(NodeTag.BooleanLiteral)
        {
            _value = value;
        }

        internal Leaf(Int32 value) : base(NodeTag.IntegerLiteral)
        {
            _value = value;
        }

        internal Leaf(Double value) : base(NodeTag.RealLiteral)
        {
            _value = value;
        }

        public string Index
        {
            get { return _name; }
        }
    }

    internal class UnaryOperationLeaf : Node
    {
        private readonly string _operation;

        internal UnaryOperationLeaf(string operation) : base(NodeTag.Unary)
        {
            _operation = operation;
        }
    }

    internal class PrimitiveTypeLeaf : Node
    {
        private readonly string _primitiveType;

        internal PrimitiveTypeLeaf(string primitiveType) : base(NodeTag.PrimitiveType)
        {
            _primitiveType = primitiveType;
        }
    }

    internal class TernaryNode : Node
    {
        private readonly Node _n1;
        private readonly Node _n2;
        private readonly Node _n3;

        public TernaryNode(NodeTag nodeTag, Node n1, Node n2, Node n3) : base(nodeTag)
        {
            _n1 = n1;
            _n2 = n2;
            _n3 = n3;
        }
    }

// MakeBinaryNode(NodeTag.modifiablePrimaryNode, )

    internal class UnaryNode : Node
    {
        Node _child;

        internal UnaryNode(NodeTag t, Node c)
            : base(t)
        {
            _child = c;
        }
    }

    internal class BinaryNode : Node
    {
        private readonly Node _lhs;
        private readonly Node _rhs;

        internal BinaryNode(NodeTag t, Node l, Node r) : base(t)
        {
            _lhs = l;
            _rhs = r;
        }
    }
}