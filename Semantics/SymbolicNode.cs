namespace NCCompiler_CompilersCourse.Semantics;

public abstract class SymbolicNode
{
}

public class TypedSymbolicNode : SymbolicNode
{
    public TypeNode Type { get; set; } = new TypeNode(MyType.Undefined);

    public TypedSymbolicNode()
    {
    }

    public TypedSymbolicNode(TypeNode typeNode)
    {
        Type = typeNode;
    }
}

public class TypeNode : SymbolicNode
{
    public new MyType MyType { get; set; }

    public TypeNode(MyType myType)
    {
        MyType = myType;
    }

    public TypeNode()
    {
        MyType = MyType.Undefined;
    }

    public bool IsTheSame(TypeNode anotherObject)
    {
        return MyType == anotherObject.MyType;
    }

    public TypeNode ConvertTo(TypeNode toTypeNode)
    {
        if (toTypeNode.IsTheSame(this)) return this;
        return (toTypeNode.MyType, MyType) switch
        {
            (MyType.Integer, MyType.Real) => toTypeNode,
            (MyType.Integer, MyType.Boolean) => toTypeNode,
            (MyType.Real, MyType.Integer) => toTypeNode,
            (MyType.Real, MyType.Boolean) => toTypeNode,
            (MyType.Boolean, MyType.Integer) => toTypeNode,
            _ => throw new Exception($"Can't convert from type {MyType} to {toTypeNode.MyType}")
        };
    }

    public bool IsConvertibleTo(TypeNode toTypeNode)
    {
        try
        {
            ConvertTo(toTypeNode);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public TypeNode GetFinalTypeNode()
    {
        if (this is not UserDefinedTypeNode userDefinedTypeNode) return this;
        return userDefinedTypeNode.GetFinalTypeNode();
    }
}

public class ArrayTypeNode : TypeNode
{
    public TypeNode ElementTypeNode { get; }
    public ValueNode? Size { get; set; }

    public ArrayTypeNode(TypeNode elementTypeNode, ValueNode size) : base(MyType.CompoundType)
    {
        ElementTypeNode = elementTypeNode;
        Size = size;
    }

    public ArrayTypeNode(TypeNode elementTypeNode) : base(MyType.CompoundType)
    {
        ElementTypeNode = elementTypeNode;
    }

    public new bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject is not ArrayTypeNode tempObj) return false;
        return MyType == tempObj.MyType && ElementTypeNode.IsTheSame(tempObj) && Size == tempObj.Size;
    }
}

public class StructTypeNode : TypeNode
{
    public Dictionary<string, TypeNode> StructFields { get; set; }

    public StructTypeNode(Dictionary<string, TypeNode> structFields) : base(MyType.CompoundType)
    {
        StructFields = structFields;
    }

    public new bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject is not StructTypeNode tempObj) return false;
        if (StructFields.Count != tempObj.StructFields.Count) return false;
        foreach (var field in StructFields)
        {
            if (!tempObj.StructFields.ContainsKey(field.Key)) return false;
            if (!field.Value.IsTheSame(tempObj.StructFields[field.Key])) return false;
        }

        return true;
    }
}

public class UserDefinedTypeNode : TypeNode
{
    public TypeNode Type { get; set; }
    public string Name { get; set; }
    public new MyType MyType { get; set; } = MyType.DeclaredType;

    public new bool IsTheSame(TypeNode anotherObject)
    {
        return Type.IsTheSame(anotherObject);
    }

    public new TypeNode GetFinalTypeNode()
    {
        switch (Type)
        {
            case ArrayTypeNode arrayTypeNode:
                return arrayTypeNode;
            case StructTypeNode structTypeNode:
                return structTypeNode;
            case UserDefinedTypeNode userDefinedTypeNode:
                return userDefinedTypeNode.GetFinalTypeNode();
            case { } simpleTypeNode:
                return simpleTypeNode;
        }

        throw new Exception("Got null type node");
    }
}

public class ValueNode : SymbolicNode
{
    public new Object? Value { get; set; }
    public TypeNode Type { get; set; }

    public ValueNode(TypeNode type, object? value = null)
    {
        Value = value;
        Type = type;
    }

    public ValueNode()
    {
        Value = null;
        Type = new TypeNode(MyType.Undefined);
    }

    public ValueNode GetFinalValueNode()
    {
        if (Type is not UserDefinedTypeNode userDefinedTypeNode) return this;
        var finalType = userDefinedTypeNode.GetFinalTypeNode();
        Type = finalType;
        return this;
    }
}

