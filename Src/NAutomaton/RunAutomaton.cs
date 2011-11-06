using System;

namespace NAutomaton
{
    public class RunAutomaton
    {
        internal bool[] accept;
        internal int initial;

        public virtual int InitialState
        {
            get { return initial; }
        }

        public virtual bool IsAccept(int state)
        {
            return accept[state];
        }

        public virtual int Step(int state, char c)
        {
            throw new NotImplementedException();
        }
    }
}