
namespace NAutomaton
{
    public class BasicOperations
    {
        internal static Automaton Concatenate(Automaton automaton, Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Concatenate(System.Collections.Generic.IList<Automaton> l)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Optional(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Repeat(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Repeat(Automaton automaton, int min)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Repeat(Automaton automaton, int min, int max)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Complement(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Minus(Automaton automaton, Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Intersection(Automaton automaton, Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static bool SubsetOf(Automaton automaton, Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Union(Automaton automaton, Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Union(System.Collections.Generic.ICollection<Automaton> l)
        {
            throw new System.NotImplementedException();
        }

        internal static void Determinize(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static void AddEpsilons(Automaton automaton, System.Collections.Generic.ICollection<StatePair> pairs)
        {
            throw new System.NotImplementedException();
        }

        internal static bool IsEmptyString(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static bool IsEmpty(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static bool IsTotal(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static string GetShortestExample(Automaton automaton, bool accepted)
        {
            throw new System.NotImplementedException();
        }

        internal static bool Run(Automaton automaton, string s)
        {
            throw new System.NotImplementedException();
        }
    }
}
