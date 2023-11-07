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
                var exprs = UniversalVisit(node.Children[0]);
                // check that all elements in the array has the same type
                var arrayElements = exprs.Children;
                if (arrayElements.Count == 0)
                {
                    return new SymbolicNode(MyType.Array, arrayElements: arrayElements);
                }

                var arrayElementType = arrayElements[0].MyType;
                var arrayElementCompoundType = arrayElements[0].CompoundType;
                var arrayElementArrayElementType = arrayElements[0].ArrayElements
                foreach (var e in arrayElements)
                {
                    
                }
        }
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
        if (dividerOperand.Value != null && (double)dividerOperand.Value == 0)
        {
            throw new Exception("Error: Division by zero");
        }
    }

    private SymbolicNode _visitBinaryOperations(ComplexNode node)
    {
        var operand1 = UniversalVisit(node.Children[0]);
        var operand2 = UniversalVisit(node.Children[1]);
        if (operand1.MyType != operand2.MyType)
        {
            throw new Exception(
                $"Operation performed on operands with different types: {operand1.MyType}, {operand2.MyType}");
        }

        var operandsType = operand1.MyType;
        _checkOperationAllowance(operandsType, node.NodeTag);

        // TODO - add simplifying tree if both operands are compile-time constants
        // switch (node.NodeTag)...
        switch (node.NodeTag)
        {
            case NodeTag.Div or NodeTag.Rem:
                _checkDivision(operand2);
                break;
        }

        return new SymbolicNode(operandsType, new List<SymbolicNode> { operand1, operand2 });
    }

    private SymbolicNode _visitUnaryOperations(ComplexNode node)
    {
        SymbolicNode number;
        MyType operationType;
        List<SymbolicNode> children = new();
        switch (node.NodeTag)
        {
            case NodeTag.NotInteger:
                number = UniversalVisit(node.Children[0]);
                // TODO - check not 5
                operationType = MyType.Boolean;
                children.Add(number);
                break;
            case NodeTag.SignToInteger or NodeTag.SignToDouble:
                var sign = UniversalVisit(node.Children[0]);
                number = UniversalVisit(node.Children[1]);
                children.Add(sign);
                children.Add(number);
                // TODO - unary operations are now only supported for number literals
                operationType = number.MyType;
                break;
            default:
                throw new Exception($"Trying to process unary operation. Actual type: {node.NodeTag}");
        }

        _checkOperationAllowance(number.MyType, node.NodeTag);
        return new SymbolicNode(operationType, children: children);
    }

    private SymbolicNode _visitModifiablePrimary(ComplexNode node)
    {
        var modifiablePrimary = UniversalVisit(node.Children[0]);
        var arg2 = UniversalVisit(node.Children[1]);
        switch (node.NodeTag)
        {
            case NodeTag.ModifiablePrimaryGettingSize:
                if (modifiablePrimary.MyType != MyType.Array)
                {
                    throw new Exception(
                        $"Error: Trying to get size not from array, but from {modifiablePrimary.MyType}");
                }

                // TODO - does identifier that is array has type MyType.Array?
                // TODO - what if struct has a field called size
                return new SymbolicNode(MyType.Integer, new List<SymbolicNode> { modifiablePrimary, arg2 });


            case NodeTag.ModifiablePrimaryGettingField:
                if (modifiablePrimary.MyType != MyType.CompoundType)
                {
                    throw new Exception("Trying to get a field not from a struct");
                }

                var fieldName = arg2.Name ??
                                throw new Exception("Something wrong with struct field name. It's not in the tree");

                if (!(modifiablePrimary.StructFields?.ContainsKey(fieldName) ?? false))
                {
                    throw new Exception(
                        $"Struct {modifiablePrimary.CompoundType!.Name} doesn't have a field called {arg2.Name}");
                }

                return new SymbolicNode(arg2.MyType, new List<SymbolicNode> { modifiablePrimary, arg2 });


            case NodeTag.ModifiablePrimaryGettingValueFromArray:
                if (modifiablePrimary.MyType != MyType.CompoundType || modifiablePrimary.CompoundType?.ArrayElements == null)
                {
                    throw new Exception(
                        $"Error: Trying to get an array element not from array, but from {modifiablePrimary.MyType}");
                }

                // TODO - check out of range if index is a compile time constant
                var arrayLen = modifiablePrimary.ArrayElements!.Count;
                if (arrayLen < 1)
                {
                    throw new Exception("Index out of range: array length is 0");
                }

                var arrayElement = modifiablePrimary.ArrayElements![0];
                if (arg2.MyType != MyType.Integer)
                {
                    throw new Exception($"Array index type is not integer, but {arg2.MyType}");
                }

                return new SymbolicNode(arrayElement.MyType, new List<SymbolicNode> { modifiablePrimary, arg2 },
                    structFields: arrayElement.StructFields, arrayElements: arrayElement.ArrayElements,
                    value: arrayElement.Value, compoundType: arrayElement.CompoundType);
            default:
                throw new Exception($"Trying to visit {node.NodeTag} as ModifiablePrimary");
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