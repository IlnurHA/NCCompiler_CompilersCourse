namespace NCCompiler_CompilersCourse;

class Program
{
    public static void Main(string[] args)
    {
        string inputExpression = "x + 42 * (y - 3.14)";
        Lexer lexer = new Lexer(inputExpression);
        List<Token> tokens = lexer.Tokenize();

        foreach (Token token in tokens)
        {
            Console.WriteLine($"Type: {token.Type}, Lexeme: \"{token.Lexeme}\", Value: {token.Value}");
        }
    }
}