public class ConstNode : ValueNode
{
    public ConstNode(TypeNode typeNode, object value) : base(typeNode, value)
    {
    }
}

public class VarNode : ValueNode
{
    public new string? Name { get; set; } = null;
    public bool IsInitialized { get; set; } = false;

    public VarNode(string name)
    {
        Name = name;
    }

    public VarNode GetFinalVarNode()
    {
        if (Type is not UserDefinedTypeNode userDefinedTypeNode) return this;
        var finalType = userDefinedTypeNode.GetFinalTypeNode();
        Type = finalType;
        return this;
    }

    public VarNode()
    {
    }
}

public class PrimitiveVarNode : VarNode
{
    public new string Name { get; }

    public PrimitiveVarNode(string name)
    {
        Name = name;
    }
}

public class StatementNode : TypedSymbolicNode
{
}

public class BreakNode : StatementNode
{
}

public class AssertNode : StatementNode
{
    private ValueNode leftExpression;
    private ValueNode rightExpression;

    public AssertNode(ValueNode left, ValueNode right)
    {
        leftExpression = left;
        rightExpression = right;
    }
}

public class ReturnNode : StatementNode
{
}

public class DeclarationNode : StatementNode
{
    public VarNode Variable { get; set; }

    public static VarNode GetAppropriateVarNode(PrimitiveVarNode primitiveVarNode, TypeNode type, ValueNode? value)
    {
        switch (type)
        {
            case UserDefinedTypeNode userDefinedTypeNode:
                return GetAppropriateVarNode(primitiveVarNode, userDefinedTypeNode.GetFinalTypeNode(), value);
            case ArrayTypeNode arrayTypeNode:
                return new ArrayVarNode(primitiveVarNode.Name!, arrayTypeNode.ElementTypeNode, value);
            case StructTypeNode structTypeNode:
                var newNode = StructVarNode.FromType(structTypeNode);
                newNode.Name = primitiveVarNode.Name!;
                if (value is not null) newNode.Value = value;
                return newNode;
            case { } typeNode:
                return new VarNode(primitiveVarNode.Name!)
                    {Type = typeNode, Value = value, IsInitialized = value is not null};
        }

        throw new Exception($"Got unexpected type node {type.GetType()}");
    }
}

public class FullVariableDeclaration : DeclarationNode
{
    public FullVariableDeclaration(PrimitiveVarNode primitiveVarNode, TypeNode type, ValueNode value)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, type, value);
    }
}

public class TypeVariableDeclaration : DeclarationNode
{
    public TypeVariableDeclaration(PrimitiveVarNode primitiveVarNode, TypeNode type)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, type, null);
    }
}

public class ValueVariableDeclaration : DeclarationNode
{
    public ValueVariableDeclaration(PrimitiveVarNode primitiveVarNode, ValueNode value)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, value.Type, value);
    }
}

public class TypeDeclarationNode : StatementNode
{
    public UserDefinedTypeNode DeclaredType { get; }

    public TypeDeclarationNode(PrimitiveVarNode varNode, TypeNode type)
    {
        DeclaredType = new UserDefinedTypeNode {Name = varNode.Name, Type = type};
    }
}

public class AssignmentNode : StatementNode
{
    public ValueNode Variable { get; set; }

    public AssignmentNode(ValueNode varNode, ValueNode value)
    {
        Variable = varNode;
        Variable.Value = value;
    }
}

public class BodyNode : TypedSymbolicNode
{
    public List<StatementNode> Statements { get; }

    public BodyNode(List<StatementNode> statements, TypeNode typeNode)
    {
        Type = typeNode;
        Statements = statements;
    }
    
    public BodyNode()
    {
        Statements = new List<StatementNode>();
        Type = new TypeNode(MyType.Undefined);
    }

    public void AddStatement(StatementNode statement)
    {
        Statements.Add(statement);
    }
}

public class OperationNode : ValueNode
{
    public OperationType OperationType { get; set; }
    public List<ValueNode> Operands { get; set; } = new List<ValueNode>();

    public OperationNode(OperationType operationType)
    {
        OperationType = operationType;
    }

    public OperationNode(OperationType operationType, List<ValueNode> operands, TypeNode typeNode) : base(
        type: typeNode)
    {
        OperationType = operationType;
        Operands = operands;
    }
}

