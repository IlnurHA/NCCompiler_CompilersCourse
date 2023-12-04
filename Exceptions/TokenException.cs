using NCCompiler_CompilersCourse.Lexer;

namespace NCCompiler_CompilersCourse.Exceptions;

public class TokenException : LexerException
{
    public TokenException(string? message, Span? span) : base(message, span)
    {
    }
}