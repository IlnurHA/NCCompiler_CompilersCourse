using System.Linq.Expressions;
using System.Numerics;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    public ScopeStack ScopeStack { get; set; } = new();

    public SymbolicNode Visit(ComplexNode node)
    {
        switch (node.NodeTag)
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

                ScopeStack.AddVariable(identifierNode);
                return identifierNode;
            case NodeTag.VariableDeclarationIdenExpr:
                var identifier = UniversalVisit(node.Children[0]);
                var expr = UniversalVisit(node.Children[1]);

                if (identifier.MyType != MyType.Undefined)
                    throw new Exception($"Unexpected type ({identifier.MyType})");

                identifier.MyType = expr.MyType;
                identifier.Value = expr;

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
                    !CheckCompoundType(typeDeclFull.CompoundType, exprDeclFull.CompoundType))
                {
                    throw new Exception(
                        $"Type of declared variable doesn't match with expression type ({typeDeclFullMyType}, {exprDeclFull.MyType})");
                }

                idDeclFull.MyType = exprDeclFull.MyType;
                idDeclFull.Value = exprDeclFull;
                if (exprDeclFull.MyType == MyType.CompoundType)
                {
                    idDeclFull.CompoundType = typeDeclFull.CompoundType;
                }

                ScopeStack.AddVariable(idDeclFull);
                return idDeclFull;

            case NodeTag.ArrayType:
                var exprArrType = UniversalVisit(node.Children[0]);
                var typeArrType = UniversalVisit(node.Children[1]);

                if (exprArrType.MyType != MyType.Integer)
                    throw new Exception($"Cannot make array with non integral size, given type {exprArrType.MyType}");

                return new SymbolicNode(MyType.CompoundType, compoundType: typeArrType,
                    value: exprArrType);
            case NodeTag.RecordType:
                var varRecType = UniversalVisit(node.Children[0]);
                return new SymbolicNode(MyType.CompoundType, compoundType: varRecType);
            case NodeTag.VariableDeclarations:
                if (node.Children.Length == 0)
                {
                    return new SymbolicNode(MyType.CompoundType, new List<SymbolicNode>());
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
                foreach (var element in routine.FuncArguments!)
                {
                    if (!CheckRoutinesArgument(element, exprsRoutineCall.Children[counter]))
                    {
                        throw new Exception($"Unexpected argument type ({element.Key}) in function");
                    }

                    counter++;
                }

                idRoutineCall.MyType = MyType.Function;
                idRoutineCall.Children.Add(exprsRoutineCall);
                return idRoutineCall;

            case NodeTag.ExpressionsContinuous:
                var exprsCont = UniversalVisit(node.Children[0]);
                var exprCont = UniversalVisit(node.Children[1]);

                if (exprsCont.Children.Count == 0)
                {
                    return new SymbolicNode(MyType.Expressions, new List<SymbolicNode> {exprsCont, exprCont});
                }

                exprsCont.Children.Add(exprCont);
                return exprsCont;

            case NodeTag.Assignment:
                var modPrimaryAssignment = UniversalVisit(node.Children[0]);
                var exprAssignment = UniversalVisit(node.Children[1]);

                switch (modPrimaryAssignment.MyType, exprAssignment.MyType)
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
                        if (!modPrimaryAssignment.CompoundType!.Equals(exprAssignment.CompoundType!))
                        {
                            throw new Exception(
                                $"Unexpected type in assignment statement ({modPrimaryAssignment.MyType}, {exprAssignment.MyType})");
                        }

                        break;
                    default:
                        throw new Exception(
                            $"Unexpected type in assignment statement ({modPrimaryAssignment.MyType}, {exprAssignment.MyType})");
                }

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
                    var singleBody = UniversalVisit(node.Children[1]);
                    if (singleBody.MyType == MyType.Return)
                    {
                        if (!ScopeStack.HasRoutineScope()) throw new Exception("Return without routine scope");
                        myType = ((SymbolicNode) singleBody.Value!).MyType;
                    }

                    if (singleBody.MyType == MyType.Break)
                    {
                        if (ScopeStack.GetLastScope().ScopeContextVar is not Scope.ScopeContext.Loop)
                        {
                            throw new Exception("Cannot break from the given context");
                        }
                    }

                    return new SymbolicNode(myType, new List<SymbolicNode> {singleBody});
                }
                
                var bodyCont = UniversalVisit(node.Children[0]);
                var currentBody = UniversalVisit(node.Children[1]);

                if (currentBody.MyType == MyType.Return)
                {
                    MyType newType = (currentBody.Value! as SymbolicNode)!.MyType;
                    if (bodyCont.MyType != MyType.Undefined && bodyCont.MyType != newType)
                    {
                        throw new Exception($"Types of returns didn't match ({bodyCont.MyType}, {newType})");
                    }

                    if (bodyCont.MyType == MyType.Undefined) bodyCont.MyType = newType;
                    if (!ScopeStack.HasRoutineScope()) throw new Exception("Return without routine scope");
                }
                if (currentBody.MyType == MyType.Break)
                {
                    if (ScopeStack.GetLastScope().ScopeContextVar is not (Scope.ScopeContext.Loop or Scope.ScopeContext.IfStatement))
                    {
                        throw new Exception("Cannot break from the given context");
                    }
                }

                bodyCont.Children.Add(currentBody);
                return bodyCont;
            
            case NodeTag.IfStatement or NodeTag.IfElseStatement:
                ScopeStack.NewScope(Scope.ScopeContext.IfStatement);
                var exprIf = UniversalVisit(node.Children[0]);
                var bodyIf = UniversalVisit(node.Children[1]);
                SymbolicNode? bodyElse = null;
                if (node.NodeTag == NodeTag.IfElseStatement)
                {
                    bodyElse = UniversalVisit(node.Children[2]);
                }

                if (exprIf.MyType != MyType.Boolean) throw new Exception($"Cannot match type {exprIf.MyType} to 'boolean'");
                var symbolicNode = new SymbolicNode(myType: MyType.Undefined, new List<SymbolicNode> {exprIf, bodyIf});
                if (bodyElse != null)
                {
                    symbolicNode.Children.Add(bodyElse);
                }
                ScopeStack.DeleteScope();
                return symbolicNode;
            
            case NodeTag.ForeachLoop:
                ScopeStack.NewScope(Scope.ScopeContext.Loop);
                var idForEachLoop = UniversalVisit(node.Children[0]);
                var fromForEachLoop = UniversalVisit(node.Children[1]);

                if (fromForEachLoop.MyType != MyType.CompoundType) throw new Exception($"Cannot iterate through {fromForEachLoop.MyType}");
                if (fromForEachLoop.CompoundType == null || fromForEachLoop.CompoundType.MyType != MyType.Array)
                    throw new Exception("Cannot iterate through non-array type");

                idForEachLoop.MyType = fromForEachLoop.CompoundType.MyType;
                if (fromForEachLoop.CompoundType.MyType == MyType.CompoundType)
                {
                    idForEachLoop.CompoundType = fromForEachLoop.CompoundType.CompoundType;
                }
                ScopeStack.AddVariable(idForEachLoop);
                var bodyForEachLoop = UniversalVisit(node.Children[2]);
                ScopeStack.DeleteScope();
                return new SymbolicNode(MyType.ForEachLoop,
                    new List<SymbolicNode> {idForEachLoop, fromForEachLoop, bodyForEachLoop});
            case NodeTag.ProgramSimpleDeclaration or NodeTag.ProgramRoutineDeclaration:
                if (node.Children[0] == null)
                {
                    return new SymbolicNode(MyType.Undefined, new List<SymbolicNode>{UniversalVisit(node.Children[1])});
                }

                var progs = UniversalVisit(node.Children[0]);
                var decl = UniversalVisit(node.Children[1]);
                
                progs.Children.Add(decl);
                return progs;
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
        if (map.Value.MyType != expr.MyType) return false;
        if (map.Value.MyType == MyType.Structure) return map.Value.Name == expr.Value as string;
        return true;
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

    private SymbolicNode UniversalVisit(Node node)
    {
        return node switch
        {
            ComplexNode complexNode => Visit(complexNode),
            LeafNode<dynamic> leafNode => VisitLeaf(leafNode),
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
                return node.Tag switch
                {
                    NodeTag.Identifier => new SymbolicNode(myType: MyType.Undefined, name: stringNode.Value),
                    NodeTag.PrimitiveType => new SymbolicNode(myType: MyType.PrimitiveType, value: stringNode.Value),
                    NodeTag.Unary => new SymbolicNode(myType: MyType.Unary, value: stringNode.Value),
                    _ => throw new Exception($"Invalid node tag {node.Tag} for LeafNode<string>")
                };
            default:
                throw new Exception("Error in creating a leaf symbolic node");
        }

        return new SymbolicNode(myType: myType, value: node.Value);
    }
}