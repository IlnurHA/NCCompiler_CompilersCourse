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

		public static Node MakeQuaternary(NodeTag tag, Node n1, Node n2, Node n3, Node n4)
        {
            return new QuaternaryNode(tag, n1, n2, n3, n4);
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

        public virtual string Unparse()
        {
            return "\n\t";
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

		public string Unparse() {
			switch (base._tag) {
				case NodeTag.VariableDeclarationFull:
					return $"VariableDeclarationFull{base.Unparse()}({_n1.Unparse()})({_n2.Unparse()})({_n3.Unparse()})";
				case NodeTag.RoutineDeclaration:
					return $"RoutineDeclaration{base.Unparse()}({_n1.Unparse()})({_n2.Unparse()})({_n3.Unparse()})";
				case NodeTag.ForLoop:
					return $"ForLoop{base.Unparse()}({_n1.Unparse()})({_n2.Unparse()})({_n3.Unparse()})";
				case NodeTag.ForeachLoop:
					return $"ForeachLoop{base.Unparse()}({_n1.Unparse()})({_n2.Unparse()})({_n3.Unparse()})";
				case NodeTag.IfElseStatement:
					return $"IfElseStatement{base.Unparse()}({_n1.Unparse()})({_n2.Unparse()})({_n3.Unparse()})";
				case _:
					throw new Exception("Not implemented");
			}
		}
    }

	internal class QuaternaryNode : Node
    {
        private readonly Node _n1;
        private readonly Node _n2;
        private readonly Node _n3;
        private readonly Node _n4;

        public QuaternaryNode(NodeTag nodeTag, Node n1, Node n2, Node n3, Node n4) : base(nodeTag)
        {
            _n1 = n1;
            _n2 = n2;
            _n3 = n3;
			_n4 = n4;
        }
		public string Unparse() {
			switch (base._tag) {
				case NodeTag.RoutineDeclarationWithType:
					return $"RoutineDeclarationWithType{base.Unparse()}({_n1.Unparse()})({_n2.Unparse()})({_n3.Unparse()})({_n4.Unparse()})";
				case _:
					throw new Exception("Not implemented");
			}
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
		public string Unparse() {
			string op;
			switch (base._tag) {
				case NodeTag.RecordType:
					op = "RecordType";
					break;
				case NodeTag.NotInteger:
					op = "NotInteger";
					break;
				case NodeTag.ArrayConst:
					op = "ArrayConst";
					break;
				case _:
					throw new Exception("Not implemented");
			}
			return $"{op}{base.Unparse()}{_child.Unparse()}";
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
        
        public override string Unparse() {
            string op = "";
            switch (Tag) {
                case NodeTag.Lt: op = "<"; break;
                case NodeTag.Le: op = "<="; break;
                case NodeTag.Gt: op = ">"; break;
                case NodeTag.Ge: op = ">="; break;
                case NodeTag.Ne: op = "/="; break;
                case NodeTag.Eq: op = "="; break;
                case NodeTag.And:   op = "and"; break;
                case NodeTag.Or: op = "or"; break;
                case NodeTag.Xor:  op = "xor"; break;
                case NodeTag.Div:   op = "/"; break;
                case NodeTag.Minus: op = "-"; break;
                case NodeTag.Plus:  op = "+"; break;
                case NodeTag.Rem:   op = "%"; break;
                case NodeTag.Mul:   op = "*"; break;
                case NodeTag.Cast:   op = "cast"; break;
                case NodeTag.ModifiablePrimaryGettingSize:   op = "get-size"; break;
                case NodeTag.ModifiablePrimaryGettingField:   op = "get-field"; break;
                case NodeTag.Assert:   op = "assert"; break;
                case NodeTag.ModifiablePrimaryGettingValueFromArray:   op = "[i]"; break;
                case NodeTag.SignToDouble:   op = "apply-sign"; break;
                case NodeTag.SignToInteger:   op = "apply-sign"; break;
                case NodeTag.NotInteger:   op = "not"; break;
                case NodeTag.IfStatement:   op = "if"; break;
                case NodeTag.WhileLoop:   op = "while"; break;
                case NodeTag.Range:   op = "range"; break;
                case NodeTag.RangeReverse:   op = "reverse-range"; break;
                case NodeTag.ExpressionsContinuous:   op = "expressions"; break;
                case NodeTag.RoutineCall:   op = "call"; break;
                case NodeTag.Assignment:   op = "assign"; break;
                case NodeTag.BodySimpleDeclaration:   op = "body"; break;
                case NodeTag.BodyStatement:   op = "body"; break;
                case NodeTag.ArrayType:   op = "array"; break;
                case NodeTag.ParameterDeclaration:   op = "param"; break;
                case NodeTag.ParametersContinuous:   op = "params"; break;
                case NodeTag.TypeDeclaration:   op = "type-decl"; break;
                case NodeTag.VariableDeclarationIdenExpr:   op = "ident-expr"; break;
                case NodeTag.VariableDeclarationIdenType:   op = "ident-type"; break;
                case NodeTag.ProgramRoutineDeclaration:   op = "program-routine"; break;
                case NodeTag.ProgramSimpleDeclaration:   op = "program-simple"; break;
            }
            return $"{op}{base.Unparse()}({_lhs.Unparse()})({_rhs.Unparse()})";
        }
    }
}