using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAutomaton
{
    public class Transition
    {
        public int Min { get; set; }

        public int Max { get; set; }

        public State Destination { get; set; }
    }
}
