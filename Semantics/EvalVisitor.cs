using System.Linq.Expressions;
using NCCompiler_CompilersCourse.Parser;

namespace NCCompiler_CompilersCourse.Semantics;

class EvalVisitor : IVisitor
{
    private Dictionary<string, dynamic> _registers = new();

    public void Visit(ComplexNode node)
    {
        throw new NotImplementedException();
        // достать детей
        // выполнить операцию с детьми (action) (вызовется visit детей, который вызовет action детей)
        // выполнить свою опрерацию
    }

    public T VisitLeaf<T>(LeafNode<T> node)
    {
        switch (node.Tag)
        {
            case NodeTag.IntegerLiteral:
            case NodeTag.RealLiteral:
            case NodeTag.BooleanLiteral:
            case NodeTag.Identifier:
            case NodeTag.Unary:
            case NodeTag.PrimitiveType:
                return node.Value;
            default:
                throw new Exception($"Incorrect tag:{node.Tag}");
        }
    }

    public void Action(Node node)
    {
        switch (node)
        {
            case LeafNode<int>:
            case LeafNode<bool>:
            case LeafNode<string>:
            case LeafNode<double>:
                // TODO - print
                break;
            case ComplexNode complexNode:
                Action(complexNode);
                break;
        }
    }


    private Node Action(ComplexNode node)
    {
        switch (node.Tag)
        {
            // c.m.a.s.d.f.g.g = 5+7
            case NodeTag.Unary:
                break;
            case NodeTag.SignToInteger:
            case NodeTag.SignToDouble:
                LeafNode<string> sign = node.Children[0] as LeafNode<string>;
                // var type123 = node.Children[1].GetType().GetGenericArguments()[0];
                // LeafNode<type123> result = 
                if (node.Children[1] is LeafNode<int>)
                {
                    LeafNode<int> intResult = node.Children[1] as LeafNode<int>;
                    switch (sign!.Value)
                    {
                        case "-":
                            return new LeafNode<int>(node.Tag, -intResult!.Value);
                        case "+":
                            return new LeafNode<int>(node.Tag, intResult!.Value);
                    }
                }
                else
                {
                    LeafNode<double> doubleResult = node.Children[1] as LeafNode<double>;
                    switch (sign!.Value)
                    {
                        case "-":
                            return new LeafNode<double>(node.Tag, -doubleResult!.Value);
                        case "+":
                            return new LeafNode<double>(node.Tag, doubleResult!.Value);
                    }
                }

                break;
            case NodeTag.Plus:
            case NodeTag.Assignment:
                var variable = node.Children[0];
                switch (variable.Tag)
                {
                    case NodeTag.Identifier:
                }

                var expression = node.Children[1];
                variable.Tag == NodeTag.Identifier
        }
    }

    private Node binaryOperation(ComplexNode node)
    {
        var operand1 = node.Children[0];
        var operand2 = node.Children[1];
        if (operand1.MyType != operand2.MyType)
        {
            throw new Exception("Invalid type");
        }
        // пока без ассайнмента
        if (operand1.MyType != MyType.Boolean && node.Tag is NodeTag.And or NodeTag.Or or NodeTag.Xor)
        {
            throw new Exception($"Operation {node.Tag} can be performed only on booleans");
        }

        if (node.Tag is NodeTag.Ge or NodeTag.Gt or NodeTag.Le or NodeTag.Lt && operand1.MyType == MyType.Boolean)
        {
            throw new Exception($"Operation {node.Tag} can't be performed on booleans");
        }

        if (node.Tag is NodeTag.Div or NodeTag.Rem)
        {
            var type;
            var val2 = operand2 is LeafNode<int> leafNode2
                ? leafNode2.Value
                : (operand2 as LeafNode<double>)!.Value;
            if (val2 == 0)
            {
                throw new Exception("Can't divide by zero");
            }
            var val1 = operand1 is LeafNode<int> leafNode
                ? leafNode.Value
                : (operand1 as LeafNode<double>)!.Value;
            switch (node.Tag)
            {
                case NodeTag.Plus:
                    
            }
        }
        // TODO - упростить
        return node;
    }
}