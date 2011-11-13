using System;

namespace NAutomaton
{
    public class Automaton
    {
        public State Initial { get; set; }

        public void Minimize()
        {
            throw new NotImplementedException();
        }

        public Automaton Intersection(Automaton automaton)
        {
            throw new NotImplementedException();
        }

        public Automaton Clone()
        {
            throw new NotImplementedException();
        }

        public Automaton Complement()
        {
            throw new NotImplementedException();
        }

        public Automaton Repeat(int min, int max)
        {
            throw new NotImplementedException();
        }

        public Automaton Repeat(int min)
        {
            throw new NotImplementedException();
        }

        public Automaton Repeat()
        {
            throw new NotImplementedException();
        }

        public Automaton Optional()
        {
            throw new NotImplementedException();
        }
    }
}
