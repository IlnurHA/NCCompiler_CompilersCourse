using System.Text.RegularExpressions;

namespace NCCompiler_CompilersCourse;

public class Lexer
{
    private readonly string _input;
    private int _currentPosition;
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
            var someVal when new Regex(@"^\w[\w\d_]*$").IsMatch(someVal) =>
                TokenType.Identifier,
            var someVal when new Regex(@"^-?\d+$").IsMatch(someVal) =>
                TokenType.Number,
            var someVal when new Regex(@"^-?\d+\.?\d+$").IsMatch(someVal) =>
                TokenType.Float,
            _ => throw new NotImplementedException($"The {token} is not implemented")
        };
    }

    public List<Token> Tokenize()
    {
        var lineCounter = 1;
        var lastEolIndex = 0;
        
        var inSingleLineComment = false;
        var inMultiLineComment = false;
        
        while (_currentPosition < _input.Length)
        {
            var currentChar = _input[_currentPosition];
            var lexemeLength = 1;

            if (inSingleLineComment || inMultiLineComment)
            {
                if (currentChar == '\n' && inSingleLineComment)
                {
                    inSingleLineComment = false;
                } 
                else if (currentChar == '*' && _input[_currentPosition + 1] == '/' && inMultiLineComment)
                {
                    inMultiLineComment = false;
                }
                else {
                    if (currentChar == '\n')
                    {
                        lineCounter++;
                        lastEolIndex = _currentPosition;
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
                    inMultiLineComment = true;
                }
                else if (_currentPosition + 1 < _input.Length &&
                         (currentChar == '/' && _input[_currentPosition + 1] == '/'))
                {
                    lexemeLength = 2;
                    inSingleLineComment = true;
                }
                else
                {
                    lexemeLength = 1;
                }
            }

            var substring = _input.Substring(_currentPosition, lexemeLength);
                _tokens.Add(
                    new Token(
                        type: GetTokenType(substring),
                        lexeme: substring,
                        span: new Span(
                            lineNum: lineCounter,
                            posBegin: _currentPosition - lastEolIndex,
                            posEnd: _currentPosition + lexemeLength - lastEolIndex
                        )
                    )
                );
            if (currentChar == '\n')
            {
                lineCounter++;
                lastEolIndex = _currentPosition;
            }

            _currentPosition += lexemeLength;
        }

        // tokens.Add(new Token(TokenType.EOF, "")); // End of file marker
        return _tokens;
    }
}