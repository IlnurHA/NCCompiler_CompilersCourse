namespace NCCompiler_CompilersCourse;

public class Lexer
{
    private readonly string input;
    private int currentPosition = 0;
    private List<Token> tokens = new List<Token>();

    public Lexer(string input)
    {
        this.input = input;
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
        var lineCounter = 1;
        var lastEOLIndex = 0;
        while (currentPosition < input.Length)
        {
            char currentChar = input[currentPosition];

            if (char.IsDigit(currentChar))
            {
                // Then it is a digit
                var lexemeLength = 1;
                var isDot = false;

                while (lexemeLength + currentPosition < input.Length && (char.IsDigit(input[lexemeLength + currentPosition]) ||
                                                                    input[lexemeLength + currentPosition] == '.'))
                {
                    lexemeLength++;
                    if (input[lexemeLength + currentPosition] == '.')
                    {
                        if (isDot)
                        {
                            throw new Exception("Wrong float argument");
                        }
                        isDot = true;
                    }
                }

                var substring = input.Substring(currentPosition, lexemeLength);
                tokens.Add(new Token(GetTokenType(substring), substring, new Span(lineCounter, currentPosition - lastEOLIndex, currentPosition + lexemeLength - lastEOLIndex)));
            }
            else if (!char.IsLetter(currentChar))
            {
                var currentLinePosition = currentPosition - lastEOLIndex;
                var currentLineExtendedPosition = currentLinePosition + 1;
                
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
                    tokens.Add(new Token(currentChar.ToString()), currentChar.ToString(), new Span(lineCounter, currentLinePosition, currentLinePosition))
                }
            }
        }

        // tokens.Add(new Token(TokenType.EOF, "")); // End of file marker
        return tokens;
    }
}