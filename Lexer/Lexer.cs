using System.Globalization;
using System.Text.RegularExpressions;
using NCCompiler_CompilersCourse.Parser;
using QUT.Gppg;

namespace NCCompiler_CompilersCourse.Lexer;

class Lexer : AbstractScanner<Node, LexLocation>
{
    private readonly string _input;
    private int _currentPosition;
    private List<Token> _tokens = new();

    public Lexer(string input)
    {
        _input = input;
        yylloc = new LexLocation();
    }

    private static TokenType GetTokenType(string token)
    {
        return token switch
        {
            // "+" => TokenType.Plus,
            // "-" => TokenType.Minus,
            "*" => TokenType.Multiply,
            "/" => TokenType.Divide,
            "%" => TokenType.Remainder,
            "(" => TokenType.LeftBracket,
            ")" => TokenType.RightBracket,
            "[" => TokenType.LeftSquaredBracket,
            "]" => TokenType.RightSquaredBracket,
            "," => TokenType.Comma,
            "\n" => TokenType.EndOfLine,
            ":" => TokenType.Colon,
            "." => TokenType.Dot,
            ".." => TokenType.TwoDots,
            "and" => TokenType.And,
            "or" => TokenType.Or,
            "xor" => TokenType.Xor,
            "=" => TokenType.EqComparison,
            ">" => TokenType.GtComparison,
            ">=" => TokenType.GeComparison,
            "<" => TokenType.LtComparisom,
            "<=" => TokenType.LeComparison,
            "/=" => TokenType.NeComparison,
            "//" => TokenType.SinglelineComment,
            "/*" => TokenType.MultilineCommentStart,
            "*/" => TokenType.MultilineCommentEnd,
            ":=" => TokenType.AssignmentOperator,
            "routine" => TokenType.Routine,
            "array" => TokenType.Array,
            "integer" => TokenType.Integer,
            "is" => TokenType.Is,
            "var" => TokenType.Var,
            "size" => TokenType.Size,
            "for" => TokenType.For,
            "from" => TokenType.From,
            "loop" => TokenType.Loop,
            "if" => TokenType.If,
            "then" => TokenType.Then,
            "else" => TokenType.Else,
            "end" => TokenType.End,
            "return" => TokenType.Return,
            "real" => TokenType.Real,
            "in" => TokenType.In,
            "assert" => TokenType.Assert,
            "while" => TokenType.While,
            "type" => TokenType.Type,
            "record" => TokenType.Record,
            "true" => TokenType.True,
            "false" => TokenType.False,
            "boolean" => TokenType.Boolean,
            var someVal when new Regex(@"^[a-zA-Z][\w\d_]*$").IsMatch(someVal) =>
                TokenType.Identifier,
            var someVal when new Regex(@"^-?\d+$").IsMatch(someVal) =>
                TokenType.Number,
            var someVal when new Regex(@"^-?\d+\.?\d+$").IsMatch(someVal) =>
                TokenType.Float,
            _ => throw new NotImplementedException($"The {token} is not implemented")
        };
    }

