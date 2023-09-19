namespace NCCompiler_CompilersCourse;

public class Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public double? Value { get; } // For constants

    public Token(TokenType type, string lexeme, double? value = null)
    {
        Type = type;
        Lexeme = lexeme;
        Value = value;
    }
}