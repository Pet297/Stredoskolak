using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.IO;

namespace Stredoskolak
{
    public static class Parser
    {
        public static Equation ParseEquation(string textRepresentation)
        {
            // Parse an expression, equals sign and another expression
            Queue<LexerToken> tokens = Tokenize(textRepresentation);
            Expression exp1 = ParseExpression(tokens);
            LexerToken nextToken = tokens.Dequeue();
            if (nextToken.Type != TokenType.EQUALS) throw new FormatException("Expected '=' in an equation.");
            Expression exp2 = ParseExpression(tokens);
            if (tokens.Count > 0) throw new FormatException("There were too many tokens while parsing the equation.");
            return new Equation(exp1, exp2);
        }
        public static Expression ParseExpression(string textRepresentation)
        {
            Queue<LexerToken> tokens = Tokenize(textRepresentation);
            return ParseExpression(tokens);
        }

        private static Expression ParseExpression(Queue<LexerToken> Tokens)
        {
            OpSum parsedSum = new OpSum();
            ExpressionType sumType = ExpressionType.PLUS;

            while (Tokens.Count > 0)
            {
                Expression ex = ParseTerm(Tokens);

                if (sumType == ExpressionType.PLUS) parsedSum.Operands.Add(ex);
                else parsedSum.Operands.Add(new OpAdditiveInverse(ex));

                if (Tokens.Count == 0) break;
                else if (Tokens.Peek().Type == TokenType.EQUALS) break;
                else if (Tokens.Peek().Type == TokenType.COMMA) break;
                else if (Tokens.Peek().Type == TokenType.RIGHT_BRACKET) break;

                LexerToken token = Tokens.Dequeue();

                switch (token.Type)
                {
                    case TokenType.PLUS:
                        sumType = ExpressionType.PLUS;
                        break;
                    case TokenType.MINUS:
                        sumType = ExpressionType.MINUS;
                        break;
                    default:
                        throw new ParserException("Expected + or -.");
                }
            }

            if (parsedSum.Operands.Count == 1) return parsedSum.Operands[0];
            else return parsedSum;
        }
        private static Expression ParseTerm(Queue<LexerToken> Tokens)
        {
            OpProduct parsedProduct = new OpProduct();

            ExpressionType prodType = ExpressionType.TIMES;

            while (Tokens.Count > 0)
            {
                Expression ex = ParseFactor(Tokens);

                if (prodType == ExpressionType.TIMES) parsedProduct.Operands.Add(ex);
                else parsedProduct.Operands.Add(new OpMultiplicativeInverse(ex));

                if (Tokens.Count == 0) break;
                else if (Tokens.Peek().Type == TokenType.EQUALS) break;
                else if (Tokens.Peek().Type == TokenType.COMMA) break;
                else if (Tokens.Peek().Type == TokenType.PLUS) break;
                else if (Tokens.Peek().Type == TokenType.MINUS) break;
                else if (Tokens.Peek().Type == TokenType.RIGHT_BRACKET) break;

                LexerToken token = Tokens.Dequeue();

                switch (token.Type)
                {
                    case TokenType.ASTERISK:
                        prodType = ExpressionType.TIMES;
                        break;
                    case TokenType.SLASH:
                        prodType = ExpressionType.DIVIDE;
                        break;
                    default:
                        throw new ParserException("Expected * or /.");
                }
            }

            if (parsedProduct.Operands.Count == 1) return parsedProduct.Operands[0];
            else return parsedProduct;
        }
        private static Expression ParseFactor(Queue<LexerToken> Tokens)
        {
            Expression parsedExpression = null;
            LexerToken t = Tokens.Dequeue();
            LexerToken s;

            switch (t.Type)
            {
                case TokenType.MINUS:
                    s = Tokens.Peek();
                    if (s.Type == TokenType.NUMERIC_LITERAL)
                    {
                        Tokens.Dequeue();

                        //Number of form -a/b or -a
                        BigInteger numerator1 = -s.NumericValue;
                        BigInteger denominator1 = 1;
                        if (Tokens.Count > 0 && Tokens.Peek().Type == TokenType.SLASH)
                        {
                            Tokens.Dequeue();
                            denominator1 = Tokens.Dequeue().NumericValue;
                        }

                        return new OpRational(numerator1, denominator1);
                    }
                    else if (s.Type == TokenType.SLASH)
                    {
                        Tokens.Dequeue();
                        //Number of form -/b or Expression of form -/EXP
                        if (Tokens.Count > 0 && Tokens.Peek().Type == TokenType.NUMERIC_LITERAL)
                        {
                            return new OpRational(-1, Tokens.Dequeue().NumericValue);
                        }
                        else parsedExpression = new OpAdditiveInverse(new OpMultiplicativeInverse(ParseFactor(Tokens)));
                    }
                    else
                    {
                        parsedExpression = new OpAdditiveInverse(ParseTerm(Tokens));
                    }
                    break;

                case TokenType.SLASH:
                    s = Tokens.Peek();
                    if (s.Type == TokenType.NUMERIC_LITERAL)
                    {
                        Tokens.Dequeue();
                        parsedExpression = new OpRational(1, s.NumericValue);
                    }
                    else
                    {
                        parsedExpression = new OpMultiplicativeInverse(ParseFactor(Tokens));
                    }
                    break;

                case TokenType.LEFT_BRACKET:
                    parsedExpression = ParseExpression(Tokens);
                    s = Tokens.Dequeue();
                    if (s.Type != TokenType.RIGHT_BRACKET) throw new FormatException("Expected ')' at the end of an bracketed expression");
                    break;

                case TokenType.NUMERIC_LITERAL:
                    //Number of form "a" or "a/b" or Expresion of form NUM/EXP
                    BigInteger numerator = t.NumericValue;
                    BigInteger denominator = 1;

                    if (Tokens.Count > 0 && Tokens.Peek().Type == TokenType.SLASH)
                    {
                        s = Tokens.Peek();
                        if (s.Type == TokenType.NUMERIC_LITERAL)
                        {
                            denominator = Tokens.Dequeue().NumericValue;
                        }
                    }

                    parsedExpression = new OpRational(numerator, denominator);

                    break;

                case TokenType.VARIABLE_LITERAL:
                    parsedExpression = new OpVariable(t.Identifier);
                    break;

                case TokenType.BINARY_FUNCTION:
                    s = Tokens.Dequeue();
                    if (s.Type != TokenType.LEFT_BRACKET) throw new FormatException("Expected '(' after a function name.");
                    Expression arg1 = ParseExpression(Tokens);
                    s = Tokens.Dequeue();
                    if (s.Type != TokenType.COMMA) throw new FormatException("Expected ',' inside a binary function.");
                    Expression arg2 = ParseExpression(Tokens);
                    s = Tokens.Dequeue();
                    if (s.Type != TokenType.RIGHT_BRACKET) throw new FormatException("Expected ')' at the end of a function.");
                    switch (t.Identifier)
                    {
                        case "pow": parsedExpression = new OpPower(arg1, arg2); break;
                        case "root": parsedExpression = new OpRoot(arg1, arg2); break;
                        case "log": parsedExpression = new OpLogarhithm(arg1, arg2); break;
                    }
                    break;

                case TokenType.UNARY_FUNCTION:
                    s = Tokens.Dequeue();
                    if (s.Type != TokenType.LEFT_BRACKET) throw new FormatException("Expected '(' after a function name.");
                    Expression argu = ParseExpression(Tokens);
                    s = Tokens.Dequeue();
                    if (s.Type != TokenType.RIGHT_BRACKET) throw new FormatException("Expected ')' at the end of a function.");
                    switch (t.Identifier)
                    {
                        case "sqrt": parsedExpression = new OpRoot(new OpRational(2, 1), argu); break;
                        case "cbrt": parsedExpression = new OpRoot(new OpRational(3, 1), argu); break;
                        case "log10": parsedExpression = new OpLogarhithm(new OpRational(10, 1), argu); break;
                        case "log2": parsedExpression = new OpLogarhithm(new OpRational(2, 1), argu); break;
                        case "ln": parsedExpression = new OpLogarhithm(new OpE(), argu); break;

                        case "sin": parsedExpression = new OpSin(argu); break;
                        case "cos": parsedExpression = new OpCos(argu); break;
                        case "tg": parsedExpression = new OpTg(argu); break;
                        case "cotg": parsedExpression = new OpCotg(argu); break;
                        case "sec": parsedExpression = new OpSec(argu); break;
                        case "csc": parsedExpression = new OpCsc(argu); break;
                        case "arcsin": parsedExpression = new OpArcSin(argu); break;
                        case "arccos": parsedExpression = new OpArcCos(argu); break;
                        case "arctg": parsedExpression = new OpArcTg(argu); break;
                        case "arccotg": parsedExpression = new OpArcCotg(argu); break;
                        case "arcsec": parsedExpression = new OpArcSec(argu); break;
                        case "arccsc": parsedExpression = new OpArcCsc(argu); break;
                    }
                    break;

                case TokenType.REAL_CONSTANT:
                    switch (t.Identifier)
                    {
                        case "pi": parsedExpression = new OpPi(); break;
                        case "e": parsedExpression = new OpE(); break;
                    }
                    break;

                case TokenType.PLUS: throw new FormatException("Unexpected '+' at the begining of an expression.");
                case TokenType.ASTERISK: throw new FormatException("Unexpected '*' at the begining of an expression.");
                case TokenType.COMMA: throw new FormatException("Unexpected ',' at the begining of an expression.");
                case TokenType.EQUALS: throw new FormatException("Unexpected '=' at the begining of an expression.");
                case TokenType.RIGHT_BRACKET: throw new FormatException("Unexpected ')' at the begining of an expression.");
            }

            if (Tokens.Count > 0 && Tokens.Peek().Type == TokenType.ARROW)
            {
                Tokens.Dequeue();
                Expression exp2 = ParseFactor(Tokens);
                parsedExpression = new OpPower(parsedExpression, exp2);
            }

            return parsedExpression;
        }
        private static Queue<LexerToken> Tokenize(string s)
        {
            int tokenStart = 0;

            Queue<LexerToken> tokens = new Queue<LexerToken>();
            CharType charType = CharType.NONE;

            for (int i = 0; i <= s.Length; i++)
            {
                if (i != tokenStart)
                {
                    if (charType == CharType.LETTER)
                    {
                        if (i == s.Length || !(s[i] >= 'A' && s[i] <= 'Z' || s[i] >= 'a' && s[i] <= 'z' || s[i] == '_' || s[i] >= '0' && s[i] <= '9'))
                        {
                            tokens.Enqueue(LexerToken.FromLiteral(s.Substring(tokenStart, i - tokenStart)));
                            tokenStart = i;
                        }
                    }
                    else if (charType == CharType.NUMBER)
                    {
                        if (i != s.Length && (s[i] >= 'A' && s[i] <= 'Z' || s[i] >= 'a' && s[i] <= 'z' || s[i] == '_'))
                        {
                            throw new FormatException($"Unexpected character '{s[i]}' after a numeral in the parsed equation.");
                        }
                        else if (i == s.Length || !(s[i] >= '0' && s[i] <= '9'))
                        {
                            tokens.Enqueue(LexerToken.Number(BigInteger.Parse(s.Substring(tokenStart, i - tokenStart))));
                            tokenStart = i;
                        }
                    }
                }

                if (i == tokenStart && i != s.Length)
                {
                    switch(s[i])
                    {
                        case '+': tokens.Enqueue(LexerToken.FromType(TokenType.PLUS)); tokenStart++; break;
                        case '-': tokens.Enqueue(LexerToken.FromType(TokenType.MINUS)); tokenStart++; break;
                        case '*': tokens.Enqueue(LexerToken.FromType(TokenType.ASTERISK)); tokenStart++; break;
                        case '/': tokens.Enqueue(LexerToken.FromType(TokenType.SLASH)); tokenStart++; break;
                        case '^': tokens.Enqueue(LexerToken.FromType(TokenType.ARROW)); tokenStart++; break;
                        case ',': tokens.Enqueue(LexerToken.FromType(TokenType.COMMA)); tokenStart++; break;
                        case '=': tokens.Enqueue(LexerToken.FromType(TokenType.EQUALS)); tokenStart++; break;
                        case '(': tokens.Enqueue(LexerToken.FromType(TokenType.LEFT_BRACKET)); tokenStart++; break;
                        case ')': tokens.Enqueue(LexerToken.FromType(TokenType.RIGHT_BRACKET)); tokenStart++; break;
                        case ' ': tokenStart++; break;
                        default:
                            if (s[i] >= 'A' && s[i] <= 'Z' || s[i] >= 'a' && s[i] <= 'z' || s[i] == '_')
                            {
                                charType = CharType.LETTER;
                            }
                            else if (s[i] >= '0' && s[i] <= '9')
                            {
                                charType = CharType.NUMBER;
                            }
                            else throw new FormatException($"Unexpected character '{s[i]}' in the parsed equation");
                            break;
                    }
                }
            }

            return tokens;
        }

