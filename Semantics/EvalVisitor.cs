using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.VisualBasic.CompilerServices;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    public ScopeStack ScopeStack { get; set; } = new();

    public SymbolicNode ProgramVisit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.ProgramRoutineDeclaration or NodeTag.ProgramSimpleDeclaration:
                var progs = node.Children[0] == null ? new ProgramNode() : (ProgramNode) node.Children[0]!.Accept(this);
                var declaration = node.Children[1]!.Accept(this);
                progs.AddDeclaration(declaration);
                return progs;
            default:
                throw new Exception($"Unexpected node tag {node.Tag}");
        }
    }

    public SymbolicNode ModifiablePrimaryVisit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.ModifiablePrimaryGettingField:
                var modPrimFieldBuffer = node.Children[0]!.Accept(this);
                var idFieldBuffer = node.Children[1]!.Accept(this);

                if (idFieldBuffer is not PrimitiveVarNode idField)
                    throw new Exception("Unexpected node type for field");

                var modPrimField = _getFromScopeStackIfNeeded<StructVarNode>(modPrimFieldBuffer);

                return new GetFieldNode(modPrimField, idField); // return VarNode
            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                var arrFromArrBuffer = node.Children[0]!.Accept(this);
                var indexFromArrBuffer = node.Children[1]!.Accept(this);

                if (indexFromArrBuffer is not ValueNode indexFromArr)
                    throw new Exception("Unexpected node type for index");

                var arrFromArr = _getFromScopeStackIfNeeded<ArrayVarNode>(arrFromArrBuffer);

                return new GetByIndexNode(arrFromArr, indexFromArr); // return VarNode
            case NodeTag.ArrayGetSorted:
                var arrGetSortedBuffer = node.Children[0]!.Accept(this);
                var arrGetSorted = _getFromScopeStackIfNeeded<ArrayVarNode>(arrGetSortedBuffer);

                return new SortedArrayNode(arrGetSorted); // TODO return VarNode
            case NodeTag.ArrayGetSize:
                var arrGetSizeBuffer = node.Children[0]!.Accept(this);
                var arrGetSize = _getFromScopeStackIfNeeded<ArrayVarNode>(arrGetSizeBuffer);

                return new ArraySizeNode(arrGetSize); // TODO return VarNode
            case NodeTag.ArrayGetReversed:
                var arrGetReversedBuffer = node.Children[0]!.Accept(this);
                var arrGetReversed = _getFromScopeStackIfNeeded<ArrayVarNode>(arrGetReversedBuffer);

                return new ReversedArrayNode(arrGetReversed); // TODO return VarNode
        }

        throw new Exception("Unimplemented");
    }

    private TDesiredType _getFromScopeStackIfNeeded<TDesiredType>(SymbolicNode node)
    {
        if (node is PrimitiveVarNode varNode)
            return (TDesiredType) Convert.ChangeType(ScopeStack.FindVariable(varNode.Name!), typeof(TDesiredType));
        if (node is TDesiredType) return (TDesiredType) Convert.ChangeType(node, typeof(TDesiredType));
        throw new Exception($"Cannot convert {node} to {typeof(TDesiredType)}");
    }

    private TDesiredType _getDesiredType<TDesiredType>(SymbolicNode node)
    {
        if (node is TDesiredType) return (TDesiredType) Convert.ChangeType(node, typeof(TDesiredType));
        throw new Exception($"Cannot convert {node} to {typeof(TDesiredType)}");
    }

    public SymbolicNode StatementVisit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.VariableDeclarationFull:
            case NodeTag.VariableDeclarationIdenType:
            case NodeTag.VariableDeclarationIdenExpr:
                var variableIdentifierBuffer = node.Children[0]!.Accept(this);
                SymbolicNode? variableTypeBuffer = null;
                SymbolicNode? valueBuffer = null;

                // VarNode variableIdentifier = (VarNode) node.Children[0]!.Accept(this);
                // TypeNode? variableType = null;
                // ValueNode? value = null;
                switch (node.Tag)
                {
                    case NodeTag.VariableDeclarationFull:
                        variableTypeBuffer = node.Children[1]!.Accept(this);
                        valueBuffer = node.Children[2]!.Accept(this);
                        break;
                    case NodeTag.VariableDeclarationIdenType:
                        variableTypeBuffer = node.Children[1]!.Accept(this);
                        break;
                    case NodeTag.VariableDeclarationIdenExpr:
                        valueBuffer = node.Children[1]!.Accept(this);
                        break;
                }

                if (variableIdentifierBuffer is not PrimitiveVarNode variableIdentifier)
                    throw new Exception("Unexpected node type for name of identifier");

                using (var scope = ScopeStack.GetLastScope())
                {
                    if (!scope.IsFree(variableIdentifier.Name!))
                        throw new Exception($"Given name {variableIdentifier.Name} is not free");

                    if (valueBuffer is null)
                    {
                        var typeDeclTypeVar = _getFromScopeStackIfNeeded<TypeNode>(variableTypeBuffer!);
                        var typeDeclarationNode = new TypeVariableDeclaration(variableIdentifier, typeDeclTypeVar);
                        var typeDeclIdVar = typeDeclarationNode.Variable;
                        ScopeStack.AddVariable(typeDeclIdVar);
                        return typeDeclarationNode;
                    }

                    if (variableTypeBuffer is null)
                    {
                        var value = _getDesiredType<ValueNode>(valueBuffer);
                        var valueDeclarationNode = new ValueVariableDeclaration(variableIdentifier, value);
                        var valueDeclIdVar = valueDeclarationNode.Variable;
                        ScopeStack.AddVariable(valueDeclIdVar);
                        return valueDeclarationNode;
                    }

                    var fullDeclType = _getFromScopeStackIfNeeded<TypeNode>(variableTypeBuffer!);
                    var fullDeclValue = _getDesiredType<ValueNode>(valueBuffer);

                    if (!fullDeclValue.Type.IsConvertibleTo(fullDeclType))
                        throw new Exception($"Cannot convert type {fullDeclValue.Type} to {fullDeclType}");

                    var fullDeclNode = new FullVariableDeclaration(variableIdentifier, fullDeclType, fullDeclValue);
                    ScopeStack.AddVariable(fullDeclNode.Variable);
                    return fullDeclNode;
                }
            case NodeTag.VariableDeclarations:
                var declarationsDecl = node.Children[0] != null
                    ? (VariableDeclarations) node.Children[0]!.Accept(this)
                    : new VariableDeclarations(new Dictionary<string, VarNode>());
                var decl = _getDesiredType<DeclarationNode>(node.Children[1]!.Accept(this));
                declarationsDecl.AddDeclaration(decl.Variable);
                return declarationsDecl;
            case NodeTag.TypeDeclaration:
                var typeIdentifierBuffer = node.Children[0]!.Accept(this);
                var typeSynonymBuffer = node.Children[1]!.Accept(this);

                var typeIdentifier = _getDesiredType<PrimitiveVarNode>(typeIdentifierBuffer);
                var typeSynonym = _getFromScopeStackIfNeeded<TypeNode>(typeSynonymBuffer);

                using (var scope = ScopeStack.GetLastScope())
                {
                    if (!scope.IsFree(typeIdentifier.Name!))
                        throw new Exception(
                            $"The user type with name {typeIdentifier.Name} already exists in this scope!");

                    var newTypeVar = new TypeDeclarationNode(typeIdentifier, typeSynonym);
                    scope.AddType(newTypeVar.DeclaredType);
                    return newTypeVar;
                }

            case NodeTag.Break:
                if (!ScopeStack.HasLoopScope()) throw new Exception("Unexpected context for 'break' statement");
                return new BreakNode();
            case NodeTag.Assert:
                var leftAssertExpressionBuffer = node.Children[0]!.Accept(this);
                var rightAssertExpressionBuffer = node.Children[1]!.Accept(this);

                var leftAssertExpression = _getDesiredType<ValueNode>(leftAssertExpressionBuffer);
                var rightAssertExpression = _getDesiredType<ValueNode>(rightAssertExpressionBuffer);

                if (!leftAssertExpression.Type.IsTheSame(rightAssertExpression.Type))
                {
                    _isValidOperation(leftAssertExpression, rightAssertExpression, operationType: OperationType.Assert);
                }

                return new AssertNode(leftAssertExpression, rightAssertExpression);
            case NodeTag.Return:
                if (node.Children.Length == 0) return new EmptyReturnNode();
                var returnValueBuffer = node.Children[0]!.Accept(this);
                var returnValue = _getDesiredType<ValueNode>(returnValueBuffer);

                return new ValueReturnNode(returnValue);
            case NodeTag.Range or NodeTag.RangeReverse:
                var leftBoundBuffer = (ValueNode) node.Children[0]!.Accept(this);
                var rightBoundBuffer = (ValueNode) node.Children[1]!.Accept(this);

                var leftBound = _getDesiredType<ValueNode>(leftBoundBuffer);
                var rightBound = _getDesiredType<ValueNode>(rightBoundBuffer);

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
                var idForLoopBuffer = node.Children[0]!.Accept(this);

                var primitiveVarNode = _getDesiredType<PrimitiveVarNode>(idForLoopBuffer);

                var idForLoop = new VarNode(primitiveVarNode.Name)
                {
                    Type = new TypeNode(MyType.Integer)
                };
                ScopeStack.AddVariable(idForLoop);

                var rangeForLoop = _getDesiredType<RangeNode>(node.Children[1]!.Accept(this));
                var bodyForLoop = _getDesiredType<BodyNode>(node.Children[2]!.Accept(this));

                ScopeStack.DeleteScope();
                return new ForLoopNode(idForLoop, rangeForLoop, bodyForLoop)
                {
                    Type = bodyForLoop.Type
                };

            case NodeTag.ForeachLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var idForEachBuffer = node.Children[0]!.Accept(this);
                var idForEachPrimitiveVarNode = _getDesiredType<PrimitiveVarNode>(idForEachBuffer);

                var fromForEachBuffer = node.Children[1]!.Accept(this);
                var fromForEach = _getFromScopeStackIfNeeded<ArrayVarNode>(fromForEachBuffer);

                var idForEach = DeclarationNode.GetAppropriateVarNode(idForEachPrimitiveVarNode,
                    fromForEach.Type.ElementTypeNode, null);

                ScopeStack.AddVariable(idForEach);

                var bodyForEach = _getDesiredType<BodyNode>(node.Children[2]!.Accept(this));
                ScopeStack.DeleteScope();
                return new ForEachLoopNode(idForEach, fromForEach, bodyForEach)
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
            case NodeTag.BodyStatement or NodeTag.BodySimpleDeclaration:
                var undefinedType = new TypeNode(MyType.Undefined);
                var bodyCont = node.Children[0] != null
                    ? (BodyNode) node.Children[0]!.Accept(this)
                    : new BodyNode(new List<StatementNode>(), new TypeNode(MyType.Undefined));
                var bodyStatement = (StatementNode) node.Children[1]!.Accept(this);

                if (!bodyStatement.Type.IsTheSame(undefinedType) && bodyCont.Type.IsTheSame(undefinedType))
                {
                    bodyCont.Type = bodyStatement.Type;
                }

                var newTypeBody = bodyStatement.Type;
                if (!bodyStatement.Type.IsTheSame(bodyCont.Type))
                {
                    newTypeBody = _isValidOperation(new ValueNode(bodyCont.Type), new ValueNode(bodyStatement.Type),
                        OperationType.Assert);
                }

                bodyCont.Type = newTypeBody;
                bodyCont.AddStatement(bodyStatement);
                return bodyCont;
            case NodeTag.Assignment:
                var idAssignment = (ValueNode) node.Children[0]!.Accept(this);
                var exprAssignment = (ValueNode) node.Children[1]!.Accept(this);

                switch (idAssignment)
                {
                    case GetFieldNode:
                    case GetByIndexNode:
                    case StructVarNode:
                    case ArrayVarNode:
                        break;
                    case VarNode varNode:
                        idAssignment = (ValueNode) ScopeStack.FindVariable(varNode.Name!);
                        break;
                    default:
                        throw new Exception("Unexpected type of node");
                }

                if (!idAssignment.Type.IsTheSame(exprAssignment.Type) &&
                    !exprAssignment.Type.IsConvertibleTo(idAssignment.Type))
                {
                    throw new Exception(
                        $"Unexpected type. Got {exprAssignment.Type.MyType}, expected {idAssignment.Type.MyType}");
                }

                return new AssignmentNode((VarNode) idAssignment, exprAssignment);
            case NodeTag.ArrayType:
                var size = (ValueNode) node.Children[0]!.Accept(this);
                var type = (TypeNode) node.Children[1]!.Accept(this);

                return new ArrayTypeNode(type, size);
            case NodeTag.ArrayTypeWithoutSize:
                var typeWithoutSize = (TypeNode) node.Children[0]!.Accept(this);
                return new ArrayTypeNode(typeWithoutSize);
            case NodeTag.RecordType:
                var declarations = (VariableDeclarations) node.Children[0]!.Accept(this);

                var fields = new Dictionary<string, TypeNode>();

                foreach (var (name, varNode) in declarations.Declarations)
                {
                    fields[name] = ((VarNode) varNode).Type;
                }

                return new StructTypeNode(fields);

            default:
                throw new Exception($"Unexpected node tag: {node.Tag}");
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
                        var interNode = node.Children[1]!.Accept(this);

                        parametersRoutineDecl = interNode is not ParameterNode
                            ? (ParametersNode) interNode
                            : new ParametersNode(new List<ParameterNode> {(ParameterNode) interNode});
                        returnTypeRoutineDecl = (TypeNode) node.Children[2]!.Accept(this);
                        bodyRoutineDeclFull = (BodyNode) node.Children[3]!.Accept(this);
                        break;
                    case NodeTag.RoutineDeclarationWithParams:
                        var interNodeDecl = node.Children[1]!.Accept(this);

                        parametersRoutineDecl = interNodeDecl is not ParameterNode
                            ? (ParametersNode) interNodeDecl
                            : new ParametersNode(new List<ParameterNode> {(ParameterNode) interNodeDecl});
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

                var returnType = new TypeNode(MyType.Undefined);
                if (returnTypeRoutineDecl != null)
                {
                    returnType = returnTypeRoutineDecl;
                }

                if (!bodyRoutineDeclFull.Type.IsConvertibleTo(returnType))
                {
                    throw new Exception(
                        $"Unexpected return type. Got {bodyRoutineDeclFull.Type.MyType}, expected {returnType.MyType}");
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
        if (operand.Type is StructTypeNode or ArrayTypeNode)
        {
            throw new Exception($"Can't perform operation {operationType}: incorrect types");
        }

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
        if (operand1.Type is StructTypeNode or ArrayTypeNode || operand2.Type is StructTypeNode or ArrayTypeNode)
        {
            throw new Exception($"Can't perform operation {operationType}: incorrect types");
        }

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

    private ConstNode _performOperation(ConstNode operand1, ConstNode operand2, TypeNode resultType,
        Func<dynamic, dynamic, dynamic> operation)
    {
        var booleanType = new TypeNode(MyType.Boolean);
        var realType = new TypeNode(MyType.Real);
        var integerType = new TypeNode(MyType.Integer);
        if (resultType.IsTheSame(realType))
        {
            return new ConstNode(resultType,
                operation(Convert.ToDouble(operand1.Value), Convert.ToDouble(operand2.Value)));
        }

        if (resultType.IsTheSame(integerType))
        {
            return new ConstNode(resultType,
                operation(Convert.ToInt32(operand1.Value), Convert.ToInt32(operand2.Value)));
        }

        if (resultType.IsTheSame(booleanType))
        {
            return new ConstNode(resultType,
                operation(Convert.ToBoolean(operand1.Value), Convert.ToBoolean(operand2.Value)));
        }

        throw new Exception($"resultType {resultType} is neither boolean, integer nor real");
    }

    private ConstNode _performOperation(ConstNode operand, TypeNode resultType, Func<dynamic, dynamic> operation)
    {
        var booleanType = new TypeNode(MyType.Boolean);
        var realType = new TypeNode(MyType.Real);
        var integerType = new TypeNode(MyType.Integer);
        if (resultType.IsTheSame(realType))
        {
            return new ConstNode(resultType, operation(Convert.ToDouble(operand.Value)));
        }

        if (resultType.IsTheSame(integerType))
        {
            return new ConstNode(resultType, operation(Convert.ToInt32(operand.Value)));
        }

        if (resultType.IsTheSame(booleanType))
        {
            return new ConstNode(resultType, operation(Convert.ToBoolean(operand.Value)));
        }

        throw new Exception($"resultType {resultType} is neither boolean, integer nor real");
    }

    private ConstNode _simplifyOperation(ConstNode operand, TypeNode resultType, OperationType operationType)
    {
        return operationType switch
        {
            OperationType.UnaryMinus => PerformOperation((a) => -a),
            OperationType.UnaryPlus => PerformOperation((a) => +a),
            OperationType.Not => PerformOperation((a) => !Convert.ToBoolean(a)),
            _ => throw new Exception("Something is wrong in _simplifyUnaryOperation")
        };

        ConstNode PerformOperation(Func<dynamic, dynamic> operation)
        {
            return _performOperation(operand, resultType, operation);
        }
    }

    private ConstNode _simplifyOperation(ConstNode operand1, ConstNode operand2, TypeNode resultType,
        OperationType operationType)
    {
        return operationType switch
        {
            OperationType.And => PerformOperation((a, b) => a && b),
            OperationType.Or => PerformOperation((a, b) => a || b),
            OperationType.Xor => PerformOperation((a, b) => a ^ b),
            OperationType.Ge => PerformOperation((a, b) => a >= b),
            OperationType.Gt => PerformOperation((a, b) => a > b),
            OperationType.Le => PerformOperation((a, b) => a <= b),
            OperationType.Lt => PerformOperation((a, b) => a < b),
            OperationType.Eq => PerformOperation((a, b) => a == b),
            OperationType.Ne => PerformOperation((a, b) => a != b),
            OperationType.Plus => PerformOperation((a, b) => a + b),
            OperationType.Minus => PerformOperation((a, b) => a - b),
            OperationType.Mul => PerformOperation((a, b) => a * b),
            // TODO division by 0
            OperationType.Div => PerformOperation((a, b) => a / b),
            OperationType.Rem => PerformOperation((a, b) => a % b),
            _ => throw new Exception("Unknown operation")
        };

        ConstNode PerformOperation(Func<dynamic, dynamic, dynamic> operation)
        {
            return _performOperation(operand1, operand2, resultType, operation);
        }
    }

    private ValueNode _getVarForIdentifier(ValueNode identifier)
    {
        if (identifier is not VarNode varNode1) return identifier;
        var value1 = ScopeStack.FindVariable(varNode1.Name!);
        return value1 switch
        {
            TypeNode typeNode => throw new Exception(
                $"Can't perform operation on user-defined type {typeNode.MyType}"),
            VarNode {Type.MyType: MyType.Undefined} => throw new Exception(
                "Can't perform operation on undefined type"),
            VarNode varNode => varNode,
            _ => identifier
        };
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
                operand1 = _getVarForIdentifier(operand1);
                operand2 = _getVarForIdentifier(operand2);
                var operationType = _nodeTagToOperationType(node);
                var resultType = _isValidOperation(operand1, operand2, operationType);
                if (operand1 is ConstNode constNode1 && operand2 is ConstNode constNode2)
                {
                    var simplifiedConstNode = _simplifyOperation(constNode1, constNode2, resultType, operationType);
                    return simplifiedConstNode;
                }

                return new OperationNode(operationType, new List<ValueNode> {operand1, operand2}, resultType);
            case NodeTag.NotExpression:
            case NodeTag.SignToInteger:
            case NodeTag.SignToDouble:
                var operand = (ValueNode) node.Children[0]!.Accept(this);
                operationType = _nodeTagToOperationType(node);
                resultType = _isValidUnaryOperation(operand, operationType);
                if (operand is ConstNode constNode)
                {
                    return _simplifyOperation(constNode, resultType, operationType);
                }

                return new OperationNode(operationType, new List<ValueNode> {operand}, resultType);
            case NodeTag.ArrayConst:
                var expressions = (ExpressionsNode) node.Children[0]!.Accept(this);

                var typeExpr = new TypeNode();
                var realType = new TypeNode(MyType.Real);
                var integerType = new TypeNode(MyType.Integer);
                var booleanType = new TypeNode(MyType.Boolean);

                foreach (var expr in expressions.Expressions)
                {
                    if (expr.Type.IsTheSame(realType) &&
                        typeExpr.MyType is MyType.Undefined or MyType.Boolean or MyType.Integer) typeExpr = realType;
                    if (expr.Type.IsTheSame(integerType) && typeExpr.MyType is MyType.Undefined or MyType.Boolean)
                        typeExpr = integerType;
                    if (expr.Type.IsTheSame(booleanType) && typeExpr.MyType is MyType.Undefined) typeExpr = booleanType;

                    if (expr.Type.MyType == MyType.CompoundType && typeExpr.MyType != MyType.Undefined)
                    {
                        throw new Exception($"Cannot conform types: {expr.Type.GetType()}, {typeExpr.GetType()}");
                    }
                }

                return new ArrayConst(expressions);
            default:
                throw new Exception($"Invalid operation tag: {node.Tag}");
        }
    }

    public SymbolicNode VisitLeaf<T>(LeafNode<T> node)
    {
        switch (node.Tag)
        {
            case NodeTag.BooleanLiteral:
                return new ConstNode(new TypeNode(MyType.Boolean), node.Value!);
            case NodeTag.IntegerLiteral:
                return new ConstNode(new TypeNode(MyType.Integer), node.Value!);
            case NodeTag.RealLiteral:
                return new ConstNode(new TypeNode(MyType.Real), node.Value!);
            case NodeTag.Identifier:
                return new PrimitiveVarNode((node.Value! as string)!);
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