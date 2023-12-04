using NCCompiler_CompilersCourse.Lexer;

namespace NCCompiler_CompilersCourse.Exceptions;

public class LexerException : Exception
{
    /// <summary>
    ///  The span of an error
    /// </summary>
    public Span? Span { get; }

    public LexerException(string? message = null, Span? span = null) : base(message)
    {
        Span = span;
    }
    
    


    public override string ToString()
    {
        return $"{base.Message} Span: {Span}";
    }
}