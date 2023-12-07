using QUT.Gppg;

namespace NCCompiler_CompilersCourse.Exceptions;

public class SemanticException : Exception
{
    /// <summary>
    ///  The span of an error
    /// </summary>
    /// 
    public LexLocation? LexLocation { get; }

    public SemanticException(string? message = null, LexLocation? lexLocation = null) : base(message)
    {
        LexLocation = lexLocation;
    }
}