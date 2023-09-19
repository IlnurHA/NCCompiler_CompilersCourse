namespace NCCompiler_CompilersCourse;

public enum TokenType
{
    // Tokens without values
    routine, array, integer, is,
    var, size, for, from, loop,
    if, then, else, end, return, real,
    in, assert, while, and, or, xor, type,
    record, true, false, boolean,

    // Tokens with values
    identifier, number, float,
    
    // Other tokens
    end_of_line,
    colon, dot, two_dots,
    comma,
    
    // Comments
    multiline_comment_start,
    multiline_comment_end,
    singleline_comment,
    
    assignment_operator,
    
    // Boolean operators
    eq_comparison,
    gr_comparison,
    greq_comparison,
    l_comparisom,
    leq_comparison,
    neq_comparison,
    
    // Arithmetic operators
    plus,
    minus,
    division,
    remainder,
    
    // Brackets
    left_bracket,
    right_bracket,
    left_squared_bracket,
    right_squared_bracket,
}