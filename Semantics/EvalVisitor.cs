﻿using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.VisualBasic.CompilerServices;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    public SemanticsScopeStack SemanticsScopeStack { get; set; } = new();

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

                var modPrimField =
                    (StructVarNode) _getFromScopeStackIfNeeded(modPrimFieldBuffer);
                return new GetFieldNode(modPrimField, idField); // return VarNode
            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                var arrFromArrBuffer = node.Children[0]!.Accept(this);
                var indexFromArrBuffer = node.Children[1]!.Accept(this);

                if (_getFromScopeStackIfNeeded(indexFromArrBuffer) is not ValueNode indexFromArr)
                    throw new Exception("Unexpected node type for index");

                var arrFromArr = (ArrayVarNode) _getFromScopeStackIfNeeded(arrFromArrBuffer);

                return new GetByIndexNode(arrFromArr, indexFromArr); // return VarNode
            case NodeTag.ArrayGetSorted:
                var arrGetSortedBuffer = node.Children[0]!.Accept(this);
                var arrGetSorted = (ValueNode) _getFromScopeStackIfNeeded(arrGetSortedBuffer);

                if (arrGetSorted.Type is not ArrayTypeNode arrGetSortedType)
                    throw new Exception($"Cannot sort {arrGetSorted.Type.GetType()}");

                if (arrGetSortedType.ElementTypeNode.MyType == MyType.CompoundType)
                {
                    throw new Exception("Cannot sort compound types!");
                }

                return new SortedArrayNode(arrGetSorted); // TODO return VarNode
            case NodeTag.ArrayGetSize:
                var arrGetSizeBuffer = node.Children[0]!.Accept(this);
                var arrGetSize = (VarNode) _getFromScopeStackIfNeeded(arrGetSizeBuffer);

                if (arrGetSize.Type is not ArrayTypeNode)
                    throw new Exception($"Cannot sort {arrGetSize.Type.GetType()}");

                return new ArraySizeNode(arrGetSize); // TODO return VarNode
            case NodeTag.ArrayGetReversed:
                var arrGetReversedBuffer = node.Children[0]!.Accept(this);
                var arrGetReversed =
                    (ValueNode) _getFromScopeStackIfNeeded(arrGetReversedBuffer);

                if (arrGetReversed.Type is not ArrayTypeNode)
                    throw new Exception($"Cannot sort {arrGetReversed.Type.GetType()}");

                return new ReversedArrayNode(arrGetReversed); // TODO return VarNode
        }

        throw new Exception("Unimplemented");
    }

    private object _getFromScopeStackIfNeeded(SymbolicNode node)
    {
        if (node is PrimitiveVarNode varNode)
        {
            var foundVar = SemanticsScopeStack.FindVariable(varNode.Name);
            return foundVar;
        }

        return node;
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
                        variableTypeBuffer = ((TypeNode) _getFromScopeStackIfNeeded(node.Children[1]!.Accept(this)));
                        valueBuffer = node.Children[2]!.Accept(this);
                        break;
                    case NodeTag.VariableDeclarationIdenType:
                        variableTypeBuffer = ((TypeNode) _getFromScopeStackIfNeeded(node.Children[1]!.Accept(this)));
                        break;
                    case NodeTag.VariableDeclarationIdenExpr:
                        valueBuffer = node.Children[1]!.Accept(this);
                        break;
                }

                if (variableIdentifierBuffer is not PrimitiveVarNode variableIdentifier)
                    throw new Exception("Unexpected node type for name of identifier");

                using (var scope = SemanticsScopeStack.GetLastScope())
                {
                    if (!scope.IsFree(variableIdentifier.Name!))
                        throw new Exception($"Given name {variableIdentifier.Name} is not free");

                    if (valueBuffer is null)
                    {
                        var typeDeclTypeVar =
                            (TypeNode) _getFromScopeStackIfNeeded(variableTypeBuffer!);
                        var typeDeclarationNode = new TypeVariableDeclaration(variableIdentifier, typeDeclTypeVar);
                        var typeDeclIdVar = typeDeclarationNode.Variable;
                        SemanticsScopeStack.AddVariable(typeDeclIdVar);
                        return typeDeclarationNode;
                    }

                    if (variableTypeBuffer is null)
                    {
                        var value = (ValueNode) _getFromScopeStackIfNeeded(valueBuffer);
                        var valueDeclarationNode = new ValueVariableDeclaration(variableIdentifier, value);
                        var valueDeclIdVar = valueDeclarationNode.Variable;
                        SemanticsScopeStack.AddVariable(valueDeclIdVar);
                        return valueDeclarationNode;
                    }

                    var fullDeclType = (TypeNode) _getFromScopeStackIfNeeded(variableTypeBuffer!);
                    var fullDeclValue = (ValueNode) _getFromScopeStackIfNeeded(valueBuffer);

                    if (!fullDeclValue.Type.IsConvertibleTo(fullDeclType))
                        throw new Exception($"Cannot convert type {fullDeclValue.Type} to {fullDeclType}");

                    var fullDeclNode = new FullVariableDeclaration(variableIdentifier, fullDeclType, fullDeclValue);
                    SemanticsScopeStack.AddVariable(fullDeclNode.Variable);
                    return fullDeclNode;
                }
            case NodeTag.VariableDeclarations:
                var declarationsDecl = node.Children[0] != null
                    ? (VariableDeclarations) node.Children[0]!.Accept(this)
                    : new VariableDeclarations(new Dictionary<string, VarNode>());
                var decl = (DeclarationNode) node.Children[1]!.Accept(this);
                declarationsDecl.AddDeclaration(decl.Variable);
                declarationsDecl.DeclarationNodes.Add(decl);
                return declarationsDecl;
            case NodeTag.TypeDeclaration:
                var typeIdentifierBuffer = node.Children[0]!.Accept(this);
                var typeSynonymBuffer = node.Children[1]!.Accept(this);

                var typeIdentifier = (PrimitiveVarNode) typeIdentifierBuffer;
                var typeSynonym = (TypeNode) _getFromScopeStackIfNeeded(typeSynonymBuffer);

                using (var scope = SemanticsScopeStack.GetLastScope())
                {
                    if (!scope.IsFree(typeIdentifier.Name!))
                        throw new Exception(
                            $"The user type with name {typeIdentifier.Name} already exists in this scope!");

                    var newTypeVar = new TypeDeclarationNode(typeIdentifier, typeSynonym);
                    scope.AddType(newTypeVar.DeclaredType);
                    // return newTypeVar;
                    return null;
                }

            case NodeTag.Break:
                if (!SemanticsScopeStack.HasLoopScope())
                    throw new Exception("Unexpected context for 'break' statement");
                return new BreakNode();
            case NodeTag.Assert:
                var leftAssertExpressionBuffer = node.Children[0]!.Accept(this);
                var rightAssertExpressionBuffer = node.Children[1]!.Accept(this);

                var leftAssertExpression = (ValueNode) _getFromScopeStackIfNeeded(leftAssertExpressionBuffer);
                var rightAssertExpression = (ValueNode) _getFromScopeStackIfNeeded(rightAssertExpressionBuffer);

                if (!leftAssertExpression.Type.IsTheSame(rightAssertExpression.Type))
                {
                    _isValidOperation(leftAssertExpression, rightAssertExpression, operationType: OperationType.Assert);
                }

                return new AssertNode(leftAssertExpression, rightAssertExpression);
            case NodeTag.Return:
                if (node.Children.Length == 0) return new EmptyReturnNode();
                var returnValueBuffer = node.Children[0]!.Accept(this);
                var returnValue = (ValueNode) _getFromScopeStackIfNeeded(returnValueBuffer);

                return new ValueReturnNode(returnValue);
            case NodeTag.Range or NodeTag.RangeReverse:
                var leftBoundBuffer = node.Children[0]!.Accept(this);
                var rightBoundBuffer = node.Children[1]!.Accept(this);

                var leftBound = (ValueNode) _getFromScopeStackIfNeeded(leftBoundBuffer);
                var rightBound = (ValueNode) _getFromScopeStackIfNeeded(rightBoundBuffer);

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
                SemanticsScopeStack.NewScope(SemanticsScope.ScopeContext.Loop);
                var idForLoopBuffer = node.Children[0]!.Accept(this);

                var primitiveVarNode = (PrimitiveVarNode) idForLoopBuffer;

                var idForLoop = new VarNode(primitiveVarNode.Name)
                {
                    Type = new TypeNode(MyType.Integer)
                };
                SemanticsScopeStack.AddVariable(idForLoop);

                var rangeForLoop = (RangeNode) node.Children[1]!.Accept(this);
                var bodyForLoop = node.Children[2] is null ? new BodyNode() : (BodyNode) node.Children[2]!.Accept(this);

                SemanticsScopeStack.DeleteScope();
                return new ForLoopNode(idForLoop, rangeForLoop, bodyForLoop)
                {
                    Type = bodyForLoop.Type
                };

            case NodeTag.ForeachLoop:
                SemanticsScopeStack.NewScope(SemanticsScope.ScopeContext.Loop);
                var idForEachBuffer = node.Children[0]!.Accept(this);
                var idForEachPrimitiveVarNode =
                    (PrimitiveVarNode) idForEachBuffer;

                var fromForEachBuffer = node.Children[1]!.Accept(this);
                var fromForEach = (ArrayVarNode) _getFromScopeStackIfNeeded(fromForEachBuffer);

                var idForEach = DeclarationNode.GetAppropriateVarNode(idForEachPrimitiveVarNode,
                    fromForEach.Type.ElementTypeNode, null);

                SemanticsScopeStack.AddVariable(idForEach);

                var bodyForEach = node.Children[2] is null ? new BodyNode() : (BodyNode) node.Children[2]!.Accept(this);
                SemanticsScopeStack.DeleteScope();
                return new ForEachLoopNode(idForEach, fromForEach, bodyForEach)
                {
                    Type = bodyForEach.Type
                };
            case NodeTag.WhileLoop:
                SemanticsScopeStack.NewScope(SemanticsScope.ScopeContext.Loop);
                var condExprWhile = (ValueNode) _getFromScopeStackIfNeeded(node.Children[0]!.Accept(this));
                if (!condExprWhile.Type.IsConvertibleTo(new TypeNode(MyType.Boolean)))
                {
                    throw new Exception(
                        $"Unexpected type for while loop condition: Got {condExprWhile.Type.MyType}, expected boolean");
                }

                var bodyWhile = node.Children[1] is null ? new BodyNode() : (BodyNode) node.Children[1]!.Accept(this);

                SemanticsScopeStack.DeleteScope();
                return new WhileLoopNode(condExprWhile, bodyWhile)
                {
                    Type = bodyWhile.Type
                };

            case NodeTag.IfStatement:
                SemanticsScopeStack.NewScope(SemanticsScope.ScopeContext.IfStatement);
                var condIf = (ValueNode) _getFromScopeStackIfNeeded(node.Children[0]!.Accept(this));
                if (!condIf.Type.IsConvertibleTo(new TypeNode(MyType.Boolean)))
                {
                    throw new Exception(
                        $"Unexpected type for if statement condition: Got {condIf.Type.MyType}, expected boolean");
                }

                var bodyIf = node.Children[1] is null ? new BodyNode() : (BodyNode) node.Children[1]!.Accept(this);
                SemanticsScopeStack.DeleteScope();
                return new IfStatement(condIf, bodyIf)
                {
                    Type = bodyIf.Type
                };

            case NodeTag.IfElseStatement:
                SemanticsScopeStack.NewScope(SemanticsScope.ScopeContext.IfStatement);
                var condIfElse = (ValueNode) _getFromScopeStackIfNeeded(node.Children[0]!.Accept(this));
                if (!condIfElse.Type.IsConvertibleTo(new TypeNode(MyType.Boolean)))
                {
                    throw new Exception(
                        $"Unexpected type for if statement condition: Got {condIfElse.Type.MyType}, expected boolean");
                }

                var bodyIfElse = node.Children[1] is null
                    ? new BodyNode(new List<StatementNode>(), new TypeNode())
                    : (BodyNode) node.Children[1]!.Accept(this);
                var bodyElse = node.Children[2] is null
                    ? new BodyNode(new List<StatementNode>(), new TypeNode())
                    : (BodyNode) node.Children[2]!.Accept(this);

                var newType = bodyIfElse.Type;
                var undefinedTypeIf = new TypeNode();

                if (bodyIfElse.Type.IsTheSame(undefinedTypeIf))
                {
                    newType = bodyElse.Type;
                }
                else if (!bodyElse.Type.IsTheSame(undefinedTypeIf) && !bodyIfElse.Type.IsTheSame(bodyElse.Type))
                {
                    newType = _isValidOperation(new ValueNode(bodyElse.Type), new ValueNode(bodyIfElse.Type),
                        OperationType.Assert);
                }

                SemanticsScopeStack.DeleteScope();
                return new IfElseStatement(condIfElse, bodyIfElse, bodyElse)
                {
                    Type = newType
                };

            case NodeTag.BodyStatement or NodeTag.BodySimpleDeclaration:
                var undefinedType = new TypeNode(MyType.Undefined);

                var bodyCont = node.Children[0] != null
                    ? (BodyNode) node.Children[0]!.Accept(this)
                    : new BodyNode(new List<StatementNode>(), new TypeNode(MyType.Undefined));

                var bodyStatement =
                    (StatementNode) node.Children[1]!.Accept(this);

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
                var idAssignment = (ValueNode) _getFromScopeStackIfNeeded(node.Children[0]!.Accept(this));
                var exprAssignment = (ValueNode) _getFromScopeStackIfNeeded(node.Children[1]!.Accept(this));

                switch (idAssignment)
                {
                    case GetFieldNode:
                    case GetByIndexNode:
                    case StructVarNode:
                    case ArrayVarNode:
                        break;
                    case PrimitiveVarNode primitiveVarNodeAssignment:
                        idAssignment = (ValueNode) SemanticsScopeStack.FindVariable(primitiveVarNodeAssignment.Name);
                        break;
                    case VarNode:
                        break;
                    default:
                        throw new Exception($"Unexpected type of node {idAssignment.GetType()}");
                }

                if (!idAssignment.Type.IsTheSame(exprAssignment.Type) &&
                    !exprAssignment.Type.IsConvertibleTo(idAssignment.Type))
                {
                    throw new Exception(
                        $"Unexpected type. Got {exprAssignment.Type.GetType()}, expected {idAssignment.Type.GetType()}");
                }

                return new AssignmentNode(idAssignment, exprAssignment);

            case NodeTag.ArrayType:
                var size = (ValueNode) _getFromScopeStackIfNeeded(node.Children[0]!.Accept(this));
                var type = (TypeNode) _getFromScopeStackIfNeeded(node.Children[1]!.Accept(this));

                return new ArrayTypeNode(type, size);
            case NodeTag.ArrayTypeWithoutSize:
                var typeWithoutSize =
                    (TypeNode) _getFromScopeStackIfNeeded(node.Children[0]!.Accept(this));
                return new ArrayTypeNode(typeWithoutSize);

            case NodeTag.RecordType:
                SemanticsScopeStack.NewScope(SemanticsScope.ScopeContext.RecordDeclaration);
                var declarations =
                    (VariableDeclarations) node.Children[0]!.Accept(this);
                var variableScope = SemanticsScopeStack.GetLastScope();
                SemanticsScopeStack.DeleteScope();

                var fields = new Dictionary<string, TypeNode>();

                // Check for default values
                var withDefaultValues =
                    declarations.DeclarationNodes[0] is FullVariableDeclaration or ValueVariableDeclaration;
                foreach (var declaration in declarations.DeclarationNodes)
                {
                    switch (declaration)
                    {
                        case FullVariableDeclaration or ValueVariableDeclaration when withDefaultValues:
                        case TypeVariableDeclaration when !withDefaultValues:
                            continue;
                        default:
                            throw new Exception("All record field should be either with default value or without it");
                    }
                }

                foreach (var (name, varNode) in declarations.Declarations)
                {
                    fields[name] = varNode.Type;
                }

                return new StructTypeNode(fields)
                {
                    DefaultValues = variableScope.GetVariables()
                };

            case NodeTag.Cast:
                var typeCastBuffer = node.Children[0]!.Accept(this);
                var typeValueBuffer = node.Children[1]!.Accept(this);

                var typeCast = (TypeNode) _getFromScopeStackIfNeeded(typeCastBuffer);
                var typeValue = (ValueNode) _getFromScopeStackIfNeeded(typeValueBuffer);

                if (!typeValue.Type.IsConvertibleTo(typeCast))
                    throw new Exception($"Cannot convert {typeValue.Type} to {typeCast}");

                return new CastNode(typeCast, typeValue);

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
                var funcNameRoutineDecl = (PrimitiveVarNode) node.Children[0]!.Accept(this);

                ParametersNode? parametersRoutineDecl = null;
                TypeNode? returnTypeRoutineDecl = null;
                BodyNode? bodyRoutineDeclFull;
                int bodyIndex = 0;
                SemanticsScopeStack.NewScope(SemanticsScope.ScopeContext.Routine);
                switch (node.Tag)
                {
                    case NodeTag.RoutineDeclarationWithTypeAndParams:
                        var interNode = node.Children[1]!.Accept(this);
                        parametersRoutineDecl = interNode is not ParameterNode declTypeParam
                            ? (ParametersNode) interNode
                            : new ParametersNode(new List<ParameterNode> {declTypeParam});

                        returnTypeRoutineDecl = (TypeNode) _getFromScopeStackIfNeeded(node.Children[2]!.Accept(this));
                        bodyIndex = 3;
                        break;
                    case NodeTag.RoutineDeclarationWithParams:
                        var interNodeDecl = node.Children[1]!.Accept(this);

                        parametersRoutineDecl = interNodeDecl is not ParameterNode declParam
                            ? (ParametersNode) interNodeDecl
                            : new ParametersNode(new List<ParameterNode> {declParam});
                        bodyIndex = 2;
                        break;
                    case NodeTag.RoutineDeclarationWithType:
                        returnTypeRoutineDecl = (TypeNode) _getFromScopeStackIfNeeded(node.Children[1]!.Accept(this));
                        bodyIndex = 2;
                        break;
                    case NodeTag.RoutineDeclaration:
                        bodyIndex = 1;
                        break;
                    default:
                        throw new Exception($"Unexpected state in Routine declaration: {node.Tag}");
                }

                var returnType = new TypeNode(MyType.Undefined);
                if (returnTypeRoutineDecl is not null)
                {
                    returnType = returnTypeRoutineDecl;
                }

                SemanticsScopeStack.AddVariable(new RoutineDeclarationNode(funcNameRoutineDecl, parametersRoutineDecl,
                    returnTypeRoutineDecl, new BodyNode()));
                bodyRoutineDeclFull = node.Children[bodyIndex] is null
                    ? new BodyNode()
                    : (BodyNode) node.Children[bodyIndex]!.Accept(this);

                if (!bodyRoutineDeclFull.Type.IsConvertibleTo(returnType))
                {
                    throw new Exception(
                        $"Unexpected return type. Got {bodyRoutineDeclFull.Type.MyType}, expected {returnType.MyType}");
                }

                SemanticsScopeStack.DeleteScope();
                var funcDecl = new RoutineDeclarationNode(funcNameRoutineDecl, parametersRoutineDecl,
                    returnTypeRoutineDecl, bodyRoutineDeclFull);
                SemanticsScopeStack.AddVariable(funcDecl);
                return funcDecl;

            case NodeTag.ParameterDeclaration:
                var idParDeclBuffer = (PrimitiveVarNode) node.Children[0]!.Accept(this);
                var typeParDecl = (TypeNode) _getFromScopeStackIfNeeded(node.Children[1]!.Accept(this));

                var idParDecl = DeclarationNode.GetAppropriateVarNode(idParDeclBuffer, typeParDecl, null);

                if (!SemanticsScopeStack.isFreeInLastScope(idParDecl.Name!))
                    throw new Exception($"Variable with the given name has already declared: {idParDecl.Name}");
                SemanticsScopeStack.AddVariable(idParDecl);
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
                var idRoutineCallBuffer = (PrimitiveVarNode) node.Children[0]!.Accept(this);

                var function =
                    (RoutineDeclarationNode) _getFromScopeStackIfNeeded(
                        SemanticsScopeStack.FindVariable(idRoutineCallBuffer.Name));
                if (function.Parameters is null)
                {
                    if (node.Children.Length == 2)
                        throw new Exception("Unexpected number of arguments. Expected zero arguments");
                    return new RoutineCallNode(function, new ExpressionsNode());
                }

                if (node.Children.Length == 1)
                    throw new Exception(
                        $"Unexpected number of arguments. Got 0, expected {function.Parameters.Parameters.Count}.");
                var exprsRoutineCallBuffer = node.Children[1]!.Accept(this);
                if (exprsRoutineCallBuffer is not (ExpressionsNode or ValueNode))
                    throw new Exception(
                        $"Unexpected node type. Expected Value node, got {exprsRoutineCallBuffer.GetType()}");

                var exprsRoutineCall = exprsRoutineCallBuffer is ExpressionsNode expressionsNode
                    ? expressionsNode
                    : new ExpressionsNode(new List<ValueNode>
                        {(ValueNode) _getFromScopeStackIfNeeded(exprsRoutineCallBuffer)});

                if (exprsRoutineCall.Expressions.Count != function.Parameters.Parameters.Count)
                {
                    throw new Exception(
                        $"Unexpected number of arguments. Got {exprsRoutineCall.Expressions.Count}, expected {function.Parameters.Parameters.Count}.");
                }

                for (var counter = 0; counter < exprsRoutineCall.Expressions.Count; counter++)
                {
                    var nodeExpr = exprsRoutineCall.Expressions[counter];
                    var functionParam = function.Parameters.Parameters[counter];

                    if (!nodeExpr.Type.IsConvertibleTo(functionParam.Type))
                    {
                        throw new Exception(
                            $"Unexpected type. Got {nodeExpr.Type.MyType}, expected {functionParam.Type.MyType}");
                    }
                }

                return new RoutineCallNode(function, exprsRoutineCall);
            case NodeTag.ExpressionsContinuous:
                var expressionsContBuffer = node.Children[0]!.Accept(this);
                var expressionContBuffer = node.Children[1]!.Accept(this);

                var expressionsContNode = new ExpressionsNode();
                if (expressionsContBuffer is ValueNode expressionsCont)
                {
                    expressionsContNode.AddExpression((ValueNode) _getFromScopeStackIfNeeded(expressionsCont));
                }
                else if (expressionsContBuffer is ExpressionsNode exprNode) expressionsContNode = exprNode;
                else
                    throw new Exception(
                        $"Unexpected node type. Expected either ValueNode or ExpressionsNode, got {expressionsContBuffer.GetType()}");

                if (expressionContBuffer is not ValueNode expressionCont)
                    throw new Exception(
                        $"Unexpected node type. Expected ValueNode, got {expressionContBuffer.GetType()}");

                var expressionsType = _isValidOperation(new ValueNode(expressionCont.Type),
                    new ValueNode(expressionsContNode.Expressions[^1].Type), OperationType.Assert);

                expressionsContNode.AddExpression(expressionCont);
                return expressionsContNode;
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
        if (identifier is not PrimitiveVarNode varNode1) return identifier;
        var value1 = SemanticsScopeStack.FindVariable(varNode1.Name);
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
                ValueNode? operand = null;
                if (node.Tag == NodeTag.NotExpression) operand = (ValueNode) node.Children[0]!.Accept(this);
                else operand = (ValueNode) node.Children[1]!.Accept(this);
                operationType = _nodeTagToOperationType(node);
                resultType = _isValidUnaryOperation(operand, operationType);
                if (operand is ConstNode constNode)
                {
                    return _simplifyOperation(constNode, resultType, operationType);
                }

                return new OperationNode(operationType, new List<ValueNode> {operand}, resultType);
            case NodeTag.ArrayConst:
                var expressionsBuffer = node.Children[0]!.Accept(this);

                var expressions = expressionsBuffer is ExpressionsNode buffer
                    ? buffer
                    : new ExpressionsNode(new List<ValueNode> {(ValueNode) expressionsBuffer});

                var typeExpr = expressions.Expressions.Count == 0 ? new TypeNode() : expressions.Expressions[0].Type;
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

                    if (expr.Type.MyType == MyType.CompoundType && !typeExpr.IsTheSame(expr.Type))
                    {
                        throw new Exception($"Cannot conform types: {expr.Type.GetType()}, {typeExpr.GetType()}");
                    }
                }

                return new ArrayConst(expressions)
                {
                    Type = new ArrayTypeNode(typeExpr)
                    {
                        Size = new ConstNode(integerType, expressions.Expressions.Count),
                    }
                };
            case NodeTag.EmptyArrayConst:
                return new ArrayConst(new ExpressionsNode()) {Type = new ArrayTypeNode(new TypeNode())};
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