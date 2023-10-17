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
Program : /* empty */ | Program SimpleDeclaration | Program RoutineDeclaration
    ;

SimpleDeclaration : VariableDeclaration | TypeDeclaration
    ;

VariableDeclaration
    :
    VAR IDENTIFIER COLON Type IS Expression
    | VAR IDENTIFIER COLON Type
    | VAR IDENTIFIER IS Expression
    ;

TypeDeclaration
    : TYPE IDENTIFIER IS Type
    ;

RoutineDeclaration
    : ROUTINE IDENTIFIER LEFT_BRACKET Parameters RIGHT_BRACKET COLON Type IS Body END
    | ROUTINE IDENTIFIER LEFT_BRACKET Parameters RIGHT_BRACKET IS Body END
    ;
  
Parameters   : ParameterDeclaration | Parameters COMMA ParameterDeclaration
    ;

ParameterDeclaration : IDENTIFIER COLON IDENTIFIER
    ;
  
Type : PrimitiveType | ArrayType | RecordType | IDENTIFIER
    ;
  
PrimitiveType : INTEGER | REAL | BOOLEAN
    ;
  
RecordType :   RECORD LEFT_BRACKET VariableDeclarations RIGHT_BRACKET END
    | RECORD VariableDeclarations END
    ;
  
VariableDeclarations : /* empty */ | VariableDeclarations VariableDeclaration
    ;

ArrayType : ARRAY LEFT_SQUARED_BRACKET Expression RIGHT_SQUARED_BRACKET Type
    ;


Body :  /* empty */ | Body SimpleDeclaration | Body Statement
    ;

Statement : Assignment | RoutineCall | WhileLoop | ForLoop | ForeachLoop | IfStatement | Assert
    ;

Assignment : ModifiablePrimary ASSIGNMENT_OPERATOR Expression
    ;

RoutineCall :   IDENTIFIER LEFT_BRACKET Expressions RIGHT_BRACKET
    | IDENTIFIER
    ;

Expressions : Expression | Expressions COMMA Expression
    ;

WhileLoop : WHILE Expression LOOP Body END
    ;

ForLoop : FOR IDENTIFIER Range LOOP Body END
    ;

Range :   IN REVERSE Expression TWO_DOTS Expression
    | IN Expression TWO_DOTS Expression
    ;

ForeachLoop : FOREACH IDENTIFIER FROM ModifiablePrimary LOOP Body END
    ;

IfStatement :   IF Expression THEN Body ELSE Body END
    | IF Expression THEN Body END
    ;

Expression :   Relation
    | Expression AND Relation
    | Expression OR Relation
    | Expression XOR Relation
    | Cast
    ;

Relation   : Simple
    | Relation EQ_COMPARISON Simple
    | Relation GT_COMPARISON Simple
    | Relation GE_COMPARISON Simple
    | Relation LT_COMPARISON Simple
    | Relation LE_COMPARISON Simple
    | Relation NE_COMPARISON Simple
    ;

Simple     : Factor
    | Simple MULTIPLY Factor
    | Simple DIVIDE Factor
    | Simple REMAINDER Factor
    ;

Factor     : Summand
    | Factor PLUS Summand
    | Factor MINUS Summand
    ;

Summand : Primary | LEFT_BRACKET Expression RIGHT_BRACKET
    ;

Primary   : Sign INTEGER
    | NOT INTEGER
    | INTEGER
    | Sign REAL
    | REAL
    | TRUE | FALSE
    | ModifiablePrimary
    | LEFT_BRACKET Expressions RIGHT_BRACKET
    ;

Sign : PLUS | MINUS
    ;

Cast : Type LEFT_BRACKET Expression RIGHT_BRACKET
    ;

ModifiablePrimary   : ModifiablePrimaryWithoutSize
        | ModifiablePrimaryWithoutSize DOT SIZE
        ;

ModifiablePrimaryWithoutSize   : IDENTIFIER
        | ModifiablePrimaryWithoutSize DOT IDENTIFIER
        | ModifiablePrimaryWithoutSize LEFT_BRACKET Expression RIGHT_BRACKET
        ;
          
Assert : ASSERT Expression COMMA Expression
    ;
%%