        public static List<Equation> ParseObjectFile(string path)
        {
            List<Equation> le = new List<Equation>();
            List<string> files = new List<string>();
            ParseObjectFile0(le, files, path + ".mobj");

            return le;
        }
        public static List<Equation> ParseObjectFileWithRenames(string line)
        {
            Dictionary<string, Expression> variableRenames = new Dictionary<string, Expression>();
            string filename;

            //If renames are present
            if (line.Contains('('))
            {
                line = line.Remove(line.Length - 1, 1);
                string[] parts = line.Split('(', 2);
                filename = parts[0];

                string[] renames = parts[1].Split(',');

                foreach (string rename in renames)
                {
                    string[] r = rename.Split('/', 2);
                    variableRenames.Add(r[0], new OpVariable(r[1]));
                }
            }
            else filename = line;

            List<Equation> toAdd = ParseObjectFile(filename);

            if (variableRenames.Count > 0)
            {
                List<Equation> toAdd2 = new List<Equation>();

                foreach (Equation eq in toAdd)
                {
                    toAdd2.Add(eq.SubstitutingCopy(variableRenames));
                }

                toAdd = toAdd2;
            }

            return toAdd;
        }

        private static void ParseObjectFile0(List<Equation> equations, List<string> visitedFiles, string file)
        {
            //Keeping track of cyclic calls
            if (visitedFiles.Contains(file)) throw new InvalidDataException($"Cyclic definition was detected in object file {file}.");
            visitedFiles.Add(file);

            try
            {
                StreamReader sr = new StreamReader(file);
                HashSet<string> addedVariables = new HashSet<string>();

                string line = sr.ReadLine();

                while (line != null)
                {
                    // if it's reference to another object file
                    if (!line.Contains('='))
                    {
                        Dictionary<string, Expression> variableRenames = new Dictionary<string, Expression>();
                        string filename = null;

                        // if there are renames
                        if (line.Contains('('))
                        {
                            line = line.Remove(line.Length - 1, 1);
                            string[] parts = line.Split('(', 2);
                            filename = parts[0];

                            string[] renames = parts[1].Split(',');

                            foreach (string rename in renames)
                            {
                                string[] r = rename.Split('/', 2);
                                if (!CorrectVariableName(r[1]))
                                {
                                    throw new InvalidDataException($"Varible rename \"{r[1]}\" invalid.");
                                }
                                variableRenames.Add(r[0], new OpVariable(r[1]));
                            }
                        }
                        else filename = line;

                        List<Equation> toAdd = new List<Equation>();
                        ParseObjectFile0(toAdd, visitedFiles, filename + ".mobj");

                        if (variableRenames.Count > 0)
                        {
                            List<Equation> toAdd2 = new List<Equation>();

                            foreach (Equation eq in toAdd)
                            {
                                toAdd2.Add(eq.SubstitutingCopy(variableRenames));
                            }

                            toAdd = toAdd2;
                        }

                        HashSet<string> newlyAddedVariables = new HashSet<string>();
                        foreach (Equation eq in toAdd)
                        {
                            equations.Add(eq);
                            newlyAddedVariables.UnionWith(eq.LeftSide.ListVariables());
                            newlyAddedVariables.UnionWith(eq.RightSide.ListVariables());
                        }

                        foreach (string s in newlyAddedVariables)
                        {
                            if (!CorrectVariableName(s)) throw new InvalidDataException($"Invalid variable name was detected in file {file}.");
                        }

                        if (newlyAddedVariables.Overlaps(addedVariables)) throw new InvalidDataException($"At least 2 referenced objects in object file {file} share variable names.");
                        else addedVariables.UnionWith(newlyAddedVariables);
                    }
                    else equations.Add(ParseEquation(line));

                    line = sr.ReadLine();
                }
            }
            catch (InvalidDataException e)
            {
                throw new InvalidDataException(e.Message);
            }
            catch (FileNotFoundException)
            {
                throw new InvalidDataException($"Referenced object file {file} doesn't exist.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new InvalidDataException($"Referenced object file {file} doesn't exist.");
            }
            catch
            {
                throw new InvalidDataException($"Unknown exception while loading object in file {file}.");
            }

            visitedFiles.Remove(file);
        }

        private static readonly List<string> FunctionNames = new List<string>()
        {
            "sin", "cos", "tg", "cotg", "sec", "csc", "arcsin", "arccos", "arctg", "arccotg", "arcsec", "arccsc", "sqrt", "cbrt", "log2", "log10", "ln", "pow", "root", "log"
        };
        private static bool CorrectVariableName(string varName)
        {
            if (varName == null) return false;
            if (varName.Length == 0) return false;

            if (!(varName[0] >= 'A' && varName[0] <= 'Z' || varName[0] >= 'a' && varName[0] <= 'z' || varName[0] == '_')) return false;

            for (int i = 1; i < varName.Length; i++)
            {
                if (!(varName[i] >= 'A' && varName[i] <= 'Z' || varName[i] >= 'a' && varName[i] <= 'z' || varName[i] >= '0' && varName[i] <= '9' || varName[i] == '_')) return false;
            }

            if (FunctionNames.Contains(varName)) return false;

            return true;
        }
    }

