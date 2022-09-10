using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    class SolverException : Exception
    {
        public SolverException(string s) : base(s)
        {
        }
    }
    class ParserException : Exception
    {
        public ParserException(string s) : base(s)
        {
        }
    }
}
