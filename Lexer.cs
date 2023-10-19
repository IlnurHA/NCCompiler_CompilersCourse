using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NCCompiler_CompilersCourse;

public class Lexer
{
    private readonly string _input;
    private int _currentPosition;
    private int _lineCounter, _lastEolIndex;
    private readonly List<Token> _tokens = new();

    public Lexer(string input)
    {
        _input = input;
    }

    private static TokenType GetTokenType(string token)
    {
        return token switch
        {
            "+" => TokenType.Plus,
            "-" => TokenType.Minus,
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

    public List<Token> Tokenize()
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
            _tokens.Add(
                new Token(
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
                )
            );
            
            UpdateLine();
            _currentPosition += lexemeLength;
        }

        // tokens.Add(new Token(TokenType.EOF, "")); // End of file marker
        return _tokens;
    }
}