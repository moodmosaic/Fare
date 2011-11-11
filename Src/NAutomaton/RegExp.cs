/*
 * dk.brics.automaton
 * 
 * Copyright (c) 2001-2011 Anders Moeller
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NAutomaton
{
    public class RegExp
    {
        private static int intersection = 0x0001;
        private static int complement   = 0x0002;
        private static int empty        = 0x0004;
        private static int anystring    = 0x0008;
        private static int automaton    = 0x0010;
        private static int interval     = 0x0020;
        private static int all          = 0xffff;
        private static bool allowMutation;

        private readonly string b;
        private readonly int flags;

        private char c;
        private int digits;
        private RegExp exp1, exp2;
        private char from;
        private Kind kind;
        private int max;
        private int min;
        private int pos;
        private string s;
        private char to;

        private RegExp()
        {
        }

        public RegExp(string s)
            : this(s, all)
        {
        }

        public RegExp(string s, int syntaxFlags)
        {
            b = s;
            flags = syntaxFlags;
            RegExp e;
            if (s.Length == 0)
            {
                e = MakeString("");
            }
            else
            {
                e = ParseUnionExp();
                if (pos < b.Length)
                {
                    throw new ArgumentException("end-of-string expected at position " + pos);
                }
            }

            kind   = e.kind;
            exp1   = e.exp1;
            exp2   = e.exp2;
            this.s = e.s;
            c      = e.c;
            min    = e.min;
            max    = e.max;
            digits = e.digits;
            from   = e.from;
            to     = e.to;
            b      = null;
        }

        public Automaton ToAutomaton()
        {
            return ToAutomatonAllowMutate(null, null, true);
        }

        public Automaton ToAutomaton(bool minimize)
        {
            return ToAutomatonAllowMutate(null, null, minimize);
        }

        public Automaton ToAutomaton(IAutomatonProvider automatonProvider)
        {
            return ToAutomatonAllowMutate(null, automatonProvider, true);
        }

        public Automaton ToAutomaton(IAutomatonProvider automatonProvider, bool minimize)
        {
            return ToAutomatonAllowMutate(null, automatonProvider, minimize);
        }

        public Automaton ToAutomaton(IDictionary<string, Automaton> automata)
        {
            return ToAutomatonAllowMutate(automata, null, true);
        }

        public Automaton ToAutomaton(IDictionary<string, Automaton> automata, bool minimize)
        {
            return ToAutomatonAllowMutate(automata, null, minimize);
        }

        public bool SetAllowMutate(bool flag)
        {
            bool temp = allowMutation;
            allowMutation = flag;
            return temp;
        }

        private Automaton ToAutomatonAllowMutate(IDictionary<string, Automaton> automata, IAutomatonProvider automatonProvider, bool minimize)
        {
            bool temp = false;
            if (allowMutation)
            {
                temp = Automaton.SetAllowMutate(true); // thread unsafe
            }
            Automaton a = ToAutomaton(automata, automatonProvider, minimize);
            if (allowMutation)
            {
                Automaton.SetAllowMutate(temp);
            }
            return a;
        }

        private Automaton ToAutomaton(IDictionary<string, Automaton> automata, IAutomatonProvider automatonProvider, bool minimize)
        {
            List<Automaton> list;
            Automaton a = null;
            switch (kind)
            {
                case Kind.RegexpUnion:
                    list = new List<Automaton>();
                    FindLeaves(exp1, Kind.RegexpUnion, list, automata, automatonProvider, minimize);
                    FindLeaves(exp2, Kind.RegexpUnion, list, automata, automatonProvider, minimize);
                    a = BasicOperations.Union(list);
                    a.Minimize();
                    break;
                case Kind.RegexpConcatenation:
                    list = new List<Automaton>();
                    FindLeaves(exp1, Kind.RegexpConcatenation, list, automata, automatonProvider, minimize);
                    FindLeaves(exp2, Kind.RegexpConcatenation, list, automata, automatonProvider, minimize);
                    a = BasicOperations.Concatenate(list);
                    a.Minimize();
                    break;
                case Kind.RegexpIntersection:
                    a = exp1.ToAutomaton(automata, automatonProvider, minimize).Intersection(exp2.ToAutomaton(automata, automatonProvider, minimize));
                    a.Minimize();
                    break;
                case Kind.RegexpOptional:
                    a = exp1.ToAutomaton(automata, automatonProvider, minimize).Optional();
                    a.Minimize();
                    break;
                case Kind.RegexpRepeat:
                    a = exp1.ToAutomaton(automata, automatonProvider, minimize).Repeat();
                    a.Minimize();
                    break;
                case Kind.RegexpRepeatMin:
                    a = exp1.ToAutomaton(automata, automatonProvider, minimize).Repeat(min);
                    a.Minimize();
                    break;
                case Kind.RegexpRepeatMinmax:
                    a = exp1.ToAutomaton(automata, automatonProvider, minimize).Repeat(min, max);
                    a.Minimize();
                    break;
                case Kind.RegexpComplement:
                    a = exp1.ToAutomaton(automata, automatonProvider, minimize).Complement();
                    a.Minimize();
                    break;
                case Kind.RegexpChar:
                    a = BasicAutomata.MakeChar(c);
                    break;
                case Kind.RegexpCharRange:
                    a = BasicAutomata.MakeCharRange(from, to);
                    break;
                case Kind.RegexpAnychar:
                    a = BasicAutomata.MakeAnyChar();
                    break;
                case Kind.RegexpEmpty:
                    a = BasicAutomata.MakeEmpty();
                    break;
                case Kind.RegexpString:
                    a = BasicAutomata.MakeString(s);
                    break;
                case Kind.RegexpAnystring:
                    a = BasicAutomata.MakeAnyString();
                    break;
                case Kind.RegexpAutomaton:
                    Automaton aa = null;
                    if (automata != null)
                    {
                        aa = automata[s];
                    }
                    if (aa == null && automatonProvider != null)
                        try
                        {
                            aa = automatonProvider.GetAutomaton(s);
                        }
                        catch (IOException e)
                        {
                            throw new ArgumentException("", e);
                        }
                    if (aa == null)
                        throw new ArgumentException("'" + s + "' not found");
                    a = aa.Clone(); // Always clone here (ignore allowMutate).
                    break;
                case Kind.RegexpInterval:
                    a = BasicAutomata.MakeInterval(min, max, digits);
                    break;
            }
            return a;
        }

        private void FindLeaves(RegExp exp, Kind regKind, List<Automaton> list, IDictionary<String, Automaton> automata, IAutomatonProvider automatonProvider, bool minimize)
        {
            if (exp.kind == regKind)
            {
                FindLeaves(exp.exp1, regKind, list, automata, automatonProvider, minimize);
                FindLeaves(exp.exp2, regKind, list, automata, automatonProvider, minimize);
            }
            else
            {
                list.Add(exp.ToAutomaton(automata, automatonProvider, minimize));
            }
        }

        public override string ToString()
        {
            return ToStringBuilder(new StringBuilder()).ToString();
        }

        private StringBuilder ToStringBuilder(StringBuilder sb)
        {
            switch (kind)
            {
                case Kind.RegexpUnion:
                    sb.Append("(");
                    exp1.ToStringBuilder(sb);
                    sb.Append("|");
                    exp2.ToStringBuilder(sb);
                    sb.Append(")");
                    break;
                case Kind.RegexpConcatenation:
                    exp1.ToStringBuilder(sb);
                    exp2.ToStringBuilder(sb);
                    break;
                case Kind.RegexpIntersection:
                    sb.Append("(");
                    exp1.ToStringBuilder(sb);
                    sb.Append("&");
                    exp2.ToStringBuilder(sb);
                    sb.Append(")");
                    break;
                case Kind.RegexpOptional:
                    sb.Append("(");
                    exp1.ToStringBuilder(sb);
                    sb.Append(")?");
                    break;
                case Kind.RegexpRepeat:
                    sb.Append("(");
                    exp1.ToStringBuilder(sb);
                    sb.Append(")*");
                    break;
                case Kind.RegexpRepeatMin:
                    sb.Append("(");
                    exp1.ToStringBuilder(sb);
                    sb.Append("){").Append(min).Append(",}");
                    break;
                case Kind.RegexpRepeatMinmax:
                    sb.Append("(");
                    exp1.ToStringBuilder(sb);
                    sb.Append("){").Append(min).Append(",").Append(max).Append("}");
                    break;
                case Kind.RegexpComplement:
                    sb.Append("~(");
                    exp1.ToStringBuilder(sb);
                    sb.Append(")");
                    break;
                case Kind.RegexpChar:
                    sb.Append("\\").Append(c);
                    break;
                case Kind.RegexpCharRange:
                    sb.Append("[\\").Append(from).Append("-\\").Append(to).Append("]");
                    break;
                case Kind.RegexpAnychar:
                    sb.Append(".");
                    break;
                case Kind.RegexpEmpty:
                    sb.Append("#");
                    break;
                case Kind.RegexpString:
                    sb.Append("\"").Append(s).Append("\"");
                    break;
                case Kind.RegexpAnystring:
                    sb.Append("@");
                    break;
                case Kind.RegexpAutomaton:
                    sb.Append("<").Append(s).Append(">");
                    break;
                case Kind.RegexpInterval:
                    string s1 = min.ToString();
                    string s2 = max.ToString();
                    sb.Append("<");
                    if (digits > 0)
                        for (int i = s1.Length; i < digits; i++)
                            sb.Append('0');
                    sb.Append(s1).Append("-");
                    if (digits > 0)
                        for (int i = s2.Length; i < digits; i++)
                            sb.Append('0');
                    sb.Append(s2).Append(">");
                    break;
            }
            return sb;
        }

        public HashSet<String> GetIdentifiers()
        {
            var set = new HashSet<String>();
            GetIdentifiers(set);
            return set;
        }

        private void GetIdentifiers(HashSet<String> set)
        {
            switch (kind)
            {
                case Kind.RegexpUnion:
                case Kind.RegexpConcatenation:
                case Kind.RegexpIntersection:
                    exp1.GetIdentifiers(set);
                    exp2.GetIdentifiers(set);
                    break;
                case Kind.RegexpOptional:
                case Kind.RegexpRepeat:
                case Kind.RegexpRepeatMin:
                case Kind.RegexpRepeatMinmax:
                case Kind.RegexpComplement:
                    exp1.GetIdentifiers(set);
                    break;
                case Kind.RegexpAutomaton:
                    set.Add(s);
                    break;
            }
        }

        private static RegExp MakeUnion(RegExp exp1, RegExp exp2)
        {
            return new RegExp
            {
                kind = Kind.RegexpUnion,
                exp1 = exp1,
                exp2 = exp2
            };
        }

        private static RegExp MakeConcatenation(RegExp exp1, RegExp exp2)
        {
            if ((exp1.kind == Kind.RegexpChar || exp1.kind == Kind.RegexpString) &&
                (exp2.kind == Kind.RegexpChar || exp2.kind == Kind.RegexpString))
            {
                return MakeString(exp1, exp2);
            }
            var r = new RegExp();
            r.kind = Kind.RegexpConcatenation;
            if (exp1.kind == Kind.RegexpConcatenation &&
                (exp1.exp2.kind == Kind.RegexpChar || exp1.exp2.kind == Kind.RegexpString) &&
                (exp2.kind == Kind.RegexpChar || exp2.kind == Kind.RegexpString))
            {
                r.exp1 = exp1.exp1;
                r.exp2 = MakeString(exp1.exp2, exp2);
            }
            else if ((exp1.kind == Kind.RegexpChar || exp1.kind == Kind.RegexpString) &&
                     exp2.kind == Kind.RegexpConcatenation &&
                     (exp2.exp1.kind == Kind.RegexpChar || exp2.exp1.kind == Kind.RegexpString))
            {
                r.exp1 = MakeString(exp1, exp2.exp1);
                r.exp2 = exp2.exp2;
            }
            else
            {
                r.exp1 = exp1;
                r.exp2 = exp2;
            }
            return r;
        }

        private static RegExp MakeString(RegExp exp1, RegExp exp2)
        {
            var b = new StringBuilder();
            if (exp1.kind == Kind.RegexpString)
                b.Append(exp1.s);
            else
                b.Append(exp1.c);
            if (exp2.kind == Kind.RegexpString)
                b.Append(exp2.s);
            else
                b.Append(exp2.c);
            return MakeString(b.ToString());
        }

        private static RegExp MakeIntersection(RegExp exp1, RegExp exp2)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpIntersection;
            r.exp1 = exp1;
            r.exp2 = exp2;
            return r;
        }

        private static RegExp MakeOptional(RegExp exp)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpOptional;
            r.exp1 = exp;
            return r;
        }

        private static RegExp MakeRepeat(RegExp exp)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpRepeat;
            r.exp1 = exp;
            return r;
        }

        private static RegExp MakeRepeat(RegExp exp, int min)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpRepeatMin;
            r.exp1 = exp;
            r.min = min;
            return r;
        }

        private static RegExp MakeRepeat(RegExp exp, int min, int max)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpRepeatMinmax;
            r.exp1 = exp;
            r.min = min;
            r.max = max;
            return r;
        }

        private static RegExp MakeComplement(RegExp exp)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpComplement;
            r.exp1 = exp;
            return r;
        }

        private static RegExp MakeChar(char c)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpChar;
            r.c = c;
            return r;
        }

        private static RegExp MakeCharRange(char from, char to)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpCharRange;
            r.from = from;
            r.to = to;
            return r;
        }

        private static RegExp MakeAnyChar()
        {
            var r = new RegExp();
            r.kind = Kind.RegexpAnychar;
            return r;
        }

        private static RegExp MakeEmpty()
        {
            var r = new RegExp();
            r.kind = Kind.RegexpEmpty;
            return r;
        }

        private static RegExp MakeString(string s)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpString;
            r.s = s;
            return r;
        }

        private static RegExp MakeAnyString()
        {
            var r = new RegExp();
            r.kind = Kind.RegexpAnystring;
            return r;
        }

        private static RegExp MakeAutomaton(string s)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpAutomaton;
            r.s = s;
            return r;
        }

        private static RegExp MakeInterval(int min, int max, int digits)
        {
            var r = new RegExp();
            r.kind = Kind.RegexpInterval;
            r.min = min;
            r.max = max;
            r.digits = digits;
            return r;
        }

        private bool Peek(string str)
        {
            return More() && str.IndexOf(b[pos]) != -1;
        }

        private bool Match(char ch)
        {
            if (pos >= b.Length)
            {
                return false;
            }
            if (b[pos] == ch)
            {
                pos++;
                return true;
            }
            return false;
        }

        private bool More()
        {
            return pos < b.Length;
        }

        private char Next()
        {
            if (!More())
            {
                throw new ArgumentException("unexpected end-of-string");
            }
            return b[pos++];
        }

        private bool Check(int flag)
        {
            return (flags & flag) != 0;
        }

        private RegExp ParseUnionExp()
        {
            RegExp e = ParseInterExp();
            if (Match('|'))
            {
                e = MakeUnion(e, ParseUnionExp());
            }
            return e;
        }

        private RegExp ParseInterExp()
        {
            RegExp e = ParseConcatExp();
            if (Check(intersection) && Match('&'))
            {
                e = MakeIntersection(e, ParseInterExp());
            }
            return e;
        }

        private RegExp ParseConcatExp()
        {
            RegExp e = ParseRepeatExp();
            if (More() && !Peek(")|") && (!Check(intersection) || !Peek("&")))
            {
                e = MakeConcatenation(e, ParseConcatExp());
            }
            return e;
        }

        private RegExp ParseRepeatExp()
        {
            RegExp e = ParseComplExp();
            while (Peek("?*+{"))
            {
                if (Match('?'))
                {
                    e = MakeOptional(e);
                }
                else if (Match('*'))
                {
                    e = MakeRepeat(e);
                }
                else if (Match('+'))
                {
                    e = MakeRepeat(e, 1);
                }
                else if (Match('{'))
                {
                    int start = pos;
                    while (Peek("0123456789"))
                    {
                        Next();
                    }
                    if (start == pos)
                    {
                        throw new ArgumentException("integer expected at position " + pos);
                    }
                    int n = int.Parse(b.Substring(start, pos - start));
                    int m = -1;
                    if (Match(','))
                    {
                        start = pos;
                        while (Peek("0123456789"))
                        {
                            Next();
                        }
                        if (start != pos)
                        {
                            m = int.Parse(b.Substring(start, pos - start));
                        }
                    }
                    else
                    {
                        m = n;
                    }
                    if (!Match('}'))
                    {
                        throw new ArgumentException("expected '}' at position " + pos);
                    }

                    e = m == -1 ? MakeRepeat(e, n) : MakeRepeat(e, n, m);
                }
            }
            return e;
        }

        private RegExp ParseComplExp()
        {
            if (Check(complement) && Match('~'))
            {
                return MakeComplement(ParseComplExp());
            }

            return ParseCharClassExp();
        }

        private RegExp ParseCharClassExp()
        {
            if (Match('['))
            {
                bool negate = false;
                if (Match('^'))
                {
                    negate = true;
                }
                RegExp e = ParseCharClasses();
                if (negate)
                {
                    e = MakeIntersection(MakeAnyChar(), MakeComplement(e));
                }
                if (!Match(']'))
                {
                    throw new ArgumentException("expected ']' at position " + pos);
                }
                return e;
            }

            return ParseSimpleExp();
        }

        public RegExp ParseCharClasses()
        {
            RegExp e = ParseCharClass();
            while (More() && !Peek("]"))
            {
                e = MakeUnion(e, ParseCharClass());
            }
            return e;
        }

        public RegExp ParseCharClass()
        {
            char ch = ParseCharExp();
            if (Match('-'))
            {
                if (Peek("]"))
                {
                    return MakeUnion(MakeChar(ch), MakeChar('-'));
                }
                return MakeCharRange(ch, ParseCharExp());
            }
            return MakeChar(ch);
        }

        public RegExp ParseSimpleExp()
        {
            if (Match('.'))
                return MakeAnyChar();
            if (Check(empty) && Match('#'))
                return MakeEmpty();
            if (Check(anystring) && Match('@'))
                return MakeAnyString();
            if (Match('"'))
            {
                int start = pos;
                while (More() && !Peek("\""))
                    Next();
                if (!Match('"'))
                    throw new ArgumentException("expected '\"' at position " + pos);
                return MakeString(b.Substring(start, pos - 1));
            }
            if (Match('('))
            {
                if (Match(')'))
                    return MakeString("");
                RegExp e = ParseUnionExp();
                if (!Match(')'))
                    throw new ArgumentException("expected ')' at position " + pos);
                return e;
            }
            if ((Check(automaton) || Check(interval)) && Match('<'))
            {
                int start = pos;
                while (More() && !Peek(">"))
                    Next();
                if (!Match('>'))
                    throw new ArgumentException("expected '>' at position " + pos);
                string sb = b.Substring(start, pos - 1);
                int i = sb.IndexOf('-');
                if (i == -1)
                {
                    if (!Check(automaton))
                        throw new ArgumentException("interval syntax error at position " + (pos - 1));
                    return MakeAutomaton(sb);
                }
                if (!Check(interval))
                    throw new ArgumentException("illegal identifier at position " + (pos - 1));
                try
                {
                    if (i == 0 || i == sb.Length - 1 || i != sb.LastIndexOf('-'))
                        throw new FormatException();
                    string smin = sb.Substring(0, i);
                    string smax = sb.Substring(i + 1, sb.Length);
                    int imin = int.Parse(smin);
                    int imax = int.Parse(smax);
                    if (imin > imax)
                    {
                        int t = imin;
                        imin = imax;
                        imax = t;
                    }
                    return MakeInterval(imin, imax, smin.Length == smax.Length ? smin.Length : 0);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("interval syntax error at position " + (pos - 1));
                }
            }

            return MakeChar(ParseCharExp());
        }

        public char ParseCharExp()
        {
            Match('\\');
            return Next();
        }

        private enum Kind
        {
            RegexpUnion,
            RegexpConcatenation,
            RegexpIntersection,
            RegexpOptional,
            RegexpRepeat,
            RegexpRepeatMin,
            RegexpRepeatMinmax,
            RegexpComplement,
            RegexpChar,
            RegexpCharRange,
            RegexpAnychar,
            RegexpEmpty,
            RegexpString,
            RegexpAnystring,
            RegexpAutomaton,
            RegexpInterval
        }
    }
}