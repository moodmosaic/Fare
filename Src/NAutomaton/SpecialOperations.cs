namespace NAutomaton
{
    public class SpecialOperations
    {
        internal static Automaton Overlap(Automaton automaton, Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton SingleChars(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Trim(Automaton automaton, string set, char c)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Compress(Automaton automaton, string set, char c)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Subst(Automaton automaton,
                                        System.Collections.Generic.IDictionary
                                            <char?, System.Collections.Generic.HashSet<char?>> map)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Subst(Automaton automaton, char c, string s)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton Homomorph(Automaton automaton, char[] source, char[] dest)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton ProjectChars(Automaton automaton, System.Collections.Generic.HashSet<char?> chars)
        {
            throw new System.NotImplementedException();
        }

        internal static bool IsFinite(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static System.Collections.Generic.HashSet<string> GetStrings(Automaton automaton, int length)
        {
            throw new System.NotImplementedException();
        }

        internal static System.Collections.Generic.HashSet<string> GetFiniteStrings(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static System.Collections.Generic.HashSet<string> GetFiniteStrings(Automaton automaton, int limit)
        {
            throw new System.NotImplementedException();
        }

        internal static string GetCommonPrefix(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static void PrefixClose(Automaton automaton)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton HexCases(Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static Automaton ReplaceWhitespace(Automaton a)
        {
            throw new System.NotImplementedException();
        }

        internal static System.Collections.Generic.HashSet<State> Reverse(Automaton a)
        {
            throw new System.NotImplementedException();
        }
    }
}