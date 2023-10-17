namespace NCCompiler_CompilersCourse;

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
    Sorted, Foreach, Reversed,

    // Tokens with values
    Identifier, Number, Float,
    
    // Other tokens
    EndOfLine,
    Colon, Dot, TwoDots,
    Comma,
 
    AssignmentOperator,
    
    // Boolean operators
    And, Or, Xor,
    EqComparison,
    GtComparison,
    GeComparison,
    LtComparison,
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