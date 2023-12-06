using System.Diagnostics;
using NCCompiler_CompilersCourse.CodeGeneration;

namespace NCCompiler_CompilersCourse.Semantics;

public abstract class SymbolicNode
{
    public abstract void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue);
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

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }
}

public class TypeNode : SymbolicNode
{
    public virtual MyType MyType { get; set; }

    public TypeNode(MyType myType)
    {
        MyType = myType;
    }

    public TypeNode()
    {
        MyType = MyType.Undefined;
    }

    public virtual bool IsTheSame(TypeNode anotherObject)
    {
        return MyType == anotherObject.MyType;
    }

    public TypeNode ConvertTo(TypeNode toTypeNode)
    {
        if (this is UserDefinedTypeNode definedTypeNode) return definedTypeNode.Type.ConvertTo(toTypeNode);
        if (toTypeNode is UserDefinedTypeNode userDefinedTypeNode) return ConvertTo(userDefinedTypeNode.Type);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitTypeNode(this, queue);
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

    public override bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject is not ArrayTypeNode tempObj) return false;
        return MyType == tempObj.MyType && ElementTypeNode.IsTheSame(tempObj.ElementTypeNode) &&
               (Size == null || tempObj.Size == null ||
                (Size is not null && tempObj.Size is not null &&
                 (Size.Value is null && tempObj.Size.Value is null || 
                  (Size.Value is not null && Size.Value.Equals(tempObj.Size.Value)))));
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitArrayTypeNode(this, commands);
    }
}

public class StructTypeNode : TypeNode
{
    public Dictionary<string, TypeNode> StructFields { get; set; }
    public Dictionary<string, VarNode> DefaultValues { get; set; } = new Dictionary<string, VarNode>();

    public StructTypeNode(Dictionary<string, TypeNode> structFields) : base(MyType.CompoundType)
    {
        StructFields = structFields;
    }

    public override bool IsTheSame(TypeNode anotherObject)
    {
        if (anotherObject is not StructTypeNode tempObj) return false;
        if (StructFields.Count != tempObj.StructFields.Count) return false;
        foreach (var field in StructFields)
        {
            if (!tempObj.StructFields.ContainsKey(field.Key)) return false;
            if (!field.Value.IsTheSame(tempObj.StructFields[field.Key])) return false;
        }
        
        // foreach (var (key, varNode) in DefaultValues)
        // {
        //     if (!tempObj.DefaultValues.ContainsKey(key)) return false;
        //     if (varNode.Value != tempObj.DefaultValues[key].Value) return false;
        // }

        return true;
    }
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitStructTypeNode(this, commands);
    }
}

public class UserDefinedTypeNode : TypeNode
{
    public TypeNode Type { get; set; }
    public string Name { get; set; }
    public override MyType MyType { get; set; } = MyType.DeclaredType;

    public override bool IsTheSame(TypeNode anotherObject)
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        throw new UnreachableException();
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

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }

    public virtual bool IsTheSameValue(object anotherObject)
    {
        throw new UnreachableException();
    }
}

public class ConstNode : ValueNode
{
    public ConstNode(TypeNode typeNode, object value) : base(typeNode, value)
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitConstNode(this, commands);
    }

    public override bool IsTheSameValue(object anotherObject)
    {
        if (anotherObject is not ConstNode constNode) return false;
        return constNode.Value == Value;
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

    // public VarNode GetFinalVarNode()
    // {
    //     if (Type is not UserDefinedTypeNode userDefinedTypeNode) return this;
    //     var finalType = userDefinedTypeNode.GetFinalTypeNode();
    //     Type = finalType;
    //     return this;
    // }

    public VarNode()
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitVarNode(this, queue);
    }
    
    public void AcceptStructField(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitStructFieldNode(this, queue);
    }
}

public class PrimitiveVarNode : VarNode
{
    public new string Name { get; }

    public PrimitiveVarNode(string name)
    {
        Name = name;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitPrimitiveVarNode(this, queue);
    }
}

public class StatementNode : TypedSymbolicNode
{
}

public class BreakNode : StatementNode
{
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> commands)
    {
        visitor.VisitBreakNode(this, commands);
    }
}

public class AssertNode : StatementNode
{
    public ValueNode LeftExpression { get; }
    public ValueNode RightExpression { get; }

    public AssertNode(ValueNode left, ValueNode right)
    {
        LeftExpression = left;
        RightExpression = right;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitAssertNode(this, queue);
    }
}

public class ReturnNode : StatementNode
{
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }
}

public class DeclarationNode : StatementNode
{
    public VarNode Variable { get; set; }
    public ValueNode DeclarationValue { get; set; }

