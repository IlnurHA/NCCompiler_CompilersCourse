namespace NCCompiler_CompilersCourse;

public enum TokenType
{
    // Tokens without values
    routine_token,
    array_token,
    integer_token,
    is_token, var_token,
    size_token, for_token,
    from_token, loop_token,
    if_token, then_token,
    else_token, end_token,
    return_token, real_token,
    in_token, assert_token,
    while_token, type_token,
    record_token, true_token, false_token, boolean_token,

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
    and, or, xor,
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