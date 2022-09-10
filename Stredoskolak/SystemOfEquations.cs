using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    public class SystemOfEquations
    {
        public List<string> Parameters = new List<string>();
        public List<Equation> Definition = new List<Equation>();

        public void Solve(bool useColor)
        {
            Stack<SolutionBranch> minorBranches = new Stack<SolutionBranch>();
            Stack<SolutionBranch> majorBranches = new Stack<SolutionBranch>();

            // Creates main branch.
            SolutionBranch mainBranch = new SolutionBranch();
            foreach (Equation e in Definition)
            {
                mainBranch.Equations.Add(e.Clone());
            }
            majorBranches.Push(mainBranch);

            int branchIndex = -1;
            List<string> failedBranchReasons = new List<string>();

            List<Tuple<Dictionary<string, Expression>, List<SolutionStep>>> uniqueSolutions = new List<Tuple<Dictionary<string, Expression>, List<SolutionStep>>>();
            List<Tuple<Dictionary<string, Expression>, List<SolutionStep>>> correctSolutions = new List<Tuple<Dictionary<string, Expression>, List<SolutionStep>>>();

            // Explore all branches of solution. If there are too many, end prematurely
            while ((majorBranches.Count > 0 || minorBranches.Count > 0) && branchIndex < Settings.MaxBranchesExplored)
            {
                branchIndex++;
                if (minorBranches.Count > 0) mainBranch = minorBranches.Pop();
                else mainBranch = majorBranches.Pop();

                try
                {
                    while (true)
                    {
                        SolutionResult sr = mainBranch.DoNextSteps();

                        // We found a solution - we test for uniqueness and whether it is correct, since we don't test for permited range of variables.
                        if (sr.resultingVariables != null)
                        {
                            // Test for uniquness
                            bool newSolution = true;
                            foreach (Tuple<Dictionary<string, Expression>, List<SolutionStep>> t in uniqueSolutions)
                            {
                                bool isDifferent = false;
                                if (sr.resultingVariables.Count == t.Item1.Count)
                                {
                                    foreach (KeyValuePair<string, Expression> kp in t.Item1)
                                    {
                                        if (!sr.resultingVariables.ContainsKey(kp.Key) || !kp.Value.IsEquivalent(sr.resultingVariables[kp.Key]))
                                        {
                                            isDifferent = true;
                                        }
                                    }
                                }
                                else isDifferent = true;

                                if (!isDifferent) newSolution = false;
                            }

                            // If unique, add the solution to unique solutions. Test for corectness. If correct, add it to definitely correct solutions.
                            if (newSolution)
                            {
                                uniqueSolutions.Add(new Tuple<Dictionary<string, Expression>, List<SolutionStep>>(sr.resultingVariables, sr.stepsTaken));
                                if (Equation.TestSolution(Definition, sr.resultingVariables))
                                    correctSolutions.Add(new Tuple<Dictionary<string, Expression>, List<SolutionStep>>(sr.resultingVariables, sr.stepsTaken));
                            }
                            break;
                        }

                        // We need to branch to test different orders of variables to solve for
                        else if (sr.newMajorBranches != null)
                        {
                            foreach (SolutionBranch sb in sr.newMajorBranches)
                            {
                                majorBranches.Push(sb);
                            }
                        }

                        // We need to branch to return multiple solutions (eg. solutions to a polynomial, or trigonometric function)
                        else if (sr.newMinorBranches != null)
                        {
                            foreach (SolutionBranch sb in sr.newMinorBranches)
                            {
                                minorBranches.Push(sb);
                            }
                        }
                    }
                }
                catch (SolverException e)
                {
                    failedBranchReasons.Add(e.Message);
                }
                catch (Exception e)
                {
                    failedBranchReasons.Add("Non-solver exception occured: " + e.GetType() + " - " + e.Message);
                }
            }

            // Print solutions - the correct ones if any exist, otherwise print warning and the possibly wrong ones too.
            if (uniqueSolutions.Count > 0)
            {
                if (correctSolutions.Count == 0) Console.Error.WriteLine("Warning: No solution guaranteed to be true found. Following solutions might be wrong as they didn't pass the trial.");

                int solutionIndex = 1;
                foreach (Tuple<Dictionary<string, Expression>, List<SolutionStep>> t in (correctSolutions.Count > 0 ? correctSolutions : uniqueSolutions))
                {
                    Console.WriteLine($"SOLUTION {solutionIndex}:");
                    if (Settings.CommentateCalculations)
                    {
                        foreach (SolutionStep ss in t.Item2)
                        {
                            if (useColor) Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(ss.StepDescription);
                            if (useColor) Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(ss.EquationAfter);
                        }
                        if (useColor) Console.ResetColor();
                        Console.WriteLine($"---");
                    }
                    if (useColor) Console.ForegroundColor = ConsoleColor.Cyan;
                    foreach (KeyValuePair<string, Expression> kp in t.Item1)
                    {
                        string textExpression = kp.Value.Accept(OutputFormatters.GetFormatter(Settings.OutputFormat));

                        if (Settings.AproximatelyEvaluate)
                        {
                            try
                            {
                                double aproximateValue = kp.Value.Accept(new DoubleEvaluator());
                                Console.WriteLine($"{kp.Key}={textExpression} [{aproximateValue}]");
                            }
                            catch
                            {
                                Console.WriteLine($"{kp.Key}={textExpression} [Couldn't evaluate]");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{kp.Key}={textExpression}");
                        }
                    }
                    if (useColor) Console.ResetColor();
                    Console.WriteLine($"---");

                    solutionIndex++;
                    if (solutionIndex >= Settings.MaxSolutionsReturned + 1) break;
                }
            }
            // otherwise, return few reasons why solving failed.
            else if (failedBranchReasons.Count > 0)
            {
                if (useColor) Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Warning: All branches of the solver failed. Some of the reasons are:");
                for (int i = 0; i < failedBranchReasons.Count && i < Settings.MaxReturnedFails; i++)
                {
                    Console.WriteLine(failedBranchReasons[i]);
                }
                if (useColor) Console.ResetColor();
            }
            // solving can fail even before starting calculation:
            else
            {
                if (useColor) Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Warning: Solver couldn't continue on any branch.");
                if (useColor) Console.ResetColor();
            }
        }

        public static SystemOfEquations FromFile(string file)
        {
            List<Equation> leq = Parser.ParseObjectFile(file);
            HashSet<string> variables = new HashSet<string>();

            foreach (Equation eq in leq)
            {
                variables.UnionWith(eq.LeftSide.ListVariables());
                variables.UnionWith(eq.RightSide.ListVariables());
            }

            SystemOfEquations se = new SystemOfEquations
            {
                Parameters = new List<string>(variables),
                Definition = leq
            };

            return se;
        }
        public static SystemOfEquations FromEquations(List<Equation> equations)
        {
            SystemOfEquations soe = new SystemOfEquations();
            HashSet<string> variables = new HashSet<string>();

            foreach (Equation e in equations)
            {
                soe.Definition.Add(e.Clone());
                variables.UnionWith(e.ListVariables());
            }

            soe.Parameters = new List<string>(variables);

            return soe;
        }
    }
}