    public static VarNode GetAppropriateVarNode(PrimitiveVarNode primitiveVarNode, TypeNode type, ValueNode? value)
    {
        switch (type)
        {
            case UserDefinedTypeNode userDefinedTypeNode:
                return GetAppropriateVarNode(primitiveVarNode, userDefinedTypeNode.GetFinalTypeNode(), value);
            case ArrayTypeNode arrayTypeNode:
                return new ArrayVarNode(arrayTypeNode)
                {
                    Value = value,
                    Name = primitiveVarNode.Name,
                };
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }
}

public class FullVariableDeclaration : DeclarationNode
{
    public FullVariableDeclaration(PrimitiveVarNode primitiveVarNode, TypeNode type, ValueNode value)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, type, value);
        DeclarationValue = value;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitFullVariableDeclaration(this, queue);
    }
}

public class TypeVariableDeclaration : DeclarationNode
{
    public TypeVariableDeclaration(PrimitiveVarNode primitiveVarNode, TypeNode type)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, type, null);
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitTypeVariableDeclaration(this, queue);
    }
}

public class ValueVariableDeclaration : DeclarationNode
{
    public ValueVariableDeclaration(PrimitiveVarNode primitiveVarNode, ValueNode value)
    {
        Variable = GetAppropriateVarNode(primitiveVarNode, value.Type, value);
        DeclarationValue = value;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitValueVariableDeclaration(this, queue);
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
    public ValueNode AssignmentValue { get; }

    public AssignmentNode(ValueNode varNode, ValueNode value)
    {
        Variable = varNode;
        Variable.Value = value;
        AssignmentValue = value;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitAssignmentNode(this, queue);
    }
}

public class BodyNode : TypedSymbolicNode
{
    public List<StatementNode> Statements { get; set; }

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

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitBodyNode(this, queue);
    }

    public void Filter(HashSet<string> unusedVariables)
    {
        List<StatementNode> usedBodyStatements = new();
        foreach (var statement in Statements)
        {
            if (statement is DeclarationNode || statement.GetType().IsSubclassOf(typeof(DeclarationNode)))
            {
                if (!unusedVariables.Contains(((DeclarationNode) statement).Variable.Name!))
                    usedBodyStatements.Add(statement);
            }
            else
                usedBodyStatements.Add(statement);
            if (statement is ReturnNode) break;
        }

        Statements = usedBodyStatements;
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitOperationNode(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayVarNode(this, queue);
    }
    
    public void AcceptByValue(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayVarByValueNode(this, queue);
    }
}

public class GetByIndexNode : ValueNode
{
    public ValueNode ArrayVarNode { get; set; }
    public ValueNode Index { get; set; }
    
    public TypeNode GetTypeNode()
    {
        if (ArrayVarNode is ArrayVarNode arrayVarNode) return arrayVarNode.Type.ElementTypeNode;
        if (ArrayVarNode is GetByIndexNode getByIndexNode)
            return ((ArrayTypeNode) getByIndexNode.GetTypeNode()).ElementTypeNode;
        if (ArrayVarNode is GetFieldNode getFieldNode)
            return ((ArrayTypeNode) getFieldNode.GetTypeNode()).ElementTypeNode;
        throw new Exception($"Unexpected type: {ArrayVarNode.GetType()}");
    }

    public GetByIndexNode(ValueNode varNode, ValueNode index)
    {
        ArrayVarNode = varNode;
        Index = index;
        Type = GetTypeNode();
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitGetByIndexNode(this, queue);
    }

    public void AcceptSetByIndex(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitSetByIndex(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitStructVarNode(this, queue);
    }
    
    public void AcceptByValue(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitStructVarByValueNode(this, queue);
    }
}

public class GetFieldNode : VarNode
{
    public ValueNode StructVarNode { get; set; }
    public string FieldName { get; set; }
    public VarNode? FieldNode { get; set; }

    public TypeNode GetTypeNode()
    {
        if (StructVarNode is StructVarNode structVarNode) return structVarNode.GetField(FieldName).Type.GetFinalTypeNode();
        if (StructVarNode is GetFieldNode getFieldNode)
            return ((StructTypeNode) getFieldNode.GetTypeNode().GetFinalTypeNode()).StructFields[FieldName];
        if (StructVarNode is GetByIndexNode getByIndexNode)
            return ((StructTypeNode) getByIndexNode.GetTypeNode().GetFinalTypeNode()).StructFields[FieldName];
        throw new Exception($"Unexpected type: {StructVarNode.GetType()}");
    }

    public GetFieldNode(StructVarNode structVarNode, string fieldName) : base(structVarNode.Name!)
    {
        StructVarNode = structVarNode;
        FieldName = fieldName;
        Type = structVarNode.GetField(fieldName).Type;
    }

    public GetFieldNode(ValueNode structVarNode, PrimitiveVarNode fieldNode)
    {
        StructVarNode = structVarNode;
        FieldName = fieldNode.Name;
        FieldNode = fieldNode;
        Type = GetTypeNode();
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitGetFieldNode(this, queue);
    }

