using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    class SolutionBranch
    {
        // Use several variable to keep track of state of the solving algorithm
        readonly Dictionary<string, Expression> KnownValues = new Dictionary<string, Expression>();
        public List<Equation> Equations = new List<Equation>();
        readonly List<string> FiguredOutValues = new List<string>();
        string NextToFigureOut = null;
        readonly List<SolutionStep> stepsTaken = new List<SolutionStep>();

        bool SkipParadoxCheck = false;

        bool Step1_0 = true;
        bool Step1_1 = false;
        bool Step1_3 = false;
        bool Step3_1 = false;
        bool BranchAt1_0 = true;
        EquationOperation equOp = null;

        public SolutionBranch Clone()
        {
            SolutionBranch newSb = new SolutionBranch();

            foreach (KeyValuePair<string, Expression> kp in KnownValues)
            {
                newSb.KnownValues.Add(kp.Key, kp.Value.Clone());
            }
            foreach (Equation eq in Equations)
            {
                newSb.Equations.Add(eq.Clone());
            }
            foreach (string s in FiguredOutValues)
            {
                newSb.FiguredOutValues.Add(s);
            }
            foreach (SolutionStep step in stepsTaken)
            {
                newSb.stepsTaken.Add(step);
            }

            newSb.Step1_0 = Step1_0;
            newSb.Step1_1 = Step1_1;
            newSb.Step1_3 = Step1_3;
            newSb.Step3_1 = Step3_1;
            newSb.BranchAt1_0 = BranchAt1_0;
            newSb.equOp = equOp;

            newSb.NextToFigureOut = NextToFigureOut;
            newSb.SkipParadoxCheck = SkipParadoxCheck;

            return newSb;
        }
        public SolutionResult DoNextSteps()
        {
            // Does several steps of solving a system, until it runs into a brancing point,
            // or finds a result

            while (true)
            {
                // 0) We found out all values. Do backwards substitution.
                if (Equations.Count == 0)
                {
                    for (int i = FiguredOutValues.Count - 1; i >= 0; i--)
                    {
                        KnownValues[FiguredOutValues[i]] = KnownValues[FiguredOutValues[i]].SubstitutingCopy(KnownValues);
                        KnownValues[FiguredOutValues[i]] = Simplification.FullSimplification(KnownValues[FiguredOutValues[i]]);
                    }

                    return SolutionResult.FromSuccess(KnownValues, stepsTaken);
                }

                // If not (0), then solve (non-deterministicaly) any equation on the list for (non-deterministicaly) any variable.

                // 1) Simplify equation then remove redundant variables and/or to prove its inconsistency.
                // 1.0) Initial substitution
                else if (Step1_0)
                {
                    // Pick equation to solve next (branch for all possibilities)
                    List<SolutionBranch> newBranches = new List<SolutionBranch>();

                    if (BranchAt1_0)
                    {
                        BranchAt1_0 = false;
                        for (int i = 1; i < Equations.Count; i++)
                        {
                            SolutionBranch sb = Clone();
                            Equation x = sb.Equations[i];
                            sb.Equations.RemoveAt(i);
                            sb.Equations.Insert(0, x);
                            newBranches.Add(sb);
                        }
                        SolutionBranch skip = Clone();
                        skip.SkipParadoxCheck = true;
                        skip.Step1_0 = false;
                        skip.Step1_1 = false;
                        skip.Step1_3 = true;
                        newBranches.Add(skip);
                    }
                    Step1_0 = false;
                    Step1_1 = true;
                    Equations[0] = Equations[0].SubstitutingCopy(KnownValues);

                    stepsTaken.Add(new SolutionStep("[Substitute] ... Initial substitution of (partly know values).", Equations[0]));

                    if (newBranches.Count > 0) return SolutionResult.FromMajorBranching(newBranches);
                }
                // 1.1) First simplification
                else if (Step1_1)
                {
                    Step1_0 = false;
                    Step1_1 = false;
                    Equations[0] = Equations[0].SimplifyingCopy();

                    stepsTaken.Add(new SolutionStep("[Simplify] ... Initial simplification.", Equations[0]));
                }
                // 1.2) Do the subtraction.
                else if (NextToFigureOut == null && !SkipParadoxCheck && !Equations[0].LeftSide.IsEquivalent(new OpRational(0, 1)) && !Step1_1)
                {
                    Equations[0] = new Equation(
                        new OpRational(0, 1),
                        new OpSum(Equations[0].RightSide.Clone(), new OpAdditiveInverse(Equations[0].LeftSide.Clone()))
                        );
                    Step1_3 = true;

                    stepsTaken.Add(new SolutionStep("[-Left Side] ... Subtract to try removing redundant variables.", Equations[0]));
                }
                // 1.3) Do the actual simplification
                else if (Step1_3)
                {
                    Equations[0] = Equations[0].SimplifyingCopy();
                    Step1_3 = false;

                    stepsTaken.Add(new SolutionStep("[Simplify] ... Remove redundant variables.", Equations[0]));
                }

                // 2) The equation happens to have no variables and thus is true or false
                else if (NextToFigureOut == null && Equations[0].LeftSide.IsEquivalent(new OpRational(0, 1)) && Equations[0].RightSide.ListVariables().Count == 0)
                {
                    if (Equations[0].LeftSide.IsEquivalent(Equations[0].RightSide))
                    {
                        Equations.RemoveAt(0);
                        Step1_0 = true;
                        NextToFigureOut = null;
                        BranchAt1_0 = true;

                        stepsTaken.Add(new SolutionStep("True equation. No new value was calculated. Carrying on.", null));
                    }
                    else throw new SolverException("Untrue equation found. This solution branch resulted in a contradiciton.");
                }

                // 3) There is at least 1 variable. Pick any variable to figure out the value of
                else if (NextToFigureOut == null)
                {
                    List<string> variables = new List<string>(Equations[0].ListVariables());
                    List<SolutionBranch> newBranches = new List<SolutionBranch>();
                    Step3_1 = true;

                    for (int i = 1; i < variables.Count; i++)
                    {
                        SolutionBranch newBranch = Clone();
                        newBranch.NextToFigureOut = variables[i];
                    }

                    Step3_1 = false;
                    NextToFigureOut = variables[0];
                    stepsTaken.Add(new SolutionStep($"[NOTE] ... variable {NextToFigureOut} will be isolated.", Equations[0]));

                    return SolutionResult.FromMajorBranching(newBranches);
                }
                else if (Step3_1)
                {
                    Step3_1 = false;
                    stepsTaken.Add(new SolutionStep($"[NOTE] ... variable {NextToFigureOut} will be isolated.", Equations[0]));
                }

                // 4) Do 1 step of isolation at a time
                // 4.0) Pick operation to apply andd apply it
                else if (!Equations[0].IsVariableIsolated && equOp == null)
                {
                    // Find out, what opperation should be applied to both sides of the equation.
                    equOp = Equations[0].IsolateVariableStep(NextToFigureOut);

                    List<Equation> sol = Equations[0].IsolateVariableApply(NextToFigureOut, equOp);
                    List<SolutionBranch> newBranches = new List<SolutionBranch>();

                    // There is no know way to continue
                    if (sol.Count == 0) throw new SolverException("A brach seems to have no solution.");
                    // There is more than 1 way to continue - start new branches
                    else
                    {
                        for (int i = 1; i < sol.Count; i++)
                        {
                            SolutionBranch newBrach = Clone();
                            newBrach.Equations.RemoveAt(0);
                            newBrach.Equations.Insert(0, sol[i].SimplifyingCopy());
                            newBranches.Add(newBrach);
                        }
                    }
                    Equations[0] = sol[0];

                    stepsTaken.Add(new SolutionStep($"[{equOp}] ... Apply to both sides of the equation to isolate {NextToFigureOut}", Equations[0]));

                    return SolutionResult.FromMinorBranching(newBranches);
                }

                // 4.1) simplify.
                else if (equOp != null)
                {
                    // Do 4.0 again
                    equOp = null;

                    Equations[0] = Equations[0].SimplifyingCopy();

                    stepsTaken.Add(new SolutionStep($"[Simplify] ... Simplify equation, while isolating {NextToFigureOut}", Equations[0]));
                }

                // 5) We found out the value of 1 variable.
                else
                {
                    Tuple<string, Expression> t = Equations[0].GetCalculatedValue();

                    if (t != null)
                    {
                        KnownValues.Add(t.Item1, t.Item2);
                        Equations.RemoveAt(0);
                        Step1_0 = true;
                        NextToFigureOut = null;
                        BranchAt1_0 = true;

                        for (int i = 0; i < FiguredOutValues.Count; i++)
                        {
                            KnownValues[FiguredOutValues[i]] = KnownValues[FiguredOutValues[i]].SubstitutingCopy(KnownValues);
                            KnownValues[FiguredOutValues[i]] = KnownValues[FiguredOutValues[i]].SimplifyingCopy();
                        }

                        FiguredOutValues.Add(t.Item1);

                        stepsTaken.Add(new SolutionStep($"Found value of variable {t.Item1}",null));
                    }

                    // Go back to paradox check
                    else
                    {
                        Step1_0 = true;
                        NextToFigureOut = null;
                        BranchAt1_0 = false;
                    }
                }
            }
            throw new SolverException("This shouldn't happen - solver ended up in invalid state due to an oversight.");
        }
    }

    class SolutionResult
    {
        // Informs the solving loop, when new possible branches of solution were found,
        // or when a solution was found.

        public Dictionary<string, Expression> resultingVariables = null;
        public List<SolutionBranch> newMajorBranches = null;
        public List<SolutionBranch> newMinorBranches = null;
        public List<SolutionStep> stepsTaken = null;

        public static SolutionResult FromSuccess(Dictionary<string, Expression> result, List<SolutionStep> stepsTaken)
        {
            SolutionResult sr = new SolutionResult
            {
                stepsTaken = stepsTaken,
                resultingVariables = result
            };
            return sr;
        }
        public static SolutionResult FromMajorBranching(List<SolutionBranch> branches)
        {
            SolutionResult sr = new SolutionResult
            {
                newMajorBranches = branches
            };
            return sr;
        }
        public static SolutionResult FromMinorBranching(List<SolutionBranch> branches)
        {
            SolutionResult sr = new SolutionResult
            {
                newMinorBranches = branches
            };
            return sr;
        }
    }

    class SolutionStep
    {
        public readonly string StepDescription;
        public readonly string EquationAfter;

        public SolutionStep(string description, Equation equation)
        {
            StepDescription = description;
            if (equation != null)
            {
                IExpressionVisitor<string> visitor = OutputFormatters.GetFormatter(Settings.OutputFormat);
                EquationAfter = equation.LeftSide.Accept(visitor) + "=" + equation.RightSide.Accept(visitor);
            }
            else EquationAfter = null;
        }
    }
}
