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

    public List<Token> Tokenize()
    {
        while (currentPosition < input.Length)
        {
            char currentChar = input[currentPosition];

            if (char.IsWhiteSpace(currentChar))
            {
                currentPosition++; // Skip whitespace
            }
            else if (currentChar == '+')
            {
                tokens.Add(new Token(TokenType.Plus, "+"));
                currentPosition++;
            }
            else if (currentChar == '-')
            {
                tokens.Add(new Token(TokenType.Minus, "-"));
                currentPosition++;
            }
            else if (currentChar == '*')
            {
                tokens.Add(new Token(TokenType.Multiply, "*"));
                currentPosition++;
            }
            else if (currentChar == '/')
            {
                tokens.Add(new Token(TokenType.Divide, "/"));
                currentPosition++;
            }
            else if (currentChar == '(')
            {
                tokens.Add(new Token(TokenType.LeftParen, "("));
                currentPosition++;
            }
            else if (currentChar == ')')
            {
                tokens.Add(new Token(TokenType.RightParen, ")"));
                currentPosition++;
            }
            else if (char.IsDigit(currentChar))
            {
                // Parse numbers
                int start = currentPosition;
                while (currentPosition < input.Length && (char.IsDigit(input[currentPosition]) || input[currentPosition] == '.'))
                {
                    currentPosition++;
                }
                string numberLexeme = input.Substring(start, currentPosition - start);
                if (double.TryParse(numberLexeme, out double numberValue))
                {
                    tokens.Add(new Token(TokenType.Number, numberLexeme, numberValue));
                }
                else
                {
                    // Handle invalid number
                    throw new Exception($"Invalid number: {numberLexeme}");
                }
            }
            else if (char.IsLetter(currentChar))
            {
                // Parse identifiers
                int start = currentPosition;
                while (currentPosition < input.Length && char.IsLetterOrDigit(input[currentPosition]))
                {
                    currentPosition++;
                }
                string identifierLexeme = input.Substring(start, currentPosition - start);
                tokens.Add(new Token(TokenType.Identifier, identifierLexeme));
            }
            else
            {
                // Handle unrecognized character
                throw new Exception($"Unrecognized character: {currentChar}");
            }
        }

        tokens.Add(new Token(TokenType.EOF, "")); // End of file marker
        return tokens;
    }
}