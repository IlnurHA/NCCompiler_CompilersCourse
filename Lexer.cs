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
            var lexemeLength = 1;

            if (char.IsDigit(currentChar))
            {
                // Then it is a digit

                var isDot = false;

                while (lexemeLength + currentPosition < input.Length &&
                       (char.IsDigit(input[lexemeLength + currentPosition]) ||
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
            }
            else if (char.IsLetter(currentChar))
            {
                // Then it is a identifier or keyword

                while (
                    lexemeLength + currentPosition < input.Length &&
                    char.IsLetterOrDigit(input[lexemeLength + currentPosition])
                ) lexemeLength++;
            }

            var substring = input.Substring(currentPosition, lexemeLength);
            tokens.Add(new Token(GetTokenType(substring), substring,
                new Span(lineCounter, currentPosition - lastEOLIndex,
                    currentPosition + lexemeLength - lastEOLIndex)));
        }

        // tokens.Add(new Token(TokenType.EOF, "")); // End of file marker
        return tokens;
    }
}