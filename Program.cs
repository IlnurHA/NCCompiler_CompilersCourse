using NCCompiler_CompilersCourse.Lexer;
using NCCompiler_CompilersCourse.Semantics;

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
        if (res)
        {
            var rootNode = parser.RootNode;
            EvalVisitor visitor = new EvalVisitor();
            var rootSymbolic = visitor.UniversalVisit(rootNode);
            Console.WriteLine(res);
        }
        Console.WriteLine(res);
    }
}