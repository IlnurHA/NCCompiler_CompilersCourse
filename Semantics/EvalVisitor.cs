using System.Linq.Expressions;
using System.Numerics;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    public ScopeStack ScopeStack { get; set; } = new();

    public SymbolicNode ModifiablePrimaryVisit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.ModifiablePrimaryGettingField:
                var modPrimField = node.Children[0]!.Accept(this);
                var idField = node.Children[1]!.Accept(this);
                return new GetFieldNode((StructVarNode) modPrimField, (VarNode) idField); // return VarNode
            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                var arrFromArr = node.Children[0]!.Accept(this);
                var indexFromArr = node.Children[1]!.Accept(this);
                return new GetByIndexNode((ArrayVarNode) arrFromArr, (ValueNode) indexFromArr); // return VarNode
            case NodeTag.ArrayGetSorted:
                var arrGetSorted = node.Children[0]!.Accept(this);
                if (arrGetSorted.GetType() != typeof(ArrayVarNode))
                    throw new Exception($"Should have got 'ArrayVarNode', got '{arrGetSorted}' instead");
                return new SortedArrayNode((ArrayVarNode) arrGetSorted); // TODO return VarNode
            case NodeTag.ArrayGetSize:
                var arrGetSize = node.Children[0]!.Accept(this);
                return new ArraySizeNode((ArrayVarNode) arrGetSize); // TODO return VarNode
            case NodeTag.ArrayGetReversed:
                var arrGetReversed = node.Children[0]!.Accept(this);
                return new ReversedArrayNode((ArrayVarNode) arrGetReversed); // TODO return VarNode
        }

        throw new Exception("Unimplemented");
    }

    public SymbolicNode StatementVisit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.VariableDeclarationFull:
            case NodeTag.VariableDeclarationIdenType:
            case NodeTag.VariableDeclarationIdenExpr:
                VarNode variableIdentifier = (VarNode) node.Children[0]!.Accept(this);
                TypeNode? variableType = null;
                ValueNode? value = null;
                switch (node.Tag)
                {
                    case NodeTag.VariableDeclarationFull:
                        variableType = (TypeNode) node.Children[1]!.Accept(this);
                        value = (ValueNode) node.Children[2]!.Accept(this);
                        break;
                    case NodeTag.VariableDeclarationIdenType:
                        variableType = (TypeNode) node.Children[1]!.Accept(this);
                        break;
                    case NodeTag.VariableDeclarationIdenExpr:
                        value = (ValueNode) node.Children[1]!.Accept(this);
                        break;
                }

                using (var scope = ScopeStack.GetLastScope())
                {
                    if (variableIdentifier.Name == null || !scope.IsFree(variableIdentifier.Name))
                        throw new Exception(
                            $"The variable with name {variableIdentifier.Name} already exists in this scope!");
                    if (value != null)
                    {
                        variableIdentifier = variableIdentifier.GetFinalVarNode();
                        value = value.GetFinalValueNode();
                        if (variableType != null && value.Type.IsConvertibleTo(variableType))
                            throw new Exception($"Unexpected type of value for variable. Given type: {value.Type}");
                        variableIdentifier.Value = value;
                        variableIdentifier.IsInitialized = true;
                    }

                    scope.AddVariable(variableIdentifier);
                }

                return new DeclarationNode(variableIdentifier, value);
            case NodeTag.TypeDeclaration:
                VarNode typeIdentifier = (VarNode) node.Children[0]!.Accept(this);
                TypeNode typeSynonym = ((TypeNode) node.Children[1]!.Accept(this)).GetFinalTypeNode();
                typeIdentifier.Value = typeSynonym;
                using (var scope = ScopeStack.GetLastScope())
                {
                    if (typeIdentifier.Name == null || !scope.IsFree(typeIdentifier.Name))
                        throw new Exception(
                            $"The user type with name {typeIdentifier.Name} already exists in this scope!");
                    scope.AddVariable(typeIdentifier);
                }

                return new TypeDeclarationNode(typeIdentifier, typeSynonym);

            case NodeTag.Break:
                if (!ScopeStack.HasLoopScope()) throw new Exception("Unexpected context for 'break' statement");
                return new BreakNode();
            case NodeTag.Assert:
                ValueNode leftAssertExpression = (ValueNode) node.Children[0]!.Accept(this);
                ValueNode rightAssertExpression = (ValueNode) node.Children[1]!.Accept(this);

                if (!leftAssertExpression.Type.IsTheSame(rightAssertExpression.Type))
                {
                    _isValidOperation(leftAssertExpression, rightAssertExpression, operationType: OperationType.Assert);
                }

                return new AssertNode(leftAssertExpression, rightAssertExpression);
            case NodeTag.Return:
                if (node.Children.Length == 0) return new ReturnNode();
                ValueNode returnValue = (ValueNode) node.Children[0]!.Accept(this);
                return new ReturnNode(returnValue);
            case NodeTag.Range or NodeTag.RangeReverse:
                var leftBound = (ValueNode) node.Children[0]!.Accept(this);
                var rightBound = (ValueNode) node.Children[1]!.Accept(this);

                var integerType = new TypeNode(MyType.Integer);
                if (!leftBound.Type.IsConvertibleTo(integerType))
                {
                    throw new Exception($"Cannot convert {leftBound.Type.MyType} to integer type");
                }

                if (!rightBound.Type.IsConvertibleTo(integerType))
                {
                    throw new Exception($"Cannot convert {rightBound.Type.MyType} to integer type");
                }

                return new RangeNode(leftBound, rightBound, node.Tag == NodeTag.RangeReverse);

            case NodeTag.ForLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var idForLoop = (VarNode) node.Children[0]!.Accept(this);
                idForLoop.Type = new TypeNode(MyType.Integer);
                ScopeStack.AddVariable(idForLoop);

                var rangeForLoop = (RangeNode) node.Children[1]!.Accept(this);
                var bodyForLoop = (BodyNode) node.Children[2]!.Accept(this);

                ScopeStack.DeleteScope();
                return new ForLoopNode(idForLoop, rangeForLoop, bodyForLoop)
                {
                    Type = bodyForLoop.Type
                };

            case NodeTag.ForeachLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var idForEach = (VarNode) node.Children[0]!.Accept(this);
                var fromForEach = (ValueNode) node.Children[1]!.Accept(this);
                if (fromForEach is not ArrayVarNode arrayVarNode)
                    throw new Exception($"Unexpected type. Got {fromForEach.GetType()}, expected array type");
                idForEach.Type = ((ArrayTypeNode) arrayVarNode.Type).ElementTypeNode;
                ScopeStack.AddVariable(idForEach);

                var bodyForEach = (BodyNode) node.Children[2]!.Accept(this);
                ScopeStack.DeleteScope();
                return new ForEachLoopNode(idForEach, arrayVarNode, bodyForEach)
                {
                    Type = bodyForEach.Type
                };
            case NodeTag.WhileLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var condExprWhile = (ValueNode) node.Children[0]!.Accept(this);
                if (!condExprWhile.Type.IsConvertibleTo(new TypeNode(MyType.Boolean)))
                {
                    throw new Exception(
                        $"Unexpected type for while loop condition: Got {condExprWhile.Type.MyType}, expected boolean");
                }

                var bodyWhile = (BodyNode) node.Children[1]!.Accept(this);
                return new WhileLoopNode(condExprWhile, bodyWhile)
                {
                    Type = bodyWhile.Type
                };
            case NodeTag.IfStatement:
                ScopeStack.NewScope(Scope.ScopeContext.IfStatement);
                var condIf = (ValueNode) node.Children[0]!.Accept(this);
                if (!condIf.Type.IsConvertibleTo(new TypeNode(MyType.Boolean)))
                {
                    throw new Exception(
                        $"Unexpected type for if statement condition: Got {condIf.Type.MyType}, expected boolean");
                }

                var bodyIf = (BodyNode) node.Children[1]!.Accept(this);
                return new IfStatement(condIf, bodyIf)
                {
                    Type = bodyIf.Type
                };
            case NodeTag.IfElseStatement:
                ScopeStack.NewScope(Scope.ScopeContext.IfStatement);
                var condIfElse = (ValueNode) node.Children[0]!.Accept(this);
                if (!condIfElse.Type.IsConvertibleTo(new TypeNode(MyType.Boolean)))
                {
                    throw new Exception(
                        $"Unexpected type for if statement condition: Got {condIfElse.Type.MyType}, expected boolean");
                }

                var bodyIfElse = (BodyNode) node.Children[1]!.Accept(this);
                var bodyElse = (BodyNode) node.Children[2]!.Accept(this);

                var newType = bodyIfElse.Type;

                if (!bodyIfElse.Type.IsTheSame(bodyElse.Type))
                {
                    newType = _isValidOperation(new ValueNode(bodyElse.Type), new ValueNode(bodyIfElse.Type),
                        OperationType.Assert);
                    
                }

                return new IfElseStatement(condIfElse, bodyIfElse, bodyElse)
                {
                    Type = newType
                };
            
        }
    }

    public SymbolicNode RoutineVisit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.RoutineDeclarationWithTypeAndParams or NodeTag.RoutineDeclarationWithType
                or NodeTag.RoutineDeclaration or NodeTag.RoutineDeclarationWithParams:
                var funcNameRoutineDecl = (VarNode) node.Children[0]!.Accept(this);
                ParametersNode? parametersRoutineDecl = null;
                TypeNode? returnTypeRoutineDecl = null;
                BodyNode? bodyRoutineDeclFull;
                ScopeStack.NewScope(Scope.ScopeContext.Routine);
                switch (node.Tag)
                {
                    case NodeTag.RoutineDeclarationWithTypeAndParams:
                        parametersRoutineDecl = (ParametersNode) node.Children[1]!.Accept(this);
                        returnTypeRoutineDecl = (TypeNode) node.Children[2]!.Accept(this);
                        bodyRoutineDeclFull = (BodyNode) node.Children[3]!.Accept(this);
                        break;
                    case NodeTag.RoutineDeclarationWithParams:
                        parametersRoutineDecl = (ParametersNode) node.Children[1]!.Accept(this);
                        bodyRoutineDeclFull = (BodyNode) node.Children[2]!.Accept(this);
                        break;
                    case NodeTag.RoutineDeclarationWithType:
                        returnTypeRoutineDecl = (TypeNode) node.Children[1]!.Accept(this);
                        bodyRoutineDeclFull = (BodyNode) node.Children[2]!.Accept(this);
                        break;
                    case NodeTag.RoutineDeclaration:
                        bodyRoutineDeclFull = (BodyNode) node.Children[1]!.Accept(this);
                        break;
                    default:
                        throw new Exception($"Unexpected state in Routine declaration: {node.Tag}");
                }

                ScopeStack.DeleteScope();
                var funcDecl = new FunctionDeclNode(funcNameRoutineDecl, parametersRoutineDecl,
                    returnTypeRoutineDecl, bodyRoutineDeclFull);
                ScopeStack.AddVariable(funcDecl);
                return funcDecl;

            case NodeTag.ParameterDeclaration:
                var idParDecl = (VarNode) node.Children[0]!.Accept(this);
                var typeParDecl = (TypeNode) node.Children[1]!.Accept(this);
                idParDecl.Type = typeParDecl;

                if (!ScopeStack.isFreeInLastScope(idParDecl.Name!))
                    throw new Exception($"Variable with the given name has already declared: {idParDecl.Name}");
                ScopeStack.AddVariable(idParDecl);
                return new ParameterNode(idParDecl, typeParDecl);

            case NodeTag.ParametersContinuous:
                if (node.Children.Length == 1)
                {
                    return new ParametersNode(new List<ParameterNode>
                        {(ParameterNode) node.Children[0]!.Accept(this)});
                }

                var parametersDecl = node.Children[0]!.Accept(this);
                var parameterDecl = node.Children[1]!.Accept(this);

                if (parametersDecl.GetType() == typeof(ParameterNode))
                {
                    return new ParametersNode(new List<ParameterNode>
                        {(ParameterNode) parametersDecl, (ParameterNode) parameterDecl});
                }

                var returnParametersDecl = (ParametersNode) parametersDecl;
                returnParametersDecl.AddParameter((ParameterNode) parameterDecl);
                return returnParametersDecl;
            case NodeTag.RoutineCall:
                var idRoutineCall = (VarNode) node.Children[0]!.Accept(this);


                var function = (FunctionDeclNode) ScopeStack.FindVariable(idRoutineCall.Name!);
                if (function.Parameters == null)
                {
                    if (node.Children.Length == 2)
                        throw new Exception("Unexpected number of arguments. Expected zero arguments");
                    return new RoutineCallNode(function, new ExpressionsNode());
                }

                if (node.Children.Length == 1)
                    throw new Exception(
                        $"Unexpected number of arguments. Got 0, expected {function.Parameters.Parameters.Count}.");
                var exprsRoutineCall = (ExpressionsNode) node.Children[1]!.Accept(this);

                if (exprsRoutineCall.Expressions.Count != function.Parameters.Parameters.Count)
                {
                    throw new Exception(
                        $"Unexpected number of arguments. Got {exprsRoutineCall.Expressions.Count}, expected {function.Parameters.Parameters.Count}.");
                }

                var counter = 0;
                foreach (var nodeExpr in exprsRoutineCall.Expressions)
                {
                    if (!nodeExpr.Type.IsConvertibleTo(function.Parameters.Parameters[counter].Type))
                    {
                        throw new Exception(
                            $"Unexpected type. Got {nodeExpr.Type.MyType}, expected {function.Parameters.Parameters[counter].Type.MyType}");
                    }
                }

                return new RoutineCallNode(function, exprsRoutineCall);
            default:
                throw new Exception($"Unexpected Name Tag: {node.Tag}");
        }
    }

    private TypeNode _isValidUnaryOperation(ValueNode operand, OperationType operationType)
    {
        var booleanType = new TypeNode(MyType.Boolean);
        var realType = new TypeNode(MyType.Real);
        var integerType = new TypeNode(MyType.Integer);
        switch (operationType)
        {
            case OperationType.UnaryMinus:
            case OperationType.UnaryPlus:
                if (!(operand.Type.IsTheSame(booleanType) || operand.Type.IsTheSame(integerType) ||
                      operand.Type.IsTheSame(realType)))
                {
                    throw new Exception($"Can't perform operation {operationType}: incorrect types");
                }

                break;
            case OperationType.Not:
                if (!(operand.Type.IsTheSame(booleanType) || operand.Type.IsTheSame(integerType)))
                {
                    throw new Exception($"Can't perform operation {operationType}: incorrect types");
                }

                break;
            default:
                throw new Exception($"Invalid operation tag: {operationType}");
        }

        return operand.Type;
    }

    private TypeNode _isValidOperation(ValueNode operand1, ValueNode operand2, OperationType operationType)
    {
        var booleanType = new TypeNode(MyType.Boolean);
        var realType = new TypeNode(MyType.Real);
        var integerType = new TypeNode(MyType.Integer);
        switch (operationType)
        {
            case OperationType.And:
            case OperationType.Or:
            case OperationType.Xor:
                if (!(operand1.Type.IsConvertibleTo(booleanType) && operand2.Type.IsConvertibleTo(booleanType)))
                {
                    throw new Exception($"Can't perform operation {operationType}: incorrect types");
                }

                return booleanType;
            case OperationType.Ge:
            case OperationType.Gt:
            case OperationType.Le:
            case OperationType.Lt:
            case OperationType.Eq:
            case OperationType.Ne:
                if (operand1.Type.IsTheSame(realType) || operand2.Type.IsTheSame(realType))
                {
                    if (!(operand1.Type.IsConvertibleTo(realType) && operand2.Type.IsConvertibleTo(realType)))
                    {
                        throw new Exception($"Can't perform operation {operationType}: incorrect types");
                    }
                }

                if (!(operand1.Type.IsConvertibleTo(integerType) && operand2.Type.IsConvertibleTo(integerType)))
                {
                    throw new Exception($"Can't perform operation {operationType}: incorrect types");
                }

                return booleanType;
            case OperationType.Plus:
            case OperationType.Minus:
            case OperationType.Mul:
            case OperationType.Div:
            case OperationType.Rem:
            case OperationType.Assert:
                if (operand1.Type.IsTheSame(realType) || operand2.Type.IsTheSame(realType))
                {
                    if (!(operand1.Type.IsConvertibleTo(realType) && operand2.Type.IsConvertibleTo(realType)))
                    {
                        throw new Exception($"Can't perform operation {operationType}: incorrect types");
                    }

                    return realType;
                }

                if (!(operand1.Type.IsConvertibleTo(integerType) && operand2.Type.IsConvertibleTo(integerType)))
                {
                    throw new Exception($"Can't perform operation {operationType}: incorrect types");
                }

                return integerType;
            default:
                throw new Exception($"Invalid operation tag: {operationType}");
        }
    }

    private OperationType _nodeTagToOperationType(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.And:
                return OperationType.And;
            case NodeTag.Or:
                return OperationType.Or;
            case NodeTag.Xor:
                return OperationType.Xor;
            case NodeTag.Le:
                return OperationType.Le;
            case NodeTag.Lt:
                return OperationType.Lt;
            case NodeTag.Ge:
                return OperationType.Ge;
            case NodeTag.Gt:
                return OperationType.Gt;
            case NodeTag.Eq:
                return OperationType.Eq;
            case NodeTag.Ne:
                return OperationType.Ne;
            case NodeTag.Plus:
                return OperationType.Plus;
            case NodeTag.Minus:
                return OperationType.Minus;
            case NodeTag.Mul:
                return OperationType.Mul;
            case NodeTag.Div:
                return OperationType.Div;
            case NodeTag.Rem:
                return OperationType.Rem;
            case NodeTag.NotExpression:
                return OperationType.Not;
            case NodeTag.SignToInteger:
            case NodeTag.SignToDouble:
                var operationNode = node.Children[0]!;
                return operationNode switch
                {
                    LeafNode<string> leafNode => leafNode.Value switch
                    {
                        "-" => OperationType.UnaryMinus,
                        "+" => OperationType.UnaryPlus,
                        _ => throw new Exception($"Unexpected unary operation: {leafNode.Value}")
                    },
                    _ => throw new Exception("SignToNumber has first child unexpected type")
                };
            default:
                throw new Exception($"Invalid operation tag: {node.Tag}");
        }
    }

    public SymbolicNode ExpressionVisit(ComplexNode node)
    {
        switch (node.Tag)
        {
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
                var operand1 = (ValueNode) node.Children[0]!.Accept(this);
                var operand2 = (ValueNode) node.Children[1]!.Accept(this);
                var operationType = _nodeTagToOperationType(node);
                var resultType = _isValidOperation(operand1, operand2, operationType);
                return new OperationNode(operationType, new List<ValueNode> {operand1, operand2}, resultType);
            case NodeTag.NotExpression:
            case NodeTag.SignToInteger:
            case NodeTag.SignToDouble:
                var operand = (ValueNode) node.Children[0]!.Accept(this);
                operationType = _nodeTagToOperationType(node);
                resultType = _isValidUnaryOperation(operand, operationType);
                return new OperationNode(operationType, new List<ValueNode> {operand}, resultType);
            default:
                throw new Exception($"Invalid operation tag: {node.Tag}");
        }
    }

    public SymbolicNode VisitLeaf<T>(LeafNode<T> node)
    {
        switch (node.Tag)
        {
            case NodeTag.IntegerLiteral:
                return new ConstNode(new TypeNode(MyType.Integer), node.Value!);
            case NodeTag.RealLiteral:
                return new ConstNode(new TypeNode(MyType.Real), node.Value!);
            case NodeTag.Identifier:
                return new VarNode((node.Value! as string)!);
            case NodeTag.PrimitiveType:
                return new TypeNode(_getPrimitiveType((node.Value! as string)!));
            case NodeTag.Unary:
                return new OperationNode((node.Value! as string)! == "-"
                    ? OperationType.UnaryMinus
                    : OperationType.UnaryPlus);
            default:
                throw new Exception($"Unexpected node tag for visiting Leaf node {node.Tag}");
        }
    }

    // private bool _isConvertible(TypeNode var1, TypeNode var2)
    // {
    //     if (var1.IsTheSame(var2)) return true;
    //     return (var1.MyType, var2.MyType) switch
    //     {
    //         (MyType.Integer, MyType.Real) => true,
    //         (MyType.Integer, MyType.Boolean) => true,
    //         (MyType.Real, MyType.Real) => true,
    //         (MyType.Real, MyType.Integer) => true,
    //         (MyType.Real, MyType.Boolean) => true,
    //         (MyType.Boolean, MyType.Integer) => true,
    //         _ => false
    //     };
    // }

    private MyType _getPrimitiveType(string primitiveType)
    {
        return primitiveType switch
        {
            "integer" => MyType.Integer,
            "boolean" => MyType.Boolean,
            "real" => MyType.Real,
            _ => throw new Exception($"Unexpected type of primitive type {primitiveType}")
        };
    }
}