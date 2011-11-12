using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAutomaton
{
    public class State
    {
        public bool Accept { get; set; }

        public IList<Transition> GetSortedTransitions(bool p)
        {
            throw new NotImplementedException();
        }
    }
}
