namespace NCCompiler_CompilersCourse.Lexer;

public class Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public double? Value { get; } // For constants
    public Span Span { get; }

    public Token(TokenType type, string lexeme, Span span, double? value = null)
    {
        Type = type;
        Lexeme = lexeme;
        Value = value;
        Span = span;
    }
    
    public override string ToString()
    {
        return $"Type: {Type}, Lexeme: \"{Lexeme}\", Value: {Value}";
    }
}