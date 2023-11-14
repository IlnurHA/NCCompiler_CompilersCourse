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
                return new GetFieldNode((StructVarNode) modPrimField, (VarNode) idField)
                    .GetValueNode(); // return VarNode
            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                var arrFromArr = node.Children[0]!.Accept(this);
                var indexFromArr = node.Children[1]!.Accept(this);
                return new GetByIndexNode((ArrayVarNode) arrFromArr, (ValueNode) indexFromArr)
                    .GetValueNode(); // return VarNode
            case NodeTag.ArrayGetSorted:
                var arrGetSorted = node.Children[0]!.Accept(this);
                if (arrGetSorted.GetType() != typeof(ArrayVarNode))
                    throw new Exception($"Should have got 'ArrayVarNode', got '{arrGetSorted}' instead");
                return new SortedArrayNode((ArrayVarNode) arrGetSorted).GetValueNode(); // TODO return VarNode
            case NodeTag.ArrayGetSize:
                var arrGetSize = node.Children[0]!.Accept(this);
                return new ArraySizeNode((ArrayVarNode) arrGetSize).GetValueNode(); // TODO return VarNode
            case NodeTag.ArrayGetReversed:
                var arrGetReversed = node.Children[0]!.Accept(this);
                return new ReversedArrayNode((ArrayVarNode) arrGetReversed).GetValueNode(); // TODO return VarNode
        }

        throw new Exception("Unimplemented");
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
                    return new ParametersNode(new List<ParameterNode> {(ParameterNode) node.Children[0]!.Accept(this)});
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
                    return new RoutineCallNode(function, new ExpressionsNode()).GetValueNode();
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

                // TODO check exprs types compared with routine parameters
                return new RoutineCallNode(function, exprsRoutineCall).GetValueNode();
            default:
                throw new Exception($"Unexpected Name Tag: {node.Tag}");
        }
    }

    // public SymbolicNode ExpressionVisit(ComplexNode node)
    // {
    //     switch (node.Tag)
    //     {
    //         case NodeTag.And:
    //             var operand1 = node.Children[0]!.Accept(this);
    //             var operand2 = node.Children[0]!.Accept(this);
    //     }
    // }

    public SymbolicNode VisitLeaf<T>(LeafNode<T> node)
    {
        switch (node.Tag)
        {
            case NodeTag.IntegerLiteral:
                return new ConstNode(node.Value!, new TypeNode(MyType.Integer));
            case NodeTag.RealLiteral:
                return new ConstNode(node.Value!, new TypeNode(MyType.Real));
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