using NCCompiler_CompilersCourse.CodeGeneration;
using NCCompiler_CompilersCourse.Lexer;
using NCCompiler_CompilersCourse.Semantics;

namespace NCCompiler_CompilersCourse;

class Program
{
    public static void Main(string[] args)
    {
        while (true)
        {
            string? fileName = Console.ReadLine();
            if (fileName == "stop") break;
            // while ((fileName = Console.ReadLine()) == null) { }
            // Console.WriteLine(fileName);
            string contents = File.ReadAllText(fileName!);
            // Console.WriteLine(contents);
            Lexer.Lexer lexer = new Lexer.Lexer(contents);
        
            // foreach (var token in lexer.GetTokens())
            // {
            //     Console.WriteLine(
            //         $"Token: {token.Type}, Lexeme: {token.Lexeme}, Value: {token.Value}, Span: {token.Span}"
            //     );
            // }

            Lexer.Scanner scanner = new Lexer.Scanner(lexer);
            Parser.Parser parser = new Parser.Parser(scanner);
            var res = parser.Parse();
            Console.WriteLine($"Syntax analysis: {res}");
            
            // if (res)
            // {
            //     var rootNode = parser.RootNode;
            //     EvalVisitor visitor = new EvalVisitor();
            //     var rootSymbolic = rootNode.Accept(visitor);
            //     Console.WriteLine(res);
            //
            //     var codeGenVisit = new TranslationVisitorCodeGeneration();
            //     rootSymbolic.Accept(codeGenVisit, new Queue<BaseCommand>());
            //     var programStr = codeGenVisit.ResultingProgram;
            //     File.WriteAllText("compiledProgram.il", programStr);
            //     Console.WriteLine("Code generated");
            // }
        }
    }
}