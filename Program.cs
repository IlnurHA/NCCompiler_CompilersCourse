namespace NCCompiler_CompilersCourse;

class Program
{
    public static void Main(string[] args)
    {
        
        string? fileName;
        while ((fileName = Console.ReadLine()) == null) { }
        string contents = File.ReadAllText(fileName);
        Lexer lexer = new Lexer(contents);
        List<Token> tokens = lexer.Tokenize();
        foreach (Token token in tokens)
        {
            Console.WriteLine($"Type: {token.Type}, Lexeme: \"{token.Lexeme}\", Value: {token.Value}");
        }
    }
}