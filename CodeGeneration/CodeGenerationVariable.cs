using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationVariable
{
    private string _name;
    private int _id;
    private string _type;

    private string nodeToType(TypeNode typeNode)
    {
        return typeNode switch
        {
            ArrayTypeNode arrayTypeNode => "",
            _ => ""
        };
    }
    
    public CodeGenerationVariable(string name, int id, TypeNode typeNode)
    {
        this._name = name;
        this._id = id;
        this._type = nodeToType(typeNode);
    }
}