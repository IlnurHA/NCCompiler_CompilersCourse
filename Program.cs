using NCCompiler_CompilersCourse.Lexer;

namespace NCCompiler_CompilersCourse;

class Program
{
    public static void Main(string[] args)
    {
        string? fileName = Console.ReadLine();
        // while ((fileName = Console.ReadLine()) == null) { }
        // Console.WriteLine(fileName);
        string contents = File.ReadAllText(fileName);
        // Console.WriteLine(contents);
        Lexer.Lexer lexer = new Lexer.Lexer(contents);
        List<Token> tokens = lexer.GetTokens();
        foreach (Token token in tokens)
        {
            Console.WriteLine($"Type: {token.Type}, Lexeme: \"{token.Lexeme}\", Value: {token.Value}, Span - Line: {token.Span.LineNum}, Range: {token.Span.PosBegin}:{token.Span.PosEnd}");
        }
        
        // List<Token> tokens = lexer.GetTokens();
        // foreach (Token token in tokens)
        // {
        // Console.WriteLine($"Type: {token.Type}, Lexeme: \"{token.Lexeme}\", Value: {token.Value}");
        // }
    }
}