public class ArrayVarNode : VarNode
{
    public new ArrayTypeNode Type { get; }
    public List<ValueNode> Elements = new List<ValueNode>();

    public ArrayVarNode(string name, TypeNode elementTypeNode, ValueNode size) : base(name)
    {
        Type = new ArrayTypeNode(elementTypeNode, size);
        base.Type = Type;
    }

    public ArrayVarNode(string name, TypeNode elementTypeNode, List<ValueNode> values) : base(name)
    {
        Elements = values;
        Type = new ArrayTypeNode(elementTypeNode.GetFinalTypeNode(),
            new ValueNode(new TypeNode(MyType.Integer), values.Count));
        base.Type = Type;
    }

    // For function parameters with arbitrary number of elements
    public ArrayVarNode(string name, TypeNode elementTypeNode) : base(name)
    {
        Type = new ArrayTypeNode(elementTypeNode.GetFinalTypeNode());
        base.Type = Type;
    }

    public ArrayVarNode(ArrayTypeNode elementTypeNode)
    {
        Type = elementTypeNode;
        base.Type = Type;
    }
}

public class GetByIndexNode : ValueNode
{
    public ArrayVarNode ArrayVarNode { get; set; }
    public ValueNode Index { get; set; }

    public GetByIndexNode(ArrayVarNode varNode, ValueNode index) : base(varNode.Type.ElementTypeNode)
    {
        ArrayVarNode = varNode;
        Index = index;
    }
}

public class StructVarNode : VarNode
{
    public Dictionary<string, VarNode> Fields { get; set; }

    public StructVarNode(Dictionary<string, VarNode> fields, StructTypeNode structTypeNode)
    {
        Fields = fields;
        Type = structTypeNode;
    }

    public VarNode GetField(string fieldName)
    {
        if (!Fields.ContainsKey(fieldName))
            throw new Exception($"Trying to get undefined field {fieldName} from {Name} struct");
        return Fields[fieldName];
    }

    public static StructVarNode FromType(StructTypeNode structTypeNode)
    {
        var newDict = new Dictionary<string, VarNode>();

        foreach (var (key, value) in structTypeNode.StructFields)
        {
            newDict[key] = value switch
            {
                ArrayTypeNode arrayTypeNode => new ArrayVarNode(arrayTypeNode),
                StructTypeNode structTypeNode2 => FromType(structTypeNode2),
                { } node => new VarNode
                {
                    IsInitialized = false,
                    Type = node,
                }
            };
        }

        return new StructVarNode(newDict, structTypeNode);
    }
}

public class GetFieldNode : ValueNode
{
    public StructVarNode StructVarNode { get; set; }
    public string FieldName { get; set; }
    public VarNode? FieldNode { get; set; }

    public GetFieldNode(StructVarNode structVarNode, string fieldName) : base(structVarNode.GetField(fieldName).Type)
    {
        StructVarNode = structVarNode;
        FieldName = fieldName;
    }

    public GetFieldNode(StructVarNode structVarNode, PrimitiveVarNode fieldNode) : base(structVarNode.GetField(fieldNode.Name)
        .Type)
    {
        StructVarNode = structVarNode;
        FieldName = fieldNode.Name!;
        FieldNode = fieldNode;
    }
}

public class ArrayFunctions : ValueNode
{
    public ArrayVarNode Array { get; set; }

    public ArrayFunctions(ArrayVarNode arrayVarNode, TypeNode typeNode) : base(typeNode)
    {
        Array = arrayVarNode;
    }
}

public class SortedArrayNode : ArrayFunctions
{
    public SortedArrayNode(ArrayVarNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
}

public class ArraySizeNode : ArrayFunctions
{
    public ArraySizeNode(ArrayVarNode arrayVarNode) : base(arrayVarNode, new TypeNode(MyType.Integer))
    {
    }
}

public class ReversedArrayNode : ArrayFunctions
{
    public ReversedArrayNode(ArrayVarNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
}

public class ParameterNode : TypedSymbolicNode
{
    public VarNode Variable { get; set; }

    public ParameterNode(VarNode variable, TypeNode typeNode) : base(typeNode)
    {
        Variable = variable;
    }
}

public class ParametersNode : SymbolicNode
{
    public List<ParameterNode> Parameters { get; }

    public ParametersNode(List<ParameterNode> parameters)
    {
        Parameters = parameters;
    }

