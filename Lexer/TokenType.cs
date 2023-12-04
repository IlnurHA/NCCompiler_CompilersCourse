namespace NCCompiler_CompilersCourse.Lexer;

public enum TokenType
{
    // Common Keywords
    End, LeftBracket, RightBracket, LeftSquaredBracket,
    RightSquaredBracket, Comma, Is, Colon, EOL,
    
    // Primitive Types
    Integer, Boolean, Real, Undefined,
    
    // User-defined types
    Array, Size, Reversed, Sorted, Record, Dot, Print,
    
    // Routines
    Routine, Return,
    
    // Declarations
    Var, Type,
    
    // Statements
    AssignmentOperator, While, Loop, For, In, Reverse,
    TwoDots, Foreach, From, If, Else, Then, Break,
    Assert,
    
    // Expressions
    And, Or, Xor, LeComparison,
    LtComparison, GeComparison,
    GtComparison, EqComparison,
    NeComparison, Multiply,
    Divide, Remainder, Plus,
    Minus, Not, UnaryPlus, UnaryMinus,
    
    // Values
    Identifier, IntegralLiteral,
    True, False, RealLiteral,
}