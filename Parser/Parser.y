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
Program : /* empty */ | Program SimpleDeclaration {$$ = $2;} | Program RoutineDeclaration {$$ = $2;}
    ;

SimpleDeclaration : VariableDeclaration {$$ = $1;} | TypeDeclaration {$$ = $1;}
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

