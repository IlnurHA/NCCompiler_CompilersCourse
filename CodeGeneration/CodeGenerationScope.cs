namespace NCCompiler_CompilersCourse.CodeGeneration;

public class CodeGenerationScope
{
    private Dictionary<string, (int, int)> Variables { get; set; } = new();
}