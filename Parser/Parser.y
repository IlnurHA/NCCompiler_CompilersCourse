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

%namespace Parser
%output=Parser.cs 
%partial 
%sharetokens
%start list

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

%YYSTYPE RealTree.Node

// Tokens without values
%token ROUTINE ARRAY INTEGER IS VAR SIZE FOR FROM LOOP IF THEN ELSE END RETURN REAL IN ASSERT WHILE TYPE RECORD TRUE FALSE BOOLEAN

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
   Program : /* empty */ | SimpleDeclaration | RoutineDeclaration
       ;
 
   SimpleDeclaration : VariableDeclaration | TypeDeclaration
       ;
 
   VariableDeclaration
       : VAR Identifier COLON Type [ IS Expression ]
       | VAR Identifier   IS Expression
       ;
 
   TypeDeclaration
       : TYPE Identifier IS Type
       ;
 
   RoutineDeclaration
       : ROUTINE Indentifier LEFT_BRACKET Parameters RIGHT_BRACKET [ COLON Type ] IS
           BODY
         END
       ;
   
   Parameters   : ParameterDeclaration { COMMA ParameterDeclaration }
       ;
 
   ParameterDeclaration : Identifier COLON Identifier
       ;
   
   Type : PrimitiveType | ArrayType | RecordType | Identifier
       ;
 
%%

