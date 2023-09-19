namespace NCCompiler_CompilersCourse;

public enum TokenType
{
    // Tokens without values
    Plus, Minus, Multiply, Divide,
    LeftParen, RightParen,
    
    // Tokens with values
    Identifier, Number,
    
    // Other tokens
    EOF
}