    public class LexerToken
    {
        public readonly TokenType Type;
        public readonly BigInteger NumericValue;
        public readonly string Identifier;

        LexerToken(TokenType type, BigInteger number, string id)
        {
            Type = type;
            NumericValue = number;
            Identifier = id;
        }

        public static LexerToken FromType(TokenType type) => new LexerToken(type, 0, null);
        public static LexerToken Number(BigInteger value) => new LexerToken(TokenType.NUMERIC_LITERAL, value, null);
        public static LexerToken VariableIdentifier(string value) => new LexerToken(TokenType.VARIABLE_LITERAL, 0, value);
        public static LexerToken BinaryFunction(string functionName) => new LexerToken(TokenType.BINARY_FUNCTION, 0, functionName);
        public static LexerToken UnaryFunction(string functionName) => new LexerToken(TokenType.UNARY_FUNCTION, 0, functionName);
        public static LexerToken RealConstant(string constantType) => new LexerToken(TokenType.REAL_CONSTANT, 0, constantType);

        public static LexerToken FromLiteral(string literal)
        {
            switch (literal.ToLower())
            {
                case "pi": return RealConstant("pi");
                case "e": return RealConstant("e");

                case "sin": return UnaryFunction("sin");
                case "cos": return UnaryFunction("cos");
                case "tg": return UnaryFunction("tg");
                case "cotg": return UnaryFunction("cotg");
                case "sec": return UnaryFunction("sec");
                case "csc": return UnaryFunction("csc");
                case "arcsin": return UnaryFunction("arcsin");
                case "arccos": return UnaryFunction("arccos");
                case "arctg": return UnaryFunction("arctg");
                case "arccotg": return UnaryFunction("arccotg");
                case "arcsec": return UnaryFunction("arcsec");
                case "arccsc": return UnaryFunction("arccsc");
                case "sqrt": return UnaryFunction("sqrt");
                case "cbrt": return UnaryFunction("cbrt");
                case "log2": return UnaryFunction("log2");
                case "log10": return UnaryFunction("log10");
                case "ln": return UnaryFunction("ln");

                case "pow": return BinaryFunction("pow");
                case "root": return BinaryFunction("root");
                case "log": return BinaryFunction("log");
                default: return VariableIdentifier(literal);
            }
        }
    }

    public enum TokenType
    {
        PLUS, MINUS, ASTERISK, SLASH, ARROW, EQUALS, COMMA, LEFT_BRACKET, RIGHT_BRACKET, NUMERIC_LITERAL, VARIABLE_LITERAL, BINARY_FUNCTION, UNARY_FUNCTION, REAL_CONSTANT
    }
    public enum CharType { NONE, LETTER, NUMBER }
    public enum ExpressionType { PLUS, MINUS, TIMES, DIVIDE }
}
