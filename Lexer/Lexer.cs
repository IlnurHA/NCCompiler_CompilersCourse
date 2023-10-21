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
            "reverse" => TokenType.Reverse,
            "foreach" => TokenType.Foreach,
            "not" => TokenType.Not,

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
            TokenType.Reverse => Tokens.REVERSE,
            TokenType.Foreach => Tokens.FOREACH,
            TokenType.UnaryPlus => Tokens.UNARY_PLUS,
            TokenType.UnaryMinus => Tokens.UNARY_MINUS,
            TokenType.Not => Tokens.NOT,
            _ => throw new Exception($"Unknown type {tokenType}")
        };
    }

    private int _lineCounter;
    private int _lastEolIndex;

    private bool IsCharToken(string tokenToCheck)
    {
        if (_currentPosition + (tokenToCheck.Length - 1) >= _input.Length) return false;

        string substring = _input.Substring(_currentPosition, tokenToCheck.Length);

        return substring.Equals(tokenToCheck);
    }

    private bool IsCharTokens(params string[] tokensToCheck)
    {
        return tokensToCheck.Any(IsCharToken);
    }

    private void SkipUntilToken(string token, bool skipToken)
    {
        do
        {
            if (IsCharToken(token))
            {
                if (skipToken) _currentPosition += token.Length;
                return;
            }

            UpdateLine();
            _currentPosition++;
        } while (_input.Length > _currentPosition);
    }

    private void UpdateLine()
    {
        if (!IsCharToken("\n")) return;
        _lineCounter++;
        _lastEolIndex = _currentPosition;
    }

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
        _lineCounter = 1;
        _lastEolIndex = 0;

        while (_currentPosition < _input.Length)
        {
            var currentChar = _input[_currentPosition];
            var lexemeLength = 1;

            if (char.IsWhiteSpace(currentChar) || currentChar == '\r')
            {
                UpdateLine();
                _currentPosition++;
                continue;
            }

            if (char.IsDigit(currentChar))
            {
                // Then it is a digit

                var isDot = false;
                while (lexemeLength + _currentPosition < _input.Length &&
                       (char.IsDigit(_input[lexemeLength + _currentPosition]) ||
                        (_input[lexemeLength + _currentPosition] == '.' && !isDot)))
                {
                    if (_input[lexemeLength + _currentPosition] == '.') isDot = true;
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
                if (IsCharTokens("/=", ":=", ">=", "<=", ".."))
                {
                    lexemeLength = 2;
                }
                else if (IsCharToken("/*"))
                {
                    SkipUntilToken("*/", true);
                    continue;
                }
                else if (IsCharToken("//"))
                {
                    SkipUntilToken("\n", false);
                    continue;
                }
                else
                {
                    lexemeLength = 1;
                }
            }

            var substring = _input.Substring(_currentPosition, lexemeLength);
            var tokenType = GetTokenType(substring);
            var newToken = new Token(
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
            UpdateLine();
            _currentPosition += lexemeLength;
            return newToken;
        }

        return null;
    }


    public override int yylex()
    {
        try
        {
            Token? token;
            do
            {
                token = NextToken();
                if (token == null)
                {
                    return (int) Tokens.EOF;
                }
            } while (token.Type is TokenType.SinglelineComment or TokenType.MultilineCommentEnd
                     or TokenType.MultilineCommentStart);

            yylloc = new LexLocation((int) token.Span.LineNum, (int) token.Span.LineNum, token.Span.PosBegin,
                token.Span.PosEnd);
            switch (token.Type)
            {
                case TokenType.Identifier:
                    yylval = new LeafNode<string>(NodeTag.Identifier, token.Lexeme);
                    break;
                case TokenType.UnaryPlus or TokenType.UnaryMinus:
                    yylval = new LeafNode<string>(NodeTag.Unary, token.Lexeme);
                    break;
                case TokenType.Number:
                    yylval = new LeafNode<Int32>(NodeTag.IntegerLiteral, Convert.ToInt32(token.Value!));
                    break;
                case TokenType.Float:
                    yylval = new LeafNode<Double>(NodeTag.RealLiteral, Convert.ToDouble(token.Value!));
                    break;
                case TokenType.True or TokenType.False:
                    yylval = new LeafNode<Boolean>(NodeTag.BooleanLiteral, Convert.ToBoolean(token.Value!));
                    break;
                case TokenType.Integer or TokenType.Real or TokenType.Boolean:
                    yylval = new LeafNode<string>(NodeTag.PrimitiveType, token.Lexeme);
                    break;
            }

            return (int) GppgTokensType(token.Type);
        }
        catch (Exception exception)
        {
            yyerror(exception.ToString());
            return (int) Tokens.error;
        }
    }

    public sealed override LexLocation yylloc { get; set; }

    public override void yyerror(string format, params object[] args)
    {
        Console.Error.WriteLine(format, args);
        Console.Error.WriteLine(
            $"Line: {yylloc.StartLine}:{yylloc.EndLine}, Range: {yylloc.StartColumn}:{yylloc.EndColumn}");
        base.yyerror(format, args);
    }
}