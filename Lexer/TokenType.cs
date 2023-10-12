namespace NCCompiler_CompilersCourse.Lexer;

public enum TokenType
{
    // Tokens without values
    Routine,
    Array,
    Integer,
    Is, Var,
    Size, For,
    From, Loop,
    If, Then,
    Else, End,
    Return, Real,
    In, Assert,
    While, Type,
    Record, True, False, Boolean,

    // Tokens with values
    Identifier, Number, Float,
    
    // Other tokens
    EndOfLine,
    Colon, Dot, TwoDots,
    Comma,
    
    // Comments
    MultilineCommentStart,
    MultilineCommentEnd,
    SinglelineComment,
    
    AssignmentOperator,
    
    // Boolean operators
    And, Or, Xor,
    EqComparison,
    GtComparison,
    GeComparison,
    LtComparisom,
    LeComparison,
    NeComparison,
    
    // Arithmetic operators
    Plus,
    Minus,
    Multiply,
    Divide,
    Remainder,
    
    // Brackets
    LeftBracket,
    RightBracket,
    LeftSquaredBracket,
    RightSquaredBracket,
}