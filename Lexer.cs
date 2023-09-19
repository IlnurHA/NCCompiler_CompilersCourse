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
        int lineCounter = 1;
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
                else if (currentChar == '*' && input[_currentPosition + 1] == '/' && inMultiLineComment)
                {
                    inMultiLineComment = false;
                }
                else {
                    if (currentChar == '\n')
                    {
                        lineCounter++;
                        lastEolIndex = currentPosition;
                    }
                    _currentPosition++;
                    continue;
                }
            }
            
            if (char.IsDigit(currentChar) && (is))
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
                if (_currentPosition + 1 < input.Length &&
                    ((currentChar == '/' && input[_currentPosition + 1] == '=') ||
                     (currentChar == '*' && input[_currentPosition + 1] == '/') ||
                     (currentChar == ':' && input[_currentPosition + 1] == '=') ||
                     (currentChar == '>' && input[_currentPosition + 1] == '=') ||
                     (currentChar == '<' && input[_currentPosition + 1] == '=') ||
                     (currentChar == '.' && input[_currentPosition + 1] == '.')))
                {
                    lexemeLength = 2;
                }
                else if ((_currentPosition + 1 < input.Length) &&
                         (currentChar == '/' && input[_currentPosition + 1] == '*'))
                {
                    lexemeLength = 2;
                    inMultiLineComment = true;
                }
                else if (_currentPosition + 1 < input.Length &&
                         (currentChar == '/' && input[_currentPosition + 1] == '/'))
                {
                    lexemeLength = 2;
                    inSingleLineComment = true
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
                lastEolIndex = currentPosition;
            }
        }

        // tokens.Add(new Token(TokenType.EOF, "")); // End of file marker
        return _tokens;
    }
}