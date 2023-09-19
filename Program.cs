namespace NCCompiler_CompilersCourse;

class Program
{
    public static void Main(string[] args)
    {
        string fullPathToTests =
            Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "tests");
        var allTests = Directory.EnumerateFiles(fullPathToTests, "*.ncc");
        foreach (var filename in allTests)
        {
            string contents = File.ReadAllText(filename);
            Lexer lexer = new Lexer(contents);
            List<Token> tokens = lexer.Tokenize();
            foreach (Token token in tokens)
            {
                Console.WriteLine($"Type: {token.Type}, Lexeme: \"{token.Lexeme}\", Value: {token.Value}");
            }
        }
    }
}