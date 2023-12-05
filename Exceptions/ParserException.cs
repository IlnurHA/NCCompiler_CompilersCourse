using QUT.Gppg;

namespace NCCompiler_CompilersCourse.Exceptions;

public class ParserException : Exception
{
    /// <summary>
    ///  The span of an error
    /// </summary>
    /// 
    public LexLocation? LexLocation { get; }

    public ParserException(string? message = null, LexLocation? lexLocation = null) : base(message)
    {
        LexLocation = lexLocation;
    }
}