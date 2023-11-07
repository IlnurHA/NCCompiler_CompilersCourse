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

                // If type node is Primitive Type [!TODO for any type]
                if (typeNode.MyType != MyType.PrimitiveType) throw new Exception("Unexpected type");
                if (identifierNode.MyType != MyType.Undefined) throw new Exception("Unexpected type");

                identifierNode.MyType = GetTypeFromPrimitiveType((typeNode.Value as string)!);

                ScopeStack.AddVariable(identifierNode);
                return identifierNode;
            case NodeTag.VariableDeclarationIdenExpr:
                var identifier = UniversalVisit(node.Children[0]);
                var expr = UniversalVisit(node.Children[1]);

                if (identifier.MyType != MyType.Undefined) throw new Exception("Unexpected type");

                identifier.MyType = expr.MyType;

                ScopeStack.AddVariable(identifier);
                return new SymbolicNode(myType: MyType.Undefined, new List<SymbolicNode> { identifier, expr });
            case NodeTag.VariableDeclarationFull:
                var idDeclFull = UniversalVisit(node.Children[0]);
                var typeDeclFull = UniversalVisit(node.Children[1]);
                var exprDeclFull = UniversalVisit(node.Children[2]);

                if (idDeclFull.MyType != MyType.Undefined) throw new Exception("Unexpected type");

                // For primitive type [!TODO]
                var typeDeclFullMyType = GetTypeFromPrimitiveType((typeDeclFull.Value as string)!);
                if (exprDeclFull.MyType != typeDeclFullMyType)
                    throw new Exception(
                        $"Type of declared variable doesn't match with expression type ({typeDeclFullMyType}, {exprDeclFull.MyType})");

                idDeclFull.MyType = exprDeclFull.MyType;
                idDeclFull.Children.Add(exprDeclFull);
                // TODO
                ScopeStack.AddVariable(idDeclFull);
                return idDeclFull;
            case NodeTag.ArrayType:
                var exprArrType = UniversalVisit(node.Children[0]);
                var typeArrType = UniversalVisit(node.Children[1]);

                if (exprArrType.MyType != MyType.Integer)
                    throw new Exception($"Cannot make array with non integral size");

                return new SymbolicNode(MyType.CompoundType, compoundType: typeArrType,
                    children: new List<SymbolicNode> { exprArrType });
            case NodeTag.RoutineDeclarationWithTypeAndParams or NodeTag.RoutineDeclarationWithType
                or NodeTag.RoutineDeclaration or NodeTag.RoutineDeclarationWithParams:
                ScopeStack.NewScope();
                SymbolicNode idRoutineDecl = UniversalVisit(node.Children[0]);
                SymbolicNode? parametersRoutineDecl = null;
                SymbolicNode? typeRoutineDecl = null;
                SymbolicNode bodyRoutineDecl;
                switch (node.NodeTag)
                {
                    case NodeTag.RoutineDeclarationWithTypeAndParams:
                        parametersRoutineDecl = UniversalVisit(node.Children[1]);
                        typeRoutineDecl = UniversalVisit(node.Children[2]);
                        bodyRoutineDecl = UniversalVisit(node.Children[3]);
                        break;
                    case NodeTag.RoutineDeclarationWithType:
                        typeRoutineDecl = UniversalVisit(node.Children[1]);
                        bodyRoutineDecl = UniversalVisit(node.Children[2]);
                        break;
                    case NodeTag.RoutineDeclarationWithParams:
                        parametersRoutineDecl = UniversalVisit(node.Children[1]);
                        bodyRoutineDecl = UniversalVisit(node.Children[2]);
                        break;
                    default:
                        bodyRoutineDecl = UniversalVisit(node.Children[1]);
                        break;
                }

                // For primitive type [!TODO]
                if (typeRoutineDecl != null)
                {
                    var typeRoutineDeclMyType = GetTypeFromPrimitiveType((typeRoutineDecl.Value as string)!);
                    if (bodyRoutineDecl.MyType != typeRoutineDeclMyType)
                        throw new Exception(
                            $"Type of declared routine doesn't match with expression type ({typeRoutineDeclMyType}, {bodyRoutineDecl.MyType})");
                }

                idRoutineDecl.FuncArguments = new();
                if (parametersRoutineDecl != null)
                    foreach (SymbolicNode parameter in parametersRoutineDecl.Children)
                        idRoutineDecl.FuncArguments[parameter.Name!] = parameter;
                idRoutineDecl.MyType = bodyRoutineDecl.MyType;
                idRoutineDecl.Children.Add(bodyRoutineDecl);
                ScopeStack.DeleteScope();
                ScopeStack.AddVariable(idRoutineDecl);
                return idRoutineDecl;
            case NodeTag.ParametersContinuous:
                var parametersParametersContinuous = UniversalVisit(node.Children[0]);
                var parameterDeclarationParametersContinuous = UniversalVisit(node.Children[1]);
                parametersParametersContinuous.Children.AddRange(parameterDeclarationParametersContinuous.Children);
                return new SymbolicNode(myType: MyType.CompoundType, parametersParametersContinuous.Children);
            case NodeTag.ParameterDeclaration:
                var idParameterDeclaration = UniversalVisit(node.Children[0]);
                var typeParameterDeclaration = UniversalVisit(node.Children[1]);
                // TODO: Add structure and array support
                ScopeStack.AddVariable(idParameterDeclaration);
                return new SymbolicNode(myType: MyType.CompoundType, new List<SymbolicNode> { idParameterDeclaration });
            case NodeTag.WhileLoop:
                ScopeStack.NewScope();
                var expressionWhile = UniversalVisit(node.Children[0]);
                var bodyWhile = UniversalVisit(node.Children[1]);
                if (expressionWhile.MyType != MyType.Boolean)
                    throw new Exception("While loop condition type must be boolean!");
                ScopeStack.DeleteScope();
                return new SymbolicNode(myType: MyType.Undefined,
                    new List<SymbolicNode> { expressionWhile, bodyWhile });
            case NodeTag.ForLoop:
                ScopeStack.NewScope();
                var iterIdForLoop = UniversalVisit(node.Children[0]);
                var rangeForLoop = UniversalVisit(node.Children[1]);
                if (rangeForLoop.MyType is not (MyType.Range or MyType.ReverseRange))
                    throw new Exception($"Type of range {rangeForLoop.MyType} doesn't match with any range type");
                iterIdForLoop.MyType = MyType.Integer;
                ScopeStack.AddVariable(iterIdForLoop);
                var bodyForLoop = UniversalVisit(node.Children[2]);
                ScopeStack.DeleteScope();
                return new SymbolicNode(myType: MyType.ForLoop,
                    new List<SymbolicNode> { iterIdForLoop, rangeForLoop, bodyForLoop });
            case NodeTag.RangeReverse or NodeTag.Range:
                var fromRange = UniversalVisit(node.Children[0]);
                var toRange = UniversalVisit(node.Children[1]);
                if (fromRange.MyType != MyType.Integer || toRange.MyType != MyType.Integer)
                    throw new Exception(
                        $"Boundaries of range have incorrect type (From: {fromRange.MyType} To: {toRange.MyType}");
                return new SymbolicNode(myType: node.Tag == NodeTag.Range ? MyType.Range : MyType.ReverseRange,
                    new List<SymbolicNode> { fromRange, toRange });
        }
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