    Tokens GppgTokensType(TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Plus => Tokens.PLUS,
            TokenType.Minus => Tokens.MINUS,
            TokenType.Multiply => Tokens.MULTIPLY,
            TokenType.Divide => Tokens.DIVIDE,
            TokenType.Remainder => Tokens.REMAINDER,
            TokenType.LeftBracket => Tokens.LEFT_BRACKET,
            TokenType.RightBracket => Tokens.RIGHT_BRACKET,
            TokenType.LeftSquaredBracket => Tokens.LEFT_SQUARED_BRACKET,
            TokenType.RightSquaredBracket => Tokens.RIGHT_SQUARED_BRACKET,
            TokenType.Comma => Tokens.COMMA,
            TokenType.EndOfLine => Tokens.EOL,
            TokenType.Colon => Tokens.COLON,
            TokenType.Dot => Tokens.DOT,
            TokenType.TwoDots => Tokens.TWO_DOTS,
            TokenType.And => Tokens.AND,
            TokenType.Or => Tokens.OR,
            TokenType.Xor => Tokens.XOR,
            TokenType.EqComparison => Tokens.EQ_COMPARISON,
            TokenType.GtComparison => Tokens.GT_COMPARISON,
            TokenType.GeComparison => Tokens.GE_COMPARISON,
            TokenType.LtComparisom => Tokens.LT_COMPARISON,
            TokenType.LeComparison => Tokens.LE_COMPARISON,
            TokenType.NeComparison => Tokens.NE_COMPARISON,
            // TokenType.SinglelineComment => Tokens.SINGL,
            // TokenType.MultilineCommentStart => Tokens.MultilineCommentStart,
            // TokenType.MultilineCommentEnd => Tokens.MultilineCommentEnd,
            TokenType.AssignmentOperator => Tokens.ASSIGNMENT_OPERATOR,
            TokenType.Routine => Tokens.ROUTINE,
            TokenType.Array => Tokens.ARRAY,
            TokenType.Integer => Tokens.INTEGER,
            TokenType.Is => Tokens.IS,
            TokenType.Var => Tokens.VAR,
            TokenType.Size => Tokens.SIZE,
            TokenType.For => Tokens.FOR,
            TokenType.From => Tokens.FROM,
            TokenType.Loop => Tokens.LOOP,
            TokenType.If => Tokens.IF,
            TokenType.Then => Tokens.THEN,
            TokenType.Else => Tokens.ELSE,
            TokenType.End => Tokens.END,
            TokenType.Return => Tokens.RETURN,
            TokenType.Real => Tokens.REAL,
            TokenType.In => Tokens.IN,
            TokenType.Assert => Tokens.ASSERT,
            TokenType.While => Tokens.WHILE,
            TokenType.Type => Tokens.TYPE,
            TokenType.Record => Tokens.RECORD,
            TokenType.True => Tokens.TRUE,
            TokenType.False => Tokens.FALSE,
            TokenType.Boolean => Tokens.BOOLEAN,
            TokenType.Identifier => Tokens.IDENTIFIER,
            TokenType.Number => Tokens.NUMBER,
            TokenType.Float => Tokens.FLOAT,
            _ => throw new Exception($"Unknown type {tokenType}")
        };
    }

    private int _lineCounter = 1;
    private int _lastEolIndex;

    private bool _inSingleLineComment;
    private bool _inMultiLineComment;

    public List<Token> GetTokens()
    {
        List<Token> tokens = new List<Token>();
        Token? token = NextToken();
        while (token != null)
        {
            tokens.Add(token);
            token = NextToken();
        }

        return tokens;
    }

    public Token? NextToken()
    {
        while (_currentPosition < _input.Length)
        {
            var currentChar = _input[_currentPosition];
            var lexemeLength = 1;

            if (_inSingleLineComment || _inMultiLineComment)
            {
                if (currentChar == '\n' && _inSingleLineComment)
                {
                    _inSingleLineComment = false;
                }
                else if (currentChar == '*' && _input[_currentPosition + 1] == '/' && _inMultiLineComment)
                {
                    _inMultiLineComment = false;
                }
                else
                {
                    if (currentChar == '\n')
                    {
                        _lineCounter++;
                        _lastEolIndex = _currentPosition;
                    }

                    _currentPosition++;
                    continue;
                }
            }

            if (char.IsWhiteSpace(currentChar) || currentChar == '\r')
            {
                _currentPosition++;
                continue;
            }

            if (char.IsDigit(currentChar))
            {
                // Then it is a digit

                var isDot = false;

                while (lexemeLength + _currentPosition < _input.Length &&
                       (char.IsDigit(_input[lexemeLength + _currentPosition]) ||
                        _input[lexemeLength + _currentPosition] == '.'))
                {
                    if (_input[lexemeLength + _currentPosition] == '.')
                    {
                        if (isDot) throw new Exception("Wrong float argument");

                        if (lexemeLength + _currentPosition + 1 < _input.Length &&
                            char.IsDigit(_input[_currentPosition + lexemeLength + 1]))
                        {
                            isDot = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    lexemeLength++;
                }
            }
            else if (char.IsLetter(currentChar))
            {
                // Then it is a identifier or keyword
                while (
                    lexemeLength + _currentPosition < _input.Length &&
                    (char.IsLetterOrDigit(_input[lexemeLength + _currentPosition]) ||
                     _input[lexemeLength + _currentPosition] == '_')
                ) lexemeLength++;
            }
            else
            {
                if (_currentPosition + 1 < _input.Length &&
                    ((currentChar == '/' && _input[_currentPosition + 1] == '=') ||
                     (currentChar == '*' && _input[_currentPosition + 1] == '/') ||
                     (currentChar == ':' && _input[_currentPosition + 1] == '=') ||
                     (currentChar == '>' && _input[_currentPosition + 1] == '=') ||
                     (currentChar == '<' && _input[_currentPosition + 1] == '=') ||
                     (currentChar == '.' && _input[_currentPosition + 1] == '.')))
                {
                    lexemeLength = 2;
                }
                else if ((_currentPosition + 1 < _input.Length) &&
                         (currentChar == '/' && _input[_currentPosition + 1] == '*'))
                {
                    lexemeLength = 2;
                    _inMultiLineComment = true;
                }
                else if (_currentPosition + 1 < _input.Length &&
                         (currentChar == '/' && _input[_currentPosition + 1] == '/'))
                {
                    lexemeLength = 2;
                    _inSingleLineComment = true;
                }
                else
                {
                    lexemeLength = 1;
                }
            }

            TokenType tokenType;
            var substring = _input.Substring(_currentPosition, lexemeLength);
            if (_tokens.Count != 0)
            {
                var prevToken = _tokens.Last();

                if (substring is "+" or "-")
                {
                    switch (prevToken.Type)
                    {
                        case TokenType.Identifier or TokenType.Number or TokenType.Float or TokenType.RightBracket
                            or TokenType.RightSquaredBracket:
                            tokenType = substring == "+" ? TokenType.Plus : TokenType.Minus;
                            break;
                        default:
                            tokenType = substring == "+" ? TokenType.UnaryPlus : TokenType.UnaryMinus;
                            break;
                    }
                }
                else
                {
                    tokenType = GetTokenType(substring);
                }
            }
            else
            {
                tokenType = GetTokenType(substring);
            }

            if (currentChar == '\n')
            {
                _lineCounter++;
                _lastEolIndex = _currentPosition;
            }

            _currentPosition += lexemeLength;


            var token = new Token(
                type: tokenType,
                lexeme: substring,
                span: new Span(
                    lineNum: _lineCounter,
                    posBegin: _currentPosition - _lastEolIndex,
                    posEnd: _currentPosition + lexemeLength - _lastEolIndex
                ),
                value: tokenType is TokenType.Number or TokenType.Float
                    ? double.Parse(substring, new CultureInfo("en-US").NumberFormat)
                    : null
            );
            _tokens.Add(token);
            return token;
        }

        return null; // End of file marker
    }


    public override int yylex()
    {
        try
        {
            var token = NextToken();
            if (token == null)
            {
                return (int)Tokens.EOF;
            }

            yylloc = new LexLocation((int)token.Span.LineNum, (int)token.Span.LineNum, token.Span.PosBegin,
                token.Span.PosEnd);
            switch (token.Type)
            {
                case TokenType.Identifier:
                    yylval = Node.MakeIdentifierLeaf(token.Lexeme);
                    break;
                case TokenType.UnaryPlus or TokenType.UnaryMinus:
                    yylval = Node.MakeUnaryOperationLeaf(token.Lexeme);
                    break;
                case TokenType.Number:
                    yylval = Node.MakeIntLeaf(Convert.ToInt32(token.Value!));
                    break;
                case TokenType.Float:
                    yylval = Node.MakeDoubleLeaf(Convert.ToDouble(token.Value!));
                    break;
                case TokenType.True or TokenType.False:
                    yylval = Node.MakeBoolLeaf(Convert.ToBoolean(token.Value!));
                    break;
                case TokenType.Integer or TokenType.Real or TokenType.Boolean:
                    yylval = Node.MakePrimitiveTypeLeaf(token.Lexeme);
                    break;
            }


            return (int)GppgTokensType(token.Type);
        }
        catch (Exception exception)
        {
            yyerror(exception.ToString());
            return (int)Tokens.error;
        }
    }

    public sealed override LexLocation yylloc { get; set; }

    public override void yyerror(string format, params object[] args)
    {
        Console.Error.WriteLine(format, args);
        base.yyerror(format, args);
    }
}