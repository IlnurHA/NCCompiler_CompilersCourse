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
        Lexer.Scanner scanner = new Lexer.Scanner(new Lexer.Lexer(contents));
        Parser.Parser parser = new Parser.Parser(scanner);
        var res = parser.Parse();
        Console.WriteLine(res);
    }
}