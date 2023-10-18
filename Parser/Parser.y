/*
 *  This specification is for a version of the RealCalc example.
 *  This version creates a abstract syntax tree from the input,
 *  and demonstrates how to use a reference type as the semantic
 *  value type.
 *
 *  The parser class is declared %partial, so that the bulk of
 *  the code can be placed in the separate file RealTreeHelper.cs
 *
 *  Process with > gppg /nolines RealTree.y
 */

%namespace NCCompiler_CompilersCourse.Parser
%partial 



/*
 * The accessibility of the Parser object must not exceed that
 * of the inherited ShiftReduceParser<,>. Thus if you want to include 
 * the *source* of ShiftReduceParser from ShiftReduceParserCode.cs, 
 * then you must either set the compilation flag EXPORT_GPPG or  
 * override the default, public visibility with %visibility internal.
 * If you reference the pre-compiled QUT.ShiftReduceParser.dll then 
 * ShiftReduceParser<> is public and either visibility will work.
 */

%visibility internal

%YYSTYPE Node

// Tokens without values
%token ROUTINE ARRAY INTEGER IS VAR SIZE FOR FROM LOOP IF THEN ELSE END RETURN REAL IN ASSERT WHILE TYPE RECORD TRUE FALSE BOOLEAN FOREACH REVERSE

// Tokens with values
%token IDENTIFIER NUMBER FLOAT

// Other tokens
%token EOL COLON DOT TWO_DOTS COMMA

%token ASSIGNMENT_OPERATOR

// Boolean operators
%token AND OR XOR EQ_COMPARISON GT_COMPARISON GE_COMPARISON LT_COMPARISON LE_COMPARISON NE_COMPARISON NOT

// Arithmetic operators
%left PLUS MINUS MULTIPLY DIVIDE REMAINDER

// Brackets
%token LEFT_BRACKET RIGHT_BRACKET LEFT_SQUARED_BRACKET RIGHT_SQUARED_BRACKET
 
%%
Program : /* empty */ | Program SimpleDeclaration   { $$ = Node.MakeBinary(NodeTag.ProgramSimpleDeclaration, $1, $2); }
    | Program RoutineDeclaration                    {Node.MakeBinary(NodeTag.ProgramRoutineDeclaration, $1, $2);}
    ;

SimpleDeclaration   : VariableDeclaration   { $$ = $1; }
                    | TypeDeclaration       { $$ = $1; }
                    ;

VariableDeclaration
    :
    VAR IDENTIFIER COLON Type IS Expression { $$ = Node.MakeTernary(NodeTag.VariableDeclarationFull, $2, $4, $6); }
    | VAR IDENTIFIER COLON Type             { $$ = Node.MakeBinary(NodeTag.VariableDeclarationIdenType, $2, $4); }
    | VAR IDENTIFIER IS Expression          { $$ = Node.MakeBinary(NodeTag.VariableDeclarationIdenExpr, $2, $4); }
    ;

TypeDeclaration
    : TYPE IDENTIFIER IS Type               { $$ = Node.MakeBinary(NodeTag.TypeDeclaration, $2, $4); }
    ;

RoutineDeclaration
    : ROUTINE IDENTIFIER LEFT_BRACKET Parameters RIGHT_BRACKET COLON Type IS Body END
    { $$ = Node.MakeQuaternary(NodeTag.RoutineDeclarationWithType, $2, $4, $7, $9); }
    
    | ROUTINE IDENTIFIER LEFT_BRACKET Parameters RIGHT_BRACKET IS Body END
    { $$ = Node.MakeTernary(NodeTag.RoutineDeclaration, $2, $4, $7); }
    ;
  
Parameters      : ParameterDeclaration                  { $$ = $1; }
                | Parameters COMMA ParameterDeclaration { $$ = Node.MakeBinary(NodeTag.ParametersContinuous, $1, $3); }
                ;

ParameterDeclaration : IDENTIFIER COLON Type      {$$ = Node.MakeBinary(NodeTag.ParameterDeclaration, $1, $3);}
    ;
  
Type : PrimitiveType    {$$ = $1;}
    | ArrayType         {$$ = $1;}
    | RecordType        {$$ = $1;}
    | IDENTIFIER
    ;
  
PrimitiveType : INTEGER | REAL | BOOLEAN
    ;
  
RecordType  : RECORD LEFT_BRACKET VariableDeclarations RIGHT_BRACKET END    { $$ = Node.MakeUnary(NodeTag.RecordType, $3); }
            | RECORD VariableDeclarations END                               { $$ = Node.MakeUnary(NodeTag.RecordType, $2); }
    ;
  
VariableDeclarations : /* empty */ | VariableDeclarations VariableDeclaration
    { $$ = Node.MakeBinary(NodeTag.VariableDeclarations, $1, $2)}
    ;

ArrayType : ARRAY LEFT_SQUARED_BRACKET Expression RIGHT_SQUARED_BRACKET Type    { $$ = Node.MakeBinary(NodeTag.ArrayType, $3,  $5); }
    ;

Body :  /* empty */
        | Body SimpleDeclaration    { $$ = Node.MakeBinary(NodeTag.BodySimpleDeclaration); }
        | Body Statement            { $$ = Node.MakeBinary(NodeTag.BodyStatement); }
        ;

Statement   : Assignment    { $$ = $1; }
            | RoutineCall   { $$ = $1; }
            | WhileLoop     { $$ = $1; }
            | ForLoop       { $$ = $1; }
            | ForeachLoop   { $$ = $1; }
            | IfStatement   { $$ = $1; }
            | Assert        { $$ = $1; }
            ;