    public void AddParameter(ParameterNode parameterNode)
    {
        Parameters.Add(parameterNode);
    }
}

public class FunctionDeclNode : VarNode
{
    public PrimitiveVarNode FunctionName { get; }
    public BodyNode Body { get; }
    public ParametersNode? Parameters { get; }
    public TypeNode? ReturnType { get; }

    // For full declaration of function
    public FunctionDeclNode(PrimitiveVarNode functionName, ParametersNode? parameters, TypeNode? returnType, BodyNode body)
    {
        FunctionName = functionName;
        Body = body;
        Parameters = parameters;
        ReturnType = returnType;
        Name = FunctionName.Name;
    }
}

public class ExpressionsNode : TypedSymbolicNode
{
    public List<ValueNode> Expressions { get; }

    public ExpressionsNode(List<ValueNode> expressions) : base(expressions[0].Type)
    {
        Expressions = expressions;
    }

    public ExpressionsNode()
    {
        Expressions = new List<ValueNode>();
    }

    public void AddExpression(ValueNode expressionNode)
    {
        Expressions.Add(expressionNode);
    }
}

public class RoutineCallNode : ValueNode
{
    public FunctionDeclNode Function { get; set; }
    public ExpressionsNode? Expressions { get; set; }

    public RoutineCallNode(FunctionDeclNode function, ExpressionsNode expressions) : base(
        function.ReturnType ?? new TypeNode(MyType.Undefined))
    {
        Function = function;
        Expressions = expressions;
    }
}

public class RangeNode : SymbolicNode
{
    public ValueNode LeftBound { get; }
    public ValueNode RightBound { get; }

    public bool Reversed { get; }

    public RangeNode(ValueNode leftBound, ValueNode rightBound, bool reversed = false)
    {
        LeftBound = leftBound;
        RightBound = rightBound;
        Reversed = true;
    }
}

public class ForLoopNode : StatementNode
{
    public VarNode IdName { get; }
    public RangeNode Range { get; }
    public BodyNode Body { get; }

    public ForLoopNode(VarNode idName, RangeNode range, BodyNode body)
    {
        IdName = idName;
        Range = range;
        Body = body;
    }
}

public class ForEachLoopNode : StatementNode
{
    public VarNode IdName { get; }
    public ArrayVarNode Array { get; }
    public BodyNode Body { get; }

    public ForEachLoopNode(VarNode idName, ArrayVarNode array, BodyNode body)
    {
        IdName = idName;
        Array = array;
        Body = body;
    }
}

public class WhileLoopNode : StatementNode
{
    public ValueNode Condition { get; }
    public BodyNode Body { get; }

    public WhileLoopNode(ValueNode condition, BodyNode body)
    {
        Condition = condition;
        Body = body;
    }
}

public class IfStatement : StatementNode
{
    public ValueNode Condition { get; }
    public BodyNode Body { get; }

    public IfStatement(ValueNode condition, BodyNode body)
    {
        Condition = condition;
        Body = body;
    }
}

public class IfElseStatement : IfStatement
{
    public BodyNode BodyElse { get; }

    public IfElseStatement(ValueNode condition, BodyNode body, BodyNode bodyElse) : base(condition, body)
    {
        BodyElse = bodyElse;
    }
}

public class ArrayConst : ValueNode
{
    public ExpressionsNode Expressions { get; }

    public ArrayConst(ExpressionsNode expressions) : base(expressions.Type)
    {
        Expressions = expressions;
    }
}

public class ProgramNode : SymbolicNode
{
    public List<SymbolicNode> Declarations { get; }

    public ProgramNode()
    {
        Declarations = new List<SymbolicNode>();
    }

    public void AddDeclaration(SymbolicNode node)
    {
        Declarations.Add(node);
    }
}

public class VariableDeclarations : SymbolicNode
{
    public Dictionary<string, VarNode> Declarations { get; }

    public VariableDeclarations(Dictionary<string, VarNode> declarations)
    {
        Declarations = declarations;
    }

    public void AddDeclaration(VarNode varNode)
    {
        if (Declarations.ContainsKey(varNode.Name!))
        {
            throw new Exception("Repeated fields");
        }

        Declarations[varNode.Name!] = varNode;
    }
}

public class EmptyReturnNode : ReturnNode
{
    public EmptyReturnNode()
    {
        Type = new TypeNode(MyType.Undefined);
    }
}

public class ValueReturnNode : ReturnNode
{
    public ValueNode Value { get; }

    public ValueReturnNode(ValueNode value)
    {
        Value = value;
        Type = value.Type;
    }
}