using System.Linq.Expressions;
using System.Numerics;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    public ScopeStack ScopeStack { get; set; } = new();

    public SymbolicNode Visit(ComplexNode node)
    {
        switch (node.Tag)
        {
            case NodeTag.VariableDeclarationIdenType:
                var identifierNode = UniversalVisit(node.Children[0]);
                var typeNode = UniversalVisit(node.Children[1]);

                if (typeNode.MyType != MyType.PrimitiveType && typeNode.MyType != MyType.CompoundType)
                    throw new Exception($"Unexpected type ({typeNode.MyType})");
                if (identifierNode.MyType != MyType.Undefined) throw new Exception("Unexpected type");

                if (typeNode.MyType == MyType.PrimitiveType)
                    identifierNode.MyType = GetTypeFromPrimitiveType((typeNode.Value as string)!);
                else
                {
                    identifierNode.MyType = MyType.CompoundType;
                    identifierNode.CompoundType = typeNode;
                }

                identifierNode.IsInitialized = false;
                ScopeStack.AddVariable(identifierNode);
                return identifierNode;
            case NodeTag.VariableDeclarationIdenExpr:
                var identifier = UniversalVisit(node.Children[0]);
                var expr = UniversalVisit(node.Children[1]);

                if (identifier.MyType != MyType.Undefined)
                    throw new Exception($"Unexpected type ({identifier.MyType})");

                identifier.MyType = expr.MyType;
                if (expr.MyType == MyType.CompoundType)
                {
                    identifier.CompoundType = expr.CompoundType;
                }

                // a + b (int)
                identifier.Value = expr;
                identifier.IsInitialized = true;

                ScopeStack.AddVariable(identifier);
                return new SymbolicNode(myType: expr.MyType);
            case NodeTag.VariableDeclarationFull:
                var idDeclFull = UniversalVisit(node.Children[0]);
                var typeDeclFull = UniversalVisit(node.Children[1]);
                var exprDeclFull = UniversalVisit(node.Children[2]);

                if (idDeclFull.MyType != MyType.Undefined) throw new Exception("Unexpected type");

                var typeDeclFullMyType = typeDeclFull.MyType;
                if (typeDeclFullMyType == MyType.PrimitiveType)
                    typeDeclFullMyType = GetTypeFromPrimitiveType((typeDeclFull.Value as string)!);
                if (exprDeclFull.MyType != typeDeclFullMyType)
                    throw new Exception(
                        $"Type of declared variable doesn't match with expression type ({typeDeclFullMyType}, {exprDeclFull.MyType})");
                if (typeDeclFullMyType == MyType.CompoundType &&
                    !CheckCompoundType(typeDeclFull, exprDeclFull.CompoundType))
                {
                    throw new Exception(
                        $"Type of declared variable doesn't match with expression type ({typeDeclFullMyType}, {exprDeclFull.MyType})");
                }

                idDeclFull.MyType = exprDeclFull.MyType;
                idDeclFull.Value = exprDeclFull;
                if (exprDeclFull.MyType == MyType.CompoundType)
                {
                    idDeclFull.CompoundType = typeDeclFull;
                }

                idDeclFull.IsInitialized = true;

                ScopeStack.AddVariable(idDeclFull);
                return idDeclFull;

            case NodeTag.ArrayType:
                var exprArrType = UniversalVisit(node.Children[0]!);
                var typeArrType = UniversalVisit(node.Children[1]!);

                if (exprArrType.MyType != MyType.Integer)
                    throw new Exception($"Cannot make array with non integral size, given type {exprArrType.MyType}");

                return new SymbolicNode(MyType.CompoundType, compoundType: typeArrType,
                    arraySize: exprArrType);
            case NodeTag.RecordType:
                var varRecType = UniversalVisit(node.Children[0]);
                return new SymbolicNode(MyType.CompoundType, structFields: GetStructFields(varRecType));
            case NodeTag.VariableDeclarations:
                if (node.Children.Length == 0)
                {
                    return new SymbolicNode(MyType.VariableDeclarations, new List<SymbolicNode>());
                }

                var varDeclarations = UniversalVisit(node.Children[0]);
                var varDeclsSingle = UniversalVisit(node.Children[1]);
                varDeclarations.Children.Add(varDeclsSingle);
                return varDeclarations;
            case NodeTag.RoutineCall:
                var idRoutineCall = UniversalVisit(node.Children[0]);
                var exprsRoutineCall = UniversalVisit(node.Children[1]);

                var routine = ScopeStack.FindVariable(idRoutineCall.Name!);

                var counter = 0;
                if (routine.FuncArguments!.Count != exprsRoutineCall.Children.Count)
                {
                    throw new Exception("Unexpected number of arguments");
                }

                foreach (var element in routine.FuncArguments!)
                {
                    if (!CheckRoutinesArgument(element, exprsRoutineCall.Children[counter]))
                    {
                        throw new Exception($"Unexpected argument type ({element.Key}) in function");
                    }

                    counter++;
                }

                // TODO - not sure
                idRoutineCall.MyType = MyType.Function;
                idRoutineCall.Children.Add(exprsRoutineCall);
                return idRoutineCall;

            case NodeTag.ExpressionsContinuous:
                var exprsCont = UniversalVisit(node.Children[0]);
                var exprCont = UniversalVisit(node.Children[1]);

                if (exprsCont.Children.Count == 0)
                {
                    return new SymbolicNode(MyType.Expressions, new List<SymbolicNode> { exprsCont, exprCont });
                }

                exprsCont.Children.Add(exprCont);
                return exprsCont;

            case NodeTag.Assignment:
                var modPrimaryAssignment = UniversalVisit(node.Children[0]!);
                var exprAssignment = UniversalVisit(node.Children[1]!);
                var modPrimary = ScopeStack.FindVariable(modPrimaryAssignment.Name!);
                
                switch (modPrimary.MyType, exprAssignment.MyType)
                {
                    case (MyType.Integer, MyType.Integer):
                    case (MyType.Integer, MyType.Real):
                    case (MyType.Integer, MyType.Boolean):
                    case (MyType.Real, MyType.Real):
                    case (MyType.Real, MyType.Boolean):
                    case (MyType.Boolean, MyType.Boolean):
                    case (MyType.Boolean, MyType.Integer): // No checks during compile time
                        break;
                    case (MyType.CompoundType, MyType.CompoundType):
                        if (!CheckCompoundType(modPrimary.CompoundType, exprAssignment.CompoundType))
                        {
                            throw new Exception(
                                $"Unexpected type in assignment statement ({modPrimary.MyType}, {exprAssignment.MyType})");
                        }

                        break;
                    default:
                        throw new Exception(
                            $"Unexpected type in assignment statement ({modPrimary.MyType}, {exprAssignment.MyType})");
                }

                modPrimaryAssignment.MyType = modPrimary.MyType;
                modPrimaryAssignment.CompoundType = modPrimary.CompoundType;
                modPrimaryAssignment.IsInitialized = true;
                modPrimaryAssignment.Value = exprAssignment;
                ScopeStack.AddVariable(modPrimaryAssignment);
                return modPrimaryAssignment;
            case NodeTag.Return:
                var returnValue = UniversalVisit(node.Children[0]);

                return new SymbolicNode(MyType.Return, value: returnValue);

            case NodeTag.BodyStatement or NodeTag.BodySimpleDeclaration:
                if (node.Children[0] == null)
                {
                    MyType myType = MyType.Undefined;
                    var singleBody = UniversalVisit(node.Children[1]!);
                    if (singleBody.MyType == MyType.Return)
                    {
                        if (!ScopeStack.HasRoutineScope()) throw new Exception("Return without routine scope");
                        myType = ((SymbolicNode)singleBody.Value!).MyType;
                    }

                    if (singleBody.MyType == MyType.Break)
                    {
                        if (!ScopeStack.HasLoopScope())
                        {
                            throw new Exception("Cannot break from the given context");
                        }
                    }

                    return new SymbolicNode(myType, new List<SymbolicNode> { singleBody });
                }

                var bodyCont = UniversalVisit(node.Children[0]!);
                var currentBody = UniversalVisit(node.Children[1]!);

                if (currentBody.MyType == MyType.Return)
                {
                    var returnExpr = (currentBody.Value! as SymbolicNode)!;
                    MyType newType = returnExpr.MyType;
                    if (bodyCont.MyType != MyType.Undefined && bodyCont.MyType != newType ||
                        (bodyCont.MyType == MyType.CompoundType &&
                         !bodyCont.CompoundType!.Equals(currentBody.CompoundType)))
                    {
                        throw new Exception($"Types of returns didn't match ({bodyCont.MyType}, {newType})");
                    }

                    if (bodyCont.MyType == MyType.Undefined)
                    {
                        bodyCont.MyType = newType;
                        if (newType == MyType.CompoundType) bodyCont.CompoundType = returnExpr.CompoundType;
                    }

                    if (!ScopeStack.HasRoutineScope()) throw new Exception("Return without routine scope");
                }

                if (currentBody.MyType == MyType.Break)
                {
                    if (!ScopeStack.HasLoopScope())
                    {
                        throw new Exception("Cannot break from the given context");
                    }
                }

                bodyCont.Children.Add(currentBody);
                return bodyCont;

            case NodeTag.IfStatement or NodeTag.IfElseStatement:
                ScopeStack.NewScope(Scope.ScopeContext.IfStatement);
                var exprIf = UniversalVisit(node.Children[0]!);
                var bodyIf = UniversalVisit(node.Children[1]!);
                SymbolicNode? bodyElse = null;
                if (node.Tag == NodeTag.IfElseStatement)
                {
                    bodyElse = UniversalVisit(node.Children[2]!);
                }

                if (exprIf.MyType != MyType.Boolean)
                    throw new Exception($"Cannot match type {exprIf.MyType} to 'boolean'");
                var symbolicNode =
                    new SymbolicNode(myType: MyType.Undefined, new List<SymbolicNode> { exprIf, bodyIf });
                if (bodyElse != null)
                {
                    symbolicNode.Children.Add(bodyElse);
                }

                ScopeStack.DeleteScope();
                return symbolicNode;

            case NodeTag.ForeachLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var idForEachLoop = UniversalVisit(node.Children[0]!);
                var fromForEachLoop = UniversalVisit(node.Children[1]!);

                if (fromForEachLoop.MyType != MyType.CompoundType)
                    throw new Exception($"Cannot iterate through {fromForEachLoop.MyType}");
                if (fromForEachLoop.CompoundType == null || fromForEachLoop.CompoundType.ArrayElements == null)
                    throw new Exception("Cannot iterate through non-array type");

                idForEachLoop.MyType = fromForEachLoop.CompoundType.MyType;
                if (fromForEachLoop.CompoundType.MyType == MyType.CompoundType)
                {
                    idForEachLoop.CompoundType = fromForEachLoop.CompoundType;
                }

                idForEachLoop.IsInitialized = true;

                ScopeStack.AddVariable(idForEachLoop);
                var bodyForEachLoop = UniversalVisit(node.Children[2]!);
                ScopeStack.DeleteScope();
                return new SymbolicNode(MyType.ForEachLoop,
                    new List<SymbolicNode> { idForEachLoop, fromForEachLoop, bodyForEachLoop });
            case NodeTag.ProgramSimpleDeclaration or NodeTag.ProgramRoutineDeclaration:
                if (node.Children[0] == null)
                {
                    return new SymbolicNode(MyType.Undefined,
                        new List<SymbolicNode> { UniversalVisit(node.Children[1]!) });
                }

                var progs = UniversalVisit(node.Children[0]!);
                var decl = UniversalVisit(node.Children[1]!);

                progs.Children.Add(decl);
                return progs;
            case NodeTag.Plus or NodeTag.Minus or NodeTag.Mul or NodeTag.Div or NodeTag.Rem or NodeTag.Eq or NodeTag.Ne
                or NodeTag.Le or
                NodeTag.Lt or NodeTag.Ge or NodeTag.Gt or NodeTag.And or NodeTag.Or or NodeTag.Xor:
                return _visitBinaryOperations(node);
            case NodeTag.NotInteger or NodeTag.SignToInteger or NodeTag.SignToDouble:
                return _visitUnaryOperations(node);
            case NodeTag.ModifiablePrimaryGettingSize or NodeTag.ModifiablePrimaryGettingField
                or NodeTag.ModifiablePrimaryGettingValueFromArray:
                return _visitModifiablePrimary(node);
            case NodeTag.ArrayConst:
                var exprs = UniversalVisit(node.Children[0]!);
                // check that all elements in the array has the same type
                var arrayElements = exprs.Children;
                if (arrayElements.Count == 0)
                {
                    throw new Exception("Creating const array with zero size");
                }

                var arrayElementType = arrayElements[0].MyType;
                var arrayElementCompoundType = arrayElements[0].CompoundType;
                foreach (var e in arrayElements)
                {
                    if (e.MyType != arrayElementType || (e.MyType == MyType.CompoundType &&
                                                         arrayElementCompoundType!.Equals(e.CompoundType)))
                        throw new Exception(
                            $"Expected variables in array to be in the same type ({arrayElementType}, {e.MyType})");
                }
                // a = [1, 2, 3 ]
                // a.Value = [1,2,3]
                // a.CompoundType = [int](5)
                // [1,2,3].CompoundType = [int](5)
                // b = [3, 4, 5]

                return new SymbolicNode(MyType.CompoundType,
                    compoundType: new SymbolicNode(arrayElementType, compoundType: arrayElementCompoundType,
                        arraySize: new SymbolicNode(MyType.Integer, value: arrayElements.Count)),
                    arrayElements: arrayElements
                );
            case NodeTag.RoutineDeclarationWithTypeAndParams or NodeTag.RoutineDeclarationWithType
                or NodeTag.RoutineDeclaration or NodeTag.RoutineDeclarationWithParams:
                ScopeStack.NewScope(Scope.ScopeContext.Routine);
                SymbolicNode idRoutineDecl = UniversalVisit(node.Children[0]!);
                SymbolicNode? parametersRoutineDecl = null;
                SymbolicNode? typeRoutineDecl = null;
                SymbolicNode bodyRoutineDecl;
                switch (node.Tag)
                {
                    case NodeTag.RoutineDeclarationWithTypeAndParams:
                        parametersRoutineDecl = UniversalVisit(node.Children[1]!);
                        typeRoutineDecl = UniversalVisit(node.Children[2]!);
                        bodyRoutineDecl = UniversalVisit(node.Children[3]!);
                        break;
                    case NodeTag.RoutineDeclarationWithType:
                        typeRoutineDecl = UniversalVisit(node.Children[1]!);
                        bodyRoutineDecl = UniversalVisit(node.Children[2]!);
                        break;
                    case NodeTag.RoutineDeclarationWithParams:
                        parametersRoutineDecl = UniversalVisit(node.Children[1]!);
                        bodyRoutineDecl = UniversalVisit(node.Children[2]!);
                        break;
                    default:
                        bodyRoutineDecl = UniversalVisit(node.Children[1]!);
                        break;
                }

                if (typeRoutineDecl != null)
                {
                    var typeRoutineDeclMyType = MyType.Undefined;
                    if (typeRoutineDecl.MyType == MyType.PrimitiveType)
                        typeRoutineDeclMyType = GetTypeFromPrimitiveType((typeRoutineDecl.Value as string)!);

                    if (bodyRoutineDecl.MyType != typeRoutineDeclMyType ||
                        (typeRoutineDecl.MyType == MyType.CompoundType &&
                         typeRoutineDecl.Equals(bodyRoutineDecl.CompoundType)))
                        throw new Exception(
                            $"Type of declared routine doesn't match with expression type ({typeRoutineDeclMyType}, {bodyRoutineDecl.MyType})");
                }

                idRoutineDecl.FuncArguments = new();
                if (parametersRoutineDecl != null)
                    foreach (SymbolicNode parameter in parametersRoutineDecl.Children)
                        idRoutineDecl.FuncArguments[parameter.Name!] = parameter;
                idRoutineDecl.MyType = bodyRoutineDecl.MyType;
                if (idRoutineDecl.MyType == MyType.CompoundType)
                {
                    idRoutineDecl.CompoundType = bodyRoutineDecl.CompoundType;
                }

                idRoutineDecl.Children.Add(bodyRoutineDecl);
                ScopeStack.DeleteScope();
                ScopeStack.AddVariable(idRoutineDecl);
                return idRoutineDecl;
            case NodeTag.ParametersContinuous:
                var parametersParametersContinuous = UniversalVisit(node.Children[0]!);
                var parameterDeclarationParametersContinuous = UniversalVisit(node.Children[1]!);
                parametersParametersContinuous.Children.AddRange(parameterDeclarationParametersContinuous.Children);
                return new SymbolicNode(myType: MyType.CompoundType, parametersParametersContinuous.Children);
            case NodeTag.ParameterDeclaration:
                var idParameterDeclaration = UniversalVisit(node.Children[0]!);
                var typeParameterDeclaration = UniversalVisit(node.Children[1]!);
                if (ScopeStack.GetLastScope().FindVariable(idParameterDeclaration.Name!) != null)
                {
                    throw new Exception($"Parameter {idParameterDeclaration.Name!} is already defined");
                }

                
                if (typeParameterDeclaration.MyType == MyType.CompoundType)
                {
                    idParameterDeclaration.CompoundType = typeParameterDeclaration;
                    idParameterDeclaration.MyType = MyType.CompoundType;
                }
                else
                {
                    idParameterDeclaration.MyType = GetTypeFromPrimitiveType(typeParameterDeclaration.Value! as string);
                }

                ScopeStack.AddVariable(idParameterDeclaration);
                return new SymbolicNode(myType: MyType.CompoundType, new List<SymbolicNode> { idParameterDeclaration });
            case NodeTag.WhileLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var expressionWhile = UniversalVisit(node.Children[0]!);
                var bodyWhile = UniversalVisit(node.Children[1]!);
                if (expressionWhile.MyType != MyType.Boolean)
                    throw new Exception("While loop condition type must be boolean!");
                ScopeStack.DeleteScope();
                return new SymbolicNode(myType: MyType.Undefined,
                    new List<SymbolicNode> { expressionWhile, bodyWhile });
            case NodeTag.ForLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var iterIdForLoop = UniversalVisit(node.Children[0]!);
                var rangeForLoop = UniversalVisit(node.Children[1]!);
                if (rangeForLoop.MyType is not (MyType.Range or MyType.ReverseRange))
                    throw new Exception($"Type of range {rangeForLoop.MyType} doesn't match with any range type");
                iterIdForLoop.MyType = MyType.Integer;
                iterIdForLoop.IsInitialized = true;
                ScopeStack.AddVariable(iterIdForLoop);
                var bodyForLoop = UniversalVisit(node.Children[2]!);
                ScopeStack.DeleteScope();
                return new SymbolicNode(myType: MyType.ForLoop,
                    new List<SymbolicNode> { iterIdForLoop, rangeForLoop, bodyForLoop });
            case NodeTag.RangeReverse or NodeTag.Range:
                var fromRange = UniversalVisit(node.Children[0]!);
                var toRange = UniversalVisit(node.Children[1]!);
                if (fromRange.MyType != MyType.Integer || toRange.MyType != MyType.Integer)
                    throw new Exception(
                        $"Boundaries of range have incorrect type (From: {fromRange.MyType} To: {toRange.MyType}");
                return new SymbolicNode(myType: node.Tag == NodeTag.Range ? MyType.Range : MyType.ReverseRange,
                    new List<SymbolicNode> { fromRange, toRange });
        }

        throw new Exception($"Unexpected node tag {node.Tag}");
    }

    private Dictionary<string, SymbolicNode> GetStructFields(SymbolicNode node)
    {
        if (node.MyType != MyType.VariableDeclarations) throw new Exception($"Unexpected Type {node.MyType}");
        var toReturn = new Dictionary<string, SymbolicNode>();
        foreach (var field in node.Children)
        {
            toReturn.Add(field.Name!, field);
        }

        return toReturn;
    }

    private static Dictionary<MyType, HashSet<NodeTag>> _createAllowedOperations()
    {
        // TODO - check if all filled correctly
        HashSet<NodeTag> numbersSet = new()
        {
            NodeTag.Plus, NodeTag.Minus, NodeTag.Mul, NodeTag.Div, NodeTag.Rem, NodeTag.Eq, NodeTag.Ne, NodeTag.Le,
            NodeTag.Lt, NodeTag.Ge, NodeTag.Gt, NodeTag.SignToInteger
        };
        HashSet<NodeTag> integersSet = new(numbersSet);
        integersSet.Add(NodeTag.SignToInteger);
        integersSet.Add(NodeTag.NotInteger);
        HashSet<NodeTag> realsSet = new(numbersSet);
        realsSet.Add(NodeTag.SignToDouble);
        HashSet<NodeTag> boolsSet = new()
        {
            NodeTag.And, NodeTag.Or, NodeTag.Xor,
        };
        Dictionary<MyType, HashSet<NodeTag>> allowedOperations = new()
        {
            { MyType.Integer, integersSet },
            { MyType.Real, realsSet },
            { MyType.Boolean, boolsSet },
        };
        return allowedOperations;
    }

    private Dictionary<MyType, HashSet<NodeTag>> _allowedOperations = _createAllowedOperations();

    private void _checkOperationAllowance(MyType operandsType, NodeTag operationType)
    {
        if (_allowedOperations.TryGetValue(operandsType, out var set) && set.Contains(operationType))
        {
            // everything is ok
        }
        else
        {
            throw new Exception($"Operation {operationType} can't be performed on operands with type {operandsType}");
        }
    }

    private void _checkDivision(SymbolicNode dividerOperand)
    {
        
        // TODO 
        if (dividerOperand.Value != null)
        {
            var value = Convert.ToDouble(dividerOperand.Value);
            if (value == 0)
            {
                throw new Exception("Error: Division by zero");
            }
        }
    }

    private SymbolicNode _visitBinaryOperations(ComplexNode node)
    {
        var operand1 = UniversalVisit(node.Children[0]);
        var operand2 = UniversalVisit(node.Children[1]);
        if (operand1.MyType != operand2.MyType || operand1.MyType == MyType.CompoundType &&
            !operand1.CompoundType!.Equals(operand2.CompoundType))
        {
            throw new Exception(
                $"Operation performed on operands with different types: {operand1.MyType}, {operand2.MyType}");
        }

        var operandsType = operand1.MyType;
        _checkOperationAllowance(operandsType, node.Tag);

        // TODO - add simplifying tree if both operands are compile-time constants
        // switch (node.NodeTag)...
        switch (node.Tag)
        {
            case NodeTag.Div or NodeTag.Rem:
                if (operand2.Value is int or double)
                {
                    _checkDivision(operand2);
                }

                break;
        }

        return new SymbolicNode(operandsType, new List<SymbolicNode> { operand1, operand2 });
    }

    private SymbolicNode _visitUnaryOperations(ComplexNode node)
    {
        SymbolicNode number;
        MyType operationType;
        List<SymbolicNode> children = new();
        switch (node.Tag)
        {
            case NodeTag.NotInteger:
                number = UniversalVisit(node.Children[0]!);
                if (number.MyType != MyType.Integer)
                {
                    throw new Exception($"Unsupported operation 'not' for {number.MyType}");
                }

                if ((int)number.Value! != 0 && (int)number.Value! != 1)
                    throw new Exception($"Unsupported operation 'not' for integer value {(int)number.Value!}");
                operationType = MyType.Boolean;
                children.Add(number);
                break;
            case NodeTag.SignToInteger or NodeTag.SignToDouble:
                var sign = UniversalVisit(node.Children[0]!);
                number = UniversalVisit(node.Children[1]!);
                children.Add(sign);
                children.Add(number);
                // TODO - unary operations are now only supported for number literals

                if (number.MyType is not (MyType.Real or MyType.Integer))
                    throw new Exception($"Unsupported unary operation on {number.MyType}");

                operationType = number.MyType;
                break;
            default:
                throw new Exception($"Trying to process unary operation. Actual type: {node.Tag}");
        }

        _checkOperationAllowance(number.MyType, node.Tag);
        return new SymbolicNode(operationType, children: children);
    }

    private SymbolicNode _visitModifiablePrimary(ComplexNode node)
    {
        var modifiablePrimary = UniversalVisit(node.Children[0]!);
        var arg2 = UniversalVisit(node.Children[1]!);
        switch (node.Tag)
        {
            case NodeTag.ModifiablePrimaryGettingSize:
                if (modifiablePrimary.MyType != MyType.CompoundType || modifiablePrimary.ArrayElements == null)
                {
                    throw new Exception(
                        $"Error: Trying to get size not from array, but from {modifiablePrimary.MyType}");
                }

                return new SymbolicNode(MyType.Integer, new List<SymbolicNode> { modifiablePrimary, arg2 });


            case NodeTag.ModifiablePrimaryGettingField:
                if (modifiablePrimary.MyType != MyType.CompoundType ||
                    modifiablePrimary.CompoundType!.StructFields == null)
                {
                    throw new Exception("Trying to get a field not from a struct");
                }

                var fieldName = arg2.Name ??
                                throw new Exception("Something wrong with struct field name. It's not in the tree");

                if (!modifiablePrimary.StructFields!.ContainsKey(fieldName))
                {
                    throw new Exception(
                        $"Struct {modifiablePrimary.CompoundType!.Name} doesn't have a field called {arg2.Name}");
                }

                return new SymbolicNode(arg2.MyType, new List<SymbolicNode> { modifiablePrimary, arg2 });


            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                if (modifiablePrimary.MyType != MyType.CompoundType ||
                    modifiablePrimary.CompoundType!.ArrayElements == null)
                {
                    throw new Exception(
                        $"Error: Trying to get an array element not from array, but from {modifiablePrimary.MyType}");
                }

                // // TODO - check out of range if index is a compile time constant - Cannot Check
                // if (modifiablePrimary.ArrayElements != null) ;
                // SymbolicNode arrayLen = (modifiablePrimary.Value! as SymbolicNode)!;

                var arrayElement = modifiablePrimary.ArrayElements![0];
                if (arg2.MyType != MyType.Integer)
                {
                    throw new Exception($"Array index type is not integer, but {arg2.MyType}");
                }

                return new SymbolicNode(arrayElement.MyType, new List<SymbolicNode> { modifiablePrimary, arg2 },
                    structFields: arrayElement.StructFields, arrayElements: arrayElement.ArrayElements,
                    value: arrayElement.Value, compoundType: arrayElement.CompoundType,
                    arraySize: arrayElement.ArraySize, isInitialized: modifiablePrimary.IsInitialized);
            default:
                throw new Exception($"Trying to visit {node.Tag} as ModifiablePrimary");
        }
    }

    private bool CheckCompoundType(SymbolicNode? compoundTypeA, SymbolicNode? compoundTypeB)
    {
        if (compoundTypeA == null && compoundTypeB == null) return true;
        if (compoundTypeA == null) return false;

        return compoundTypeA.Equals(compoundTypeB);
    }

    private bool CheckRoutinesArgument(KeyValuePair<string, SymbolicNode> map, SymbolicNode expr)
    {
        if (expr.MyType == MyType.CompoundType)
        {
            return CheckCompoundType(expr, map.Value);
        }

        return map.Value.MyType == expr.MyType;
    }

    private MyType GetTypeFromPrimitiveType(string value)
    {
        return value switch
        {
            "integer" => MyType.Integer,
            "real" => MyType.Real,
            "boolean" => MyType.Boolean,
            _ => throw new Exception("Unexpected Primitive Type"),
        };
    }

    public SymbolicNode UniversalVisit(Node node)
    {
        return node switch
        {
            ComplexNode complexNode => Visit(complexNode),
            LeafNode<string> leafNode => VisitLeaf(leafNode),
            LeafNode<double> leafNode => VisitLeaf(leafNode),
            LeafNode<int> leafNode => VisitLeaf(leafNode),
            LeafNode<bool> leafNode => VisitLeaf(leafNode),
            _ => throw new Exception($"Cannot find out Node type")
        };
    }

    public SymbolicNode VisitLeaf<T>(LeafNode<T> node)
    {
        MyType myType;
        switch (node)
        {
            case LeafNode<int>:
                myType = MyType.Integer;
                break;
            case LeafNode<double>:
                myType = MyType.Real;
                break;
            case LeafNode<bool>:
                myType = MyType.Boolean;
                break;
            case LeafNode<string> stringNode:
                switch (node.Tag)
                {
                    case NodeTag.Identifier:
                        if (ScopeStack.HasVariable(stringNode.Value))
                        {
                            return ScopeStack.FindVariable(stringNode.Value);
                        }
                        return new SymbolicNode(myType: MyType.Undefined, name: stringNode.Value, isInitialized: false);
                    case NodeTag.PrimitiveType:
                        return new SymbolicNode(myType: MyType.PrimitiveType, value: stringNode.Value);
                    case NodeTag.Unary:
                        return new SymbolicNode(myType: MyType.Unary, value: stringNode.Value);
                    default:
                        throw new Exception($"Invalid node tag {node.Tag} for LeafNode<string>");
                }
            default:
                throw new Exception("Error in creating a leaf symbolic node");
        }

        return new SymbolicNode(myType: myType, value: node.Value);
    }
}