Assignment  : ModifiablePrimary ASSIGNMENT_OPERATOR Expression   { $$ = Node.MakeBinary(NodeTag.Assignment, $1,  $3); }
            ;

RoutineCall     : IDENTIFIER LEFT_BRACKET Expressions RIGHT_BRACKET     { $$ = Node.MakeBinary(NodeTag.RoutineCall, $1, $3); }
                | IDENTIFIER
                ;

Expressions     : Expression                    { $$ = $1; }
                | Expressions COMMA Expression  { $$ = Node.MakeBinary(NodeTag.ExpressionsContinuous, $1, $3); }
                ;

WhileLoop   : WHILE Expression LOOP Body END    { $$ = Node.MakeBinary(NodeTag.WhileLoop, $2, $4); }
            ;

ForLoop : FOR IDENTIFIER Range LOOP Body END    { $$ = Node.MakeTernary(NodeTag.ForLoop, $2, $3, $5); }
        ;

Range :
    IN REVERSE Expression TWO_DOTS Expression   { $$ = Node.MakeTernary(NodeTag.RangeReverse, $3, $5); }
    | IN Expression TWO_DOTS Expression         { $$ = Node.MakeTernary(NodeTag.Range, $2, $4); }
    ;

ForeachLoop : FOREACH IDENTIFIER FROM ModifiablePrimary LOOP Body END
            { $$ = Node.MakeTernary(NodeTag.ForeachLoop, $2, $4, $6); }
            ;

IfStatement     : IF Expression THEN Body ELSE Body END { $$ = Node.MakeTernary(NodeTag.IfElseStatement, $2, $4, $6); }
                | IF Expression THEN Body END           { $$ = Node.MakeBinary(NodeTag.IfStatement, $2, $4); }
                ;

Expression : /* empty */  
    | Relation {$$ = $1;}
    | Expression AND Relation {$$ = Node.MakeBinary(NodeTag.And, $1, $3);}
    | Expression OR Relation {$$ = Node.MakeBinary(NodeTag.Or, $1, $3);}
    | Expression XOR Relation {$$ = Node.MakeBinary(NodeTag.Xor, $1, $3);}
    | Cast
    ;

Relation   : Simple {$$ = $1;}
    | Relation EQ_COMPARISON Simple {$$ = Node.MakeBinary(NodeTag.Eq, $1, $3);}
    | Relation GT_COMPARISON Simple {$$ = Node.MakeBinary(NodeTag.Gt, $1, $3);}
    | Relation GE_COMPARISON Simple {$$ = Node.MakeBinary(NodeTag.Ge, $1, $3);}
    | Relation LT_COMPARISON Simple {$$ = Node.MakeBinary(NodeTag.Lt, $1, $3);}
    | Relation LE_COMPARISON Simple {$$ = Node.MakeBinary(NodeTag.Le, $1, $3);}
    | Relation NE_COMPARISON Simple {$$ = Node.MakeBinary(NodeTag.Ne, $1, $3);}
    ;

Simple     : Factor {$$ = $1;}
    | Simple MULTIPLY Factor {$$ = Node.MakeBinary(NodeTag.Mul, $1, $3);}
    | Simple DIVIDE Factor {$$ = Node.MakeBinary(NodeTag.Div, $1, $3);}
    | Simple REMAINDER Factor {$$ = Node.MakeBinary(NodeTag.Rem, $1, $3);}
    ;

Factor     : Summand {$$ = $1;}
    | Factor PLUS Summand {$$ = Node.MakeBinary(NodeTag.Plus, $1, $3);}
    | Factor MINUS Summand {$$ = Node.MakeBinary(NodeTag.Minus, $1, $3);}
    ;

Summand : Primary {$$ = $1;}
    | LEFT_BRACKET Expression RIGHT_BRACKET {$$ = $2;}
    ;

Primary   : Sign INTEGER {$$ = Node.MakeBinary(NodeTag.SignToInteger, $1, $2);}
    | NOT INTEGER {$$ = Node.MakeUnary(NodeTag.NotInteger, $2);}
    | INTEGER
    | Sign REAL {$$ = Node.MakeBinary(NodeTag.SignToDouble, $1, $2);}
    | REAL
    | TRUE | FALSE
    | ModifiablePrimary {$$ = $1;}
    | LEFT_SQUARED_BRACKET Expressions RIGHT_SQUARED_BRACKET {$$ = MakeUnary(NodeTag.ArrayConst, $1, $2);}
    ;

Sign : PLUS
    | MINUS
    ;

Cast : Type LEFT_BRACKET Expression RIGHT_BRACKET {$$ = Node.MakeBinary(NodeTag.Cast, $1, $3); }
    ;

ModifiablePrimary   : ModifiablePrimaryWithoutSize {$$ = Node.MakeUnary(NodeTag.ModifiablePrimary, $1); }
        | ModifiablePrimaryWithoutSize DOT SIZE {$$ = Node.MakeBinary(NodeTag.ModifiablePrimary, $1, $3); }
        ;

ModifiablePrimaryWithoutSize   : IDENTIFIER
        | ModifiablePrimaryWithoutSize DOT IDENTIFIER {$$ = Node.MakeBinary(NodeTag.ModifiablePrimaryWithoutSize, $1, $3); }
        | ModifiablePrimaryWithoutSize LEFT_SQUARED_BRACKET Expression RIGHT_SQUARED_BRACKET {$$ = Node.MakeBinary(NodeTag.ModifiablePrimaryWithoutSize, $1, $3); }
        ;
          
Assert : ASSERT Expression COMMA Expression {$$ = Node.MakeBinary(NodeTag.Assert, $2, $4); }
    ;
%%

