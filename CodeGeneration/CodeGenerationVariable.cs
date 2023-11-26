﻿using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationVariable
{
    private string _name;
    private int _id;
    private string _type;

    private static string MyTypeToType(MyType myType)
    {
        return myType switch
        {
            MyType.Integer => "int32",
            MyType.Real => "float32",
            MyType.Boolean => "bool",
            var type => throw new Exception($"Error. Incorrect operand type {type}.")
        };
    }
    private static string NodeToType(TypeNode typeNode)
    {
        return typeNode switch
        {
            StructTypeNode => "", // TODO: Implement struct representation
            ArrayTypeNode arrayTypeNode => $"{NodeToType(arrayTypeNode.ElementTypeNode)}[]",
            { } type => MyTypeToType(type.MyType),
        };
    }

    public string GetName()
    {
        return _name;
    }
    
    public CodeGenerationVariable(string name, int id, TypeNode typeNode)
    {
        _name = name;
        _id = id;
        _type = NodeToType(typeNode);
    }
}