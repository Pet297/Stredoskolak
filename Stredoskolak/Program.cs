using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Stredoskolak
{
    class Program
    {
        static void Main(string[] args)
        {
            bool userMode = true;
            bool showHelp = false;

            if (!File.Exists("settings.txt")) GenerateSettingsFile();
            bool success = LoadSettingsFile();
            if (!success)
            {
                Console.Error.WriteLine("Error: Settings file couldn't be loaded. The file will be deleted.");
                File.Delete("settings.txt");
                return;
            }

            for (int j = 0; j < args.Length; j++)
            {
                if (args[j] == "-a") userMode = false;
                else if (args[j] == "-h") showHelp = true;
                else if (args[j] == "-f")
                {
                    if (j <= args.Length - 1)
                    {
                        Settings.OutputFormat = args[j + 1];
                        j++;
                    }
                    else
                    {
                        Console.WriteLine("Format flag should be followed by an output format.");
                        return;
                    }
                }
                else if (args[j].StartsWith("--"))
                {
                    if (j < args.Length - 1)
                    {
                        try
                        {
                            ChangeSettingFromConsole(args[j],args[j+1]);
                            j++;
                        }
                        catch
                        {
                            Console.Error.WriteLine($"Error: Unrecognized flag encountered: \"{args[j]}\", or invalid value specified: \"{args[j+1]}\"");
                            return;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"Error: Settings flags need to be followed by a value.");
                        return;
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Error: Unrecognized flag encountered: \"{args[j]}\"");
                    return;
                }
            }

            if (showHelp)
            {
                PrintHelp();
            }
            else if (userMode)
            {
                UserModeLoop();
            }
            else
            {
                AutomaticModeLoop();
            }
        }

        static void UserModeLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Welcome to the Stredoskolak equation solver.");
                Console.WriteLine("Please select an option:");
                Console.WriteLine("1: Solve equations");
                Console.WriteLine("2: Define systems of equations");
                Console.WriteLine("3: Change solver settings");
                Console.WriteLine("anything else: exit");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");
                string option = Console.ReadLine();

                if (option == "1") EquationSolvingLoop(true);
                else if (option == "2") ObjectDefiningLoop();
                else if (option == "3") SettingsLoop();
                else break;
            }
        }
        static void AutomaticModeLoop()
        {
            EquationSolvingLoop(false);
        }

        static void EquationSolvingLoop(bool userMode)
        {
            if (userMode) Console.Clear();
            while (true)
            {

                if (userMode) Console.WriteLine("Enter the system of equations. Enter empty line to finish entering the system. Enter the word \"exit\" to exit to menu.");
                if (userMode) Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");

                List<Equation> equations = new List<Equation>();
                bool exit = false;

                while (true)
                {
                    string line = Console.ReadLine();
                    if (line == null || line == "exit")
                    {
                        exit = true;
                        break;
                    }
                    else if (line == "")
                    {
                        break;
                    }
                    else if (line.Contains("="))
                    {
                        try
                        {
                            Equation eq = Parser.ParseEquation(line);
                            equations.Add(eq);
                        }
                        catch
                        {
                            Console.WriteLine("Invalid equation. Try entering it again.");
                        }
                    }
                    else
                    {
                        try
                        {
                            List<Equation> eqs = Parser.ParseObjectFileWithRenames(line);
                            foreach (Equation e in eqs)
                            {
                                equations.Add(e);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Parsing object file threw an {e.GetType()}: {e.Message}");
                            Console.WriteLine($"Try entering it again.");
                        }
                    }
                }

                if (exit) break;

                SystemOfEquations system = SystemOfEquations.FromEquations(equations);
                if (userMode) Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");
                system.Solve(userMode);
                if (userMode) Console.WriteLine("=====================================================================================================================");
                if (userMode) Console.WriteLine("=====================================================================================================================");
                Console.WriteLine("");
            }
        }
        static void ObjectDefiningLoop()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("Defining objects. Enter one equation (or object reference) per line.");
                Console.WriteLine("Enter empty line to finish defining object. Enter the word \"exit\" to exit to menu. Correctness isn't checked.");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");

                List<string> equations = new List<string>();
                bool exit = false;

                while (true)
                {
                    string line = Console.ReadLine();
                    if (line == null || line == "exit")
                    {
                        exit = true;
                        break;
                    }
                    else if (line == "")
                    {
                        break;
                    }
                    else
                    {
                        equations.Add(line);
                    }
                }

                if (exit) break;

                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("Now enter object name:");

                string name = Console.ReadLine();

                try
                {
                    StreamWriter sw = new StreamWriter(name + ".mobj");
                    foreach (string s in equations) sw.WriteLine(s);
                    sw.Dispose();
                    Console.Clear();
                    Console.WriteLine("Object succesfully added.");
                    Console.WriteLine("");
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There was an error while writing the object file.");
                    Console.ResetColor();
                }

            }
        }
        static void SettingsLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Changing settings - Enter a character separated by new value to change it (eg. \"a 150\"). Enter anything else to exit.");
                Console.WriteLine($"A) Max decomposition factor:                                        {Settings.MaxDecompositionFactor}");
                Console.WriteLine($"B) Max root exponent decomposition factor:                          {Settings.MaxExponentFactorOfRoot}");
                Console.WriteLine($"C) Max size of numbers in bytes:                                    {Settings.MaxSizeOfNumbersInBytes}");
                Console.WriteLine($"D) Max numbers guessed when solving high order polynomials:         {Settings.MaxGuessedNumbers}");
                Console.WriteLine($"E) Guessed number complexity (how SLOWLY it grows):                 {Settings.GuessedNumberComplexity}");
                Console.WriteLine($"F) Max resulting terms while expanding product of sums:             {Settings.MaxResultingTermsFromExpansion}");
                Console.WriteLine($"G) Max resulting terms while expanding trig. function of sum:       {Settings.MaxTermsForTrigonometricSumExpansion}");
                Console.WriteLine($"H) Max branches of solution explored:                               {Settings.MaxBranchesExplored}");
                Console.WriteLine($"I) Max solutions returned on output:                                {Settings.MaxSolutionsReturned}");
                Console.WriteLine($"J) Max recursion depth, while simplifying formulas:                 {Settings.MaxDepthForSimplification}");
                Console.WriteLine($"K) Max reasons of failure returned after failing to solve a system: {Settings.MaxReturnedFails}");
                Console.WriteLine($"L) Comentate calculations?                                          {Settings.CommentateCalculations}");
                Console.WriteLine($"M) Evaluate real expressions?                                       {Settings.AproximatelyEvaluate}");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");
                string s = Console.ReadLine();
                string[] ss = s.Split(' ');
                if (ss.Length == 2)
                {
                    if (!ChangeSettingFromMenu(ss[0], ss[1])) break;
                }
                else break;
            }
        }

        static void GenerateSettingsFile()
        {
            StreamWriter sw = new StreamWriter("settings.txt");

            sw.WriteLine("MaxDecompositionFactor=101");
            //sw.WriteLine("MaxPolynomialDegree=2");
            sw.WriteLine("MaxExponentFactorOfRoot=3");
            sw.WriteLine("MaxSizeOfNumbersInBytes=15");
            sw.WriteLine("");
            sw.WriteLine("MaxGuessedNumbers=7");
            sw.WriteLine("GuessedNumberComplexity=7");
            sw.WriteLine("");
            sw.WriteLine("MaxResultingTermsFromExpansion=24");
            sw.WriteLine("MaxTermsForTrigonometricSumExpansion=4");
            sw.WriteLine("");
            sw.WriteLine("MaxBranchesExplored=4096");
            sw.WriteLine("MaxSolutionsReturned=8");
            sw.WriteLine("MaxDepthForSimplification=30");
            sw.WriteLine("MaxReturnedFails=20");
            sw.WriteLine("CommentateCalculations=false");
            sw.WriteLine("Aproximate=false");

            sw.Dispose();
        }
        static bool LoadSettingsFile()
        {
            StreamReader sr = new StreamReader("settings.txt");

            string line = sr.ReadLine();

            try
            {
                while (sr.ReadLine() != null)
                {
                    if (line != "")
                    {
                        string[] s = line.Split('=');
                        if (s.Length == 2) ChangeSettingFromSettingsFile(s[0], s[1]);
                        else
                        {
                            sr.Dispose();
                            return false;
                        }
                    }
                    line = sr.ReadLine();
                }
                sr.Dispose();
                return true;
            }
            catch
            {
                sr.Dispose();
                return false;
            }
        }
        static void ChangeSettingFromConsole(string setting, string value)
        {
            switch(setting)
            {
                case "--max-decomposition-factor": Settings.MaxDecompositionFactor = int.Parse(value); break;
                //case "--max-polynomial-degree": Settings.MaxPolynomialDegree = int.Parse(value); break;
                case "--max-exponent-factor-of-root": Settings.MaxExponentFactorOfRoot = int.Parse(value); break;
                case "--max-size-of-number-in-bytes": Settings.MaxSizeOfNumbersInBytes = int.Parse(value); break;
                case "--max-guessed-numbers": Settings.MaxGuessedNumbers = int.Parse(value); break;
                case "--guessed-number-complexity": Settings.GuessedNumberComplexity = int.Parse(value); break;
                case "--max-resulting-terms-from-expansion": Settings.MaxResultingTermsFromExpansion = int.Parse(value); break;
                case "--max-terms-for-trig-sum-expansion": Settings.MaxTermsForTrigonometricSumExpansion = int.Parse(value); break;
                case "--max-branches-explored": Settings.MaxBranchesExplored = int.Parse(value); break;
                case "--max-solutions-returned": Settings.MaxSolutionsReturned = int.Parse(value); break;
                case "--max-depth-for-simplification": Settings.MaxDepthForSimplification = int.Parse(value); break;
                case "--max-returned-fails": Settings.MaxReturnedFails = int.Parse(value); break;
                case "--commentate-calculations": Settings.CommentateCalculations = bool.Parse(value); break;
                case "--aproximate": Settings.AproximatelyEvaluate = bool.Parse(value); break;
                default: throw new FormatException("Non-existent flag specified.");
            }
        }
        static void ChangeSettingFromSettingsFile(string setting, string value)
        {
            switch (setting)
            {
                case "MaxDecompositionFactor": Settings.MaxDecompositionFactor = int.Parse(value); break;
                //case "MaxPolynomialDegree": Settings.MaxPolynomialDegree = int.Parse(value); break;
                case "MaxExponentFactorOfRoot": Settings.MaxExponentFactorOfRoot = int.Parse(value); break;
                case "MaxSizeOfNumbersInBytes": Settings.MaxSizeOfNumbersInBytes = int.Parse(value); break;
                case "MaxGuessedNumbers": Settings.MaxGuessedNumbers = int.Parse(value); break;
                case "GuessedNumberComplexity": Settings.GuessedNumberComplexity = int.Parse(value); break;
                case "MaxResultingTermsFromExpansion": Settings.MaxResultingTermsFromExpansion = int.Parse(value); break;
                case "MaxTermsForTrigonometricSumExpansion": Settings.MaxTermsForTrigonometricSumExpansion = int.Parse(value); break;
                case "MaxBranchesExplored": Settings.MaxBranchesExplored = int.Parse(value); break;
                case "MaxSolutionsReturned": Settings.MaxSolutionsReturned = int.Parse(value); break;
                case "MaxDepthForSimplification": Settings.MaxDepthForSimplification = int.Parse(value); break;
                case "MaxReturnedFails": Settings.MaxReturnedFails = int.Parse(value); break;
                case "CommentateCalculations": Settings.CommentateCalculations = bool.Parse(value); break;
                case "Aproximate": Settings.AproximatelyEvaluate = bool.Parse(value); break;
                default: throw new FormatException("Non-existent flag specified.");
            }
        }
        static bool ChangeSettingFromMenu(string setting, string value)
        {
            try
            {
                switch (setting.ToUpper())
                {
                    case "A": Settings.MaxDecompositionFactor = int.Parse(value); break;
                    //case "MaxPolynomialDegree": Settings.MaxPolynomialDegree = int.Parse(value); break;
                    case "B": Settings.MaxExponentFactorOfRoot = int.Parse(value); break;
                    case "C": Settings.MaxSizeOfNumbersInBytes = int.Parse(value); break;
                    case "D": Settings.MaxGuessedNumbers = int.Parse(value); break;
                    case "E": Settings.GuessedNumberComplexity = int.Parse(value); break;
                    case "F": Settings.MaxResultingTermsFromExpansion = int.Parse(value); break;
                    case "G": Settings.MaxTermsForTrigonometricSumExpansion = int.Parse(value); break;
                    case "H": Settings.MaxBranchesExplored = int.Parse(value); break;
                    case "I": Settings.MaxSolutionsReturned = int.Parse(value); break;
                    case "J": Settings.MaxDepthForSimplification = int.Parse(value); break;
                    case "K": Settings.MaxReturnedFails = int.Parse(value); break;
                    case "L": Settings.CommentateCalculations = bool.Parse(value); break;
                    case "M": Settings.AproximatelyEvaluate = bool.Parse(value); break;
                    default: return false;
                }
            }
            catch
            {
            }
            return true;
        }

        static void PrintHelp()
        {
            Console.WriteLine("Stredoskolak v1.0");
            Console.WriteLine("A solver for simple, repetitive, high-school-level equations.");
            Console.WriteLine("Petr Martinek, 2021");
            Console.WriteLine("");
            Console.WriteLine("For more information, run Stredoskolak in user mode, or refer to the guide.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Program arguments:");
            Console.WriteLine("");
            Console.WriteLine("-a         Run in automatic mode instead of user mode.");
            Console.WriteLine("-h         Show this help.");
            Console.WriteLine("-f [NAME]  Pick output format.");
            Console.WriteLine("");
            Console.WriteLine("Temporary settings:");
            Console.WriteLine("--max-decomposition-factor [Natural number]           Limit the primes considered while decomposing a number.");
            Console.WriteLine("--max-exponent-factor-of-root [Natural number]        Limit the primes considered while decomposing exponent of a root.");
            Console.WriteLine("--max-size-of-number-in-bytes [Natural number]        Limit the size of numbers in calculation.");
            Console.WriteLine("--max-guessed-numbers [Natural number]                Limit how many potential roots are guessed, while solving polynomials by guessing.");
            Console.WriteLine("--guessed-number-complexity [Natural number]          The larger this setting is, the less commonly are complex epressions guessed as roots of polynomials.");
            Console.WriteLine("--max-resulting-terms-from-expansion [Natural number] Limits the number of terms in expanded product of sums. Expansion doesn't occur if the number of terms would be higher.");
            Console.WriteLine("--max-terms-for-trig-sum-expansion [Natural number]   Limits expansion of trig functions applied to sums. If the sum contains more terms, expansion doesn't take place.");
            Console.WriteLine("--max-branches-explored [Natural number]              Limits how many branches of solution are explored, no matter if they fail or not.");
            Console.WriteLine("--max-solutions-returned [Natural number]             Limits how many succesful branches of solution are explored.");
            Console.WriteLine("--max-depth-for-simplification [Natural number]       Cuts a solution branch if recursive simplification alorhithm gets too deep.");
            Console.WriteLine("--max-returned-fails [Natural number]                 Limits the number of reasons for failure which are returned, when no solution is found.");
            Console.WriteLine("--commentate-calculations [true/false]                If set to true, commentates steps taken to solve a system of equations.");
            Console.WriteLine("");
            Console.WriteLine("Note: to change settings permanently, edit \"settings.txt\". Delete the file to reset settings. It is also possible to change sttings in user mode.");

            return;
        }
    }
}
