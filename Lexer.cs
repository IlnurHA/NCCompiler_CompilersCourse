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
            "=" => TokenType.EqComparison,
            "," => TokenType.Comma,
            "\n" => TokenType.EndOfLine,
            _ => throw new NotImplementedException("The token is not implemented")
        };
    }

    public List<Token> Tokenize()
    {
        const int lineCounter = 1;
        var lastEolIndex = 0;
        while (_currentPosition < _input.Length)
        {
            var currentChar = _input[_currentPosition];
            var lexemeLength = 1;

            if (char.IsDigit(currentChar))
            {
                // Then it is a digit

                var isDot = false;

                while (lexemeLength + _currentPosition < _input.Length &&
                       (char.IsDigit(_input[lexemeLength + _currentPosition]) ||
                        _input[lexemeLength + _currentPosition] == '.'))
                {
                    lexemeLength++;
                    if (_input[lexemeLength + _currentPosition] != '.') continue;
                    if (isDot) throw new Exception("Wrong float argument");

                    isDot = true;
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

                if (currentChar == '/' && input[currentPosition + 1] == '=')
                {
                    tokens.Add(new Token("/="), "/=", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                } else if (currentChar == '/' && input[currentPosition + 1] == '/')
                {
                    tokens.Add(new Token("//"), "//", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                } else if (currentChar == '/' && input[currentPosition + 1] == '*')
                {
                    tokens.Add(new Token("/*"), "/*", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                } else if (currentChar == '*' && input[currentPosition + 1] == '/')
                {
                    tokens.Add(new Token("*/"), "*/", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                }
                else if (currentChar == ':' && input[currentPosition + 1] == '=')
                { 
                    tokens.Add(new Token(":="), ":=", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                } else if (currentChar == '>' && input[currentPosition + 1] == '=')
                {
                    tokens.Add(new Token(">="), ">=", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                } else if (currentChar == '<' && input[currentPosition + 1] == '=')
                {
                    tokens.Add(new Token("<="), "<=", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                } else if (currentChar == '.' && input[currentPosition + 1] == '.')
                {
                    tokens.Add(new Token(".."), "..", new Span(lineCounter, currentLinePosition, currentLineExtendedPosition))
                }
                else
                {
                    if (currentChar == '\n')
                    {
                        lineCounter++;
                        lastEOLIndex = currentChar;
                    }
                    tokens.Add(new Token(currentChar.ToString()), currentChar.ToString(), new Span(lineCounter, currentLinePosition, currentLinePosition))
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
        }
            }

        // tokens.Add(new Token(TokenType.EOF, "")); // End of file marker
        return _tokens;
    }
}