namespace NCCompiler_CompilersCourse;

class Program
{
    public static void Main(string[] args)
    {
        // /home/kamil/RiderProjects/NCCompiler_CompilersCourse/tests/test1.ncc
        string? fileName = Console.ReadLine();
        // while ((fileName = Console.ReadLine()) == null) { }
        // Console.WriteLine(fileName);
        string contents = File.ReadAllText(fileName);
        // Console.WriteLine(contents);
        Lexer lexer = new Lexer(contents);
        List<Token> tokens = lexer.Tokenize();
        foreach (Token token in tokens)
        {
            Console.WriteLine($"Type: {token.Type}, Lexeme: \"{token.Lexeme}\", Value: {token.Value}");
        }
    }
}