    public void AcceptSetField(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitSetFieldNode(this, queue);
    }
}

public class ArrayFunctions : ValueNode
{
    public ValueNode Array { get; set; }

    public ArrayFunctions(ValueNode arrayVarNode, TypeNode typeNode) : base(typeNode)
    {
        Array = arrayVarNode;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayFunctions(this, queue);
    }
}

public class SortedArrayNode : ArrayFunctions
{
    public SortedArrayNode(ValueNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitSortedArrayNode(this, queue);
    }
}

public class ArraySizeNode : ArrayFunctions
{
    public ArraySizeNode(ValueNode arrayVarNode) : base(arrayVarNode, new TypeNode(MyType.Integer))
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArraySizeNode(this, queue);
    }
}

public class ReversedArrayNode : ArrayFunctions
{
    public ReversedArrayNode(ValueNode arrayVarNode) : base(arrayVarNode, arrayVarNode.Type)
    {
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitReversedArrayNode(this, queue);
    }
}

public class ParameterNode : TypedSymbolicNode
{
    public VarNode Variable { get; set; }

    public ParameterNode(VarNode variable, TypeNode typeNode) : base(typeNode)
    {
        Variable = variable;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        throw new Exception($"Trying to visit {GetType().Name} node with no accept implemented!");
    }
}

public class ParametersNode : SymbolicNode
{
    public List<ParameterNode> Parameters { get; set; }

    public ParametersNode(List<ParameterNode> parameters)
    {
        Parameters = parameters;
    }

    public void AddParameter(ParameterNode parameterNode)
    {
        Parameters.Add(parameterNode);
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitParametersNode(this, queue);
    }

    public void Filter(HashSet<string> unusedVariables)
    {
        Parameters = Parameters.Where(parameter => !unusedVariables.Contains(parameter.Variable.Name!)).ToList();
    }
}

public class RoutineDeclarationNode : VarNode
{
    public PrimitiveVarNode FunctionName { get; }
    public BodyNode Body { get; }
    public ParametersNode? Parameters { get; }
    public TypeNode? ReturnType { get; }

    // For full declaration of function
    public RoutineDeclarationNode(PrimitiveVarNode functionName, ParametersNode? parameters, TypeNode? returnType, BodyNode body)
    {
        FunctionName = functionName;
        Body = body;
        Parameters = parameters;
        ReturnType = returnType;
        Name = FunctionName.Name;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitRoutineDeclarationNode(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitExpressionsNode(this, queue);
    }
}

public class RoutineCallNode : ValueNode
{
    public RoutineDeclarationNode Routine { get; set; }
    public ExpressionsNode? Expressions { get; set; }

    public RoutineCallNode(RoutineDeclarationNode routine, ExpressionsNode expressions) : base(
        routine.ReturnType ?? new TypeNode(MyType.Undefined))
    {
        Routine = routine;
        Expressions = expressions;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitRoutineCallNode(this, queue);
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
        Reversed = reversed;
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitRangeNode(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitForLoopNode(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitForEachLoopNode(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitWhileLoopNode(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitIfStatement(this, queue);
    }
}

public class IfElseStatement : IfStatement
{
    public BodyNode BodyElse { get; }

    public IfElseStatement(ValueNode condition, BodyNode body, BodyNode bodyElse) : base(condition, body)
    {
        BodyElse = bodyElse;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitIfElseStatement(this, queue);
    }
}

public class ArrayConst : ValueNode
{
    public ExpressionsNode Expressions { get; }

    public ArrayConst(ExpressionsNode expressions) : base(expressions.Type)
    {
        Expressions = expressions;
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitArrayConst(this, queue);
    }
}

public class ProgramNode : SymbolicNode
{
    public List<SymbolicNode> Declarations { get; } = new();
    private bool _hasMainFlag;

    public void AddDeclaration(SymbolicNode node)
    {
        Declarations.Add(node);
    }

    public bool HasMain()
    {
        return _hasMainFlag;
    }
    
    public void SetHasMain()
    {
        _hasMainFlag = true;
    }

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitProgramNode(this, queue);
    }
}

public class VariableDeclarations : SymbolicNode
{
    public Dictionary<string, VarNode> Declarations { get; }
    public List<DeclarationNode> DeclarationNodes { get; set; } = new List<DeclarationNode>();

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

    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitVariableDeclarations(this, queue);
    }
}

public class EmptyReturnNode : ReturnNode
{
    public EmptyReturnNode()
    {
        Type = new TypeNode(MyType.Undefined);
    }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitEmptyReturnNode(this, queue);
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
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitValueReturnNode(this, queue);
    }
}

public class CastNode : ValueNode
{
    public CastNode(TypeNode type, ValueNode value) : base(type, value) { }
    
    public override void Accept(IVisitorCodeGeneration visitor, Queue<BaseCommand> queue)
    {
        visitor.VisitCastNode(this, queue);
    }
}