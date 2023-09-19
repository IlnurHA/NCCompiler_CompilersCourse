namespace NCCompiler_CompilersCourse;

public class Span
{
    public long LineNum;

    public int PosBegin, PosEnd;
    
    public Span(long lineNum, int posBegin, int posEnd)
    {
        LineNum = lineNum;
        PosBegin = posBegin;
        PosEnd = posEnd;
    }
}