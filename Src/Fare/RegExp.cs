/*
 * dk.brics.automaton
 *
 * Copyright (c) 2001-2011 Anders Moeller
 * All rights reserved.
 * http://github.com/moodmosaic/Fare/
 * Original Java code:
 * http://www.brics.dk/automaton/
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
using System.Linq;
using System.Text;

namespace Fare
{
    /// <summary>
    /// Regular Expression extension to Automaton.
    /// </summary>
    public class RegExp
    {
        private readonly string _B;
        private readonly RegExpSyntaxOptions _Flags;

        private static bool _AllowMutation;

        private char _C;
        private int _Digits;
        private RegExp _Exp1;
        private RegExp _Exp2;
        private char _From;
        private Kind _Kind;
        private int _Max;
        private int _Min;
        private int _Pos;
        private string _S;
        private char _To;

        /// <summary>
        /// Prevents a default instance of the <see cref="RegExp"/> class from being created.
        /// </summary>
        private RegExp()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegExp"/> class from a string.
        /// </summary>
        /// <param name="s">
        /// A string with the regular expression.
        /// </param>
        public RegExp(string s)
            : this(s, RegExpSyntaxOptions.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegExp"/> class from a string.
        /// </summary>
        /// <param name="s">
        /// A string with the regular expression.
        /// </param>
        /// <param name="syntaxFlags">
        /// Boolean 'or' of optional syntax constructs to be enabled.
        /// </param>
        public RegExp(string s, RegExpSyntaxOptions syntaxFlags)
        {
            _B = s;
            _Flags = syntaxFlags;
            RegExp e;
            if (s.Length == 0)
            {
                e = MakeString(string.Empty);
            }
            else
            {
                e = ParseUnionExp();
                if (_Pos < _B.Length)
                {
                    throw new ArgumentException("end-of-string expected at position " + _Pos);
                }
            }

            _Kind = e._Kind;
            _Exp1 = e._Exp1;
            _Exp2 = e._Exp2;
            _S = e._S;
            _C = e._C;
            _Min = e._Min;
            _Max = e._Max;
            _Digits = e._Digits;
            _From = e._From;
            _To = e._To;
            _B = null;
        }

        /// <summary>
        /// Constructs new
        /// <code>
        /// Automaton
        /// </code>
        /// from this
        /// <code>
        /// RegExp
        /// </code>
        /// . Same as
        /// <code>
        /// toAutomaton(null)
        /// </code>
        /// (empty automaton map).
        /// </summary>
        /// <returns>
        /// </returns>
        public Automaton ToAutomaton() => ToAutomatonAllowMutate(null, null, true);

        /// <summary>
        /// Constructs new
        /// <code>
        /// Automaton
        /// </code>
        /// from this
        /// <code>
        /// RegExp
        /// </code>
        /// . Same as
        /// <code>
        /// toAutomaton(null,minimize)
        /// </code>
        /// (empty automaton map).
        /// </summary>
        /// <param name="minimize">
        /// if set to <c>true</c> [minimize].
        /// </param>
        /// <returns>
        /// </returns>
        public Automaton ToAutomaton(bool minimize) => ToAutomatonAllowMutate(null, null, minimize);

        /// <summary>
        /// Constructs new
        /// <code>
        /// Automaton
        /// </code>
        /// from this
        /// <code>
        /// RegExp
        /// </code>
        /// . The constructed automaton is minimal and deterministic and has no transitions to dead states.
        /// </summary>
        /// <param name="automatonProvider">
        /// The provider of automata for named identifiers.
        /// </param>
        /// <returns>
        /// </returns>
        public Automaton ToAutomaton(IAutomatonProvider automatonProvider) => ToAutomatonAllowMutate(null, automatonProvider, true);

        /// <summary>
        /// Constructs new
        /// <code>
        /// Automaton
        /// </code>
        /// from this
        /// <code>
        /// RegExp
        /// </code>
        /// . The constructed automaton has no transitions to dead states.
        /// </summary>
        /// <param name="automatonProvider">
        /// The provider of automata for named identifiers.
        /// </param>
        /// <param name="minimize">
        /// if set to <c>true</c> the automaton is minimized and determinized.
        /// </param>
        /// <returns>
        /// </returns>
        public Automaton ToAutomaton(IAutomatonProvider automatonProvider, bool minimize) => ToAutomatonAllowMutate(null, automatonProvider, minimize);

        /// <summary>
        /// Constructs new
        /// <code>
        /// Automaton
        /// </code>
        /// from this
        /// <code>
        /// RegExp
        /// </code>
        /// . The constructed automaton is minimal and deterministic and has no transitions to dead states.
        /// </summary>
        /// <param name="automata">
        /// The a map from automaton identifiers to automata.
        /// </param>
        /// <returns>
        /// </returns>
        public Automaton ToAutomaton(IDictionary<string, Automaton> automata) => ToAutomatonAllowMutate(automata, null, true);

        /// <summary>
        /// Constructs new
        /// <code>
        /// Automaton
        /// </code>
        /// from this
        /// <code>
        /// RegExp
        /// </code>
        /// . The constructed automaton has no transitions to dead states.
        /// </summary>
        /// <param name="automata">
        /// The map from automaton identifiers to automata.
        /// </param>
        /// <param name="minimize">
        /// if set to <c>true</c> the automaton is minimized and determinized.
        /// </param>
        /// <returns>
        /// </returns>
        public Automaton ToAutomaton(IDictionary<string, Automaton> automata, bool minimize) => ToAutomatonAllowMutate(automata, null, minimize);

        /// <summary>
        /// Sets or resets allow mutate flag. If this flag is set, then automata construction uses
        /// mutable automata, which is slightly faster but not thread safe.
        /// </summary>
        /// <param name="flag">
        /// if set to <c>true</c> the flag is set.
        /// </param>
        /// <returns>
        /// The previous value of the flag.
        /// </returns>
        public static bool SetAllowMutate(bool flag)
        {
            var @bool = _AllowMutation;
            _AllowMutation = flag;
            return @bool;
        }

        /// <inheritdoc/>
        ///
        public override string ToString() => ToStringBuilder(new StringBuilder()).ToString();

        /// <summary>
        /// Returns the set of automaton identifiers that occur in this regular expression.
        /// </summary>
        /// <returns>
        /// The set of automaton identifiers that occur in this regular expression.
        /// </returns>
        public HashSet<string> GetIdentifiers()
        {
            var set = new HashSet<string>();
            GetIdentifiers(set);
            return set;
        }

        private static RegExp MakeUnion(RegExp exp1, RegExp exp2)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpUnion,
                _Exp1 = exp1,
                _Exp2 = exp2
            };
            return r;
        }

        private static RegExp MakeIntersection(RegExp exp1, RegExp exp2)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpIntersection,
                _Exp1 = exp1,
                _Exp2 = exp2
            };
            return r;
        }

        private static RegExp MakeConcatenation(RegExp exp1, RegExp exp2)
        {
            if ((exp1._Kind == Kind.RegexpChar || exp1._Kind == Kind.RegexpString)
                && (exp2._Kind == Kind.RegexpChar || exp2._Kind == Kind.RegexpString))
            {
                return MakeString(exp1, exp2);
            }

            var r = new RegExp
            {
                _Kind = Kind.RegexpConcatenation
            };
            if (exp1._Kind == Kind.RegexpConcatenation
                && (exp1._Exp2._Kind == Kind.RegexpChar || exp1._Exp2._Kind == Kind.RegexpString)
                && (exp2._Kind == Kind.RegexpChar || exp2._Kind == Kind.RegexpString))
            {
                r._Exp1 = exp1._Exp1;
                r._Exp2 = MakeString(exp1._Exp2, exp2);
            }
            else if ((exp1._Kind == Kind.RegexpChar || exp1._Kind == Kind.RegexpString)
                     && exp2._Kind == Kind.RegexpConcatenation
                     && (exp2._Exp1._Kind == Kind.RegexpChar || exp2._Exp1._Kind == Kind.RegexpString))
            {
                r._Exp1 = MakeString(exp1, exp2._Exp1);
                r._Exp2 = exp2._Exp2;
            }
            else
            {
                r._Exp1 = exp1;
                r._Exp2 = exp2;
            }

            return r;
        }

        private static RegExp MakeRepeat(RegExp exp)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpRepeat,
                _Exp1 = exp
            };
            return r;
        }

        private static RegExp MakeRepeat(RegExp exp, int min)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpRepeatMin,
                _Exp1 = exp,
                _Min = min
            };
            return r;
        }

        private static RegExp MakeRepeat(RegExp exp, int min, int max)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpRepeatMinMax,
                _Exp1 = exp,
                _Min = min,
                _Max = max
            };
            return r;
        }

        private static RegExp MakeOptional(RegExp exp)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpOptional,
                _Exp1 = exp
            };
            return r;
        }

        private static RegExp MakeChar(char @char)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpChar,
                _C = @char
            };
            return r;
        }

        private static RegExp MakeInterval(int min, int max, int digits)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpInterval,
                _Min = min,
                _Max = max,
                _Digits = digits
            };
            return r;
        }

        private static RegExp MakeAutomaton(string s)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpAutomaton,
                _S = s
            };
            return r;
        }

        private static RegExp MakeAnyString()
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpAnyString
            };
            return r;
        }

        private static RegExp MakeEmpty()
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpEmpty
            };
            return r;
        }

        private static RegExp MakeAnyPrintableASCIIChar() => MakeCharRange(' ', '~');

        private static RegExp MakeCharRange(char from, char to)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpCharRange,
                _From = from,
                _To = to
            };
            return r;
        }

        private static RegExp MakeComplement(RegExp exp)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpComplement,
                _Exp1 = exp
            };
            return r;
        }

        private static RegExp MakeString(string @string)
        {
            var r = new RegExp
            {
                _Kind = Kind.RegexpString,
                _S = @string
            };
            return r;
        }

        private static RegExp MakeString(RegExp exp1, RegExp exp2)
        {
            var sb = new StringBuilder();
            if (exp1._Kind == Kind.RegexpString)
            {
                sb.Append(exp1._S);
            }
            else
            {
                sb.Append(exp1._C);
            }

            if (exp2._Kind == Kind.RegexpString)
            {
                sb.Append(exp2._S);
            }
            else
            {
                sb.Append(exp2._C);
            }

            return MakeString(sb.ToString());
        }

        private Automaton ToAutomatonAllowMutate(
            IDictionary<string, Automaton> automata,
            IAutomatonProvider automatonProvider,
            bool minimize)
        {
            var @bool = false;
            if (_AllowMutation)
            {
                @bool = SetAllowMutate(true); // This is not thead safe.
            }

            var a = ToAutomaton(automata, automatonProvider, minimize);
            if (_AllowMutation)
            {
                SetAllowMutate(@bool);
            }

            return a;
        }

        private Automaton ToAutomaton(
            IDictionary<string, Automaton> automata,
            IAutomatonProvider automatonProvider,
            bool minimize)
        {
            IList<Automaton> list;
            Automaton a = null;
            switch (_Kind)
            {
                case Kind.RegexpUnion:
                    list = new List<Automaton>();
                    FindLeaves(_Exp1, Kind.RegexpUnion, list, automata, automatonProvider, minimize);
                    FindLeaves(_Exp2, Kind.RegexpUnion, list, automata, automatonProvider, minimize);
                    a = BasicOperations.Union(list);
                    a.Minimize();
                    break;

                case Kind.RegexpConcatenation:
                    list = new List<Automaton>();
                    FindLeaves(_Exp1, Kind.RegexpConcatenation, list, automata, automatonProvider, minimize);
                    FindLeaves(_Exp2, Kind.RegexpConcatenation, list, automata, automatonProvider, minimize);
                    a = BasicOperations.Concatenate(list);
                    a.Minimize();
                    break;

                case Kind.RegexpIntersection:
                    a = _Exp1.ToAutomaton(automata, automatonProvider, minimize)
                        .Intersection(_Exp2.ToAutomaton(automata, automatonProvider, minimize));
                    a.Minimize();
                    break;

                case Kind.RegexpOptional:
                    a = _Exp1.ToAutomaton(automata, automatonProvider, minimize).Optional();
                    a.Minimize();
                    break;

                case Kind.RegexpRepeat:
                    a = _Exp1.ToAutomaton(automata, automatonProvider, minimize).Repeat();
                    a.Minimize();
                    break;

                case Kind.RegexpRepeatMin:
                    a = _Exp1.ToAutomaton(automata, automatonProvider, minimize).Repeat(_Min);
                    a.Minimize();
                    break;

                case Kind.RegexpRepeatMinMax:
                    a = _Exp1.ToAutomaton(automata, automatonProvider, minimize).Repeat(_Min, _Max);
                    a.Minimize();
                    break;

                case Kind.RegexpComplement:
                    a = _Exp1.ToAutomaton(automata, automatonProvider, minimize).Complement();
                    a.Minimize();
                    break;

                case Kind.RegexpChar:
                    a = BasicAutomata.MakeChar(_C);
                    break;

                case Kind.RegexpCharRange:
                    a = BasicAutomata.MakeCharRange(_From, _To);
                    break;

                case Kind.RegexpAnyChar:
                    a = BasicAutomata.MakeAnyChar();
                    break;

                case Kind.RegexpEmpty:
                    a = BasicAutomata.MakeEmpty();
                    break;

                case Kind.RegexpString:
                    a = BasicAutomata.MakeString(_S);
                    break;

                case Kind.RegexpAnyString:
                    a = BasicAutomata.MakeAnyString();
                    break;

                case Kind.RegexpAutomaton:
                    Automaton aa = null;
                    if (automata != null)
                    {
                        automata.TryGetValue(_S, out aa);
                    }

                    if (aa == null && automatonProvider != null)
                    {
                        try
                        {
                            aa = automatonProvider.GetAutomaton(_S);
                        }
                        catch (IOException e)
                        {
                            throw new ArgumentException(string.Empty, e);
                        }
                    }

                    if (aa == null)
                    {
                        throw new ArgumentException("'" + _S + "' not found");
                    }

                    a = aa.Clone(); // Always clone here (ignore allowMutate).
                    break;

                case Kind.RegexpInterval:
                    a = BasicAutomata.MakeInterval(_Min, _Max, _Digits);
                    break;
            }

            return a;
        }

        private void FindLeaves(
            RegExp exp,
            Kind regExpKind,
            IList<Automaton> list,
            IDictionary<string, Automaton> automata,
            IAutomatonProvider automatonProvider,
            bool minimize)
        {
            if (exp._Kind == regExpKind)
            {
                FindLeaves(exp._Exp1, regExpKind, list, automata, automatonProvider, minimize);
                FindLeaves(exp._Exp2, regExpKind, list, automata, automatonProvider, minimize);
            }
            else
            {
                list.Add(exp.ToAutomaton(automata, automatonProvider, minimize));
            }
        }

        private StringBuilder ToStringBuilder(StringBuilder sb)
        {
            switch (_Kind)
            {
                case Kind.RegexpUnion:
                    sb.Append("(");
                    _Exp1.ToStringBuilder(sb);
                    sb.Append("|");
                    _Exp2.ToStringBuilder(sb);
                    sb.Append(")");
                    break;

                case Kind.RegexpConcatenation:
                    _Exp1.ToStringBuilder(sb);
                    _Exp2.ToStringBuilder(sb);
                    break;

                case Kind.RegexpIntersection:
                    sb.Append("(");
                    _Exp1.ToStringBuilder(sb);
                    sb.Append("&");
                    _Exp2.ToStringBuilder(sb);
                    sb.Append(")");
                    break;

                case Kind.RegexpOptional:
                    sb.Append("(");
                    _Exp1.ToStringBuilder(sb);
                    sb.Append(")?");
                    break;

                case Kind.RegexpRepeat:
                    sb.Append("(");
                    _Exp1.ToStringBuilder(sb);
                    sb.Append(")*");
                    break;

                case Kind.RegexpRepeatMin:
                    sb.Append("(");
                    _Exp1.ToStringBuilder(sb);
                    sb.Append("){").Append(_Min).Append(",}");
                    break;

                case Kind.RegexpRepeatMinMax:
                    sb.Append("(");
                    _Exp1.ToStringBuilder(sb);
                    sb.Append("){").Append(_Min).Append(",").Append(_Max).Append("}");
                    break;

                case Kind.RegexpComplement:
                    sb.Append("~(");
                    _Exp1.ToStringBuilder(sb);
                    sb.Append(")");
                    break;

                case Kind.RegexpChar:
                    sb.Append("\\").Append(_C);
                    break;

                case Kind.RegexpCharRange:
                    sb.Append("[\\").Append(_From).Append("-\\").Append(_To).Append("]");
                    break;

                case Kind.RegexpAnyChar:
                    sb.Append(".");
                    break;

                case Kind.RegexpEmpty:
                    sb.Append("#");
                    break;

                case Kind.RegexpString:
                    sb.Append("\"").Append(_S).Append("\"");
                    break;

                case Kind.RegexpAnyString:
                    sb.Append("@");
                    break;

                case Kind.RegexpAutomaton:
                    sb.Append("<").Append(_S).Append(">");
                    break;

                case Kind.RegexpInterval:
                    var s1 = Convert.ToDecimal(_Min).ToString();
                    var s2 = Convert.ToDecimal(_Max).ToString();
                    sb.Append("<");
                    if (_Digits > 0)
                    {
                        for (var i = s1.Length; i < _Digits; i++)
                        {
                            sb.Append('0');
                        }
                    }

                    sb.Append(s1).Append("-");
                    if (_Digits > 0)
                    {
                        for (var i = s2.Length; i < _Digits; i++)
                        {
                            sb.Append('0');
                        }
                    }

                    sb.Append(s2).Append(">");
                    break;
            }

            return sb;
        }

        private void GetIdentifiers(HashSet<string> set)
        {
            switch (_Kind)
            {
                case Kind.RegexpUnion:
                case Kind.RegexpConcatenation:
                case Kind.RegexpIntersection:
                    _Exp1.GetIdentifiers(set);
                    _Exp2.GetIdentifiers(set);
                    break;

                case Kind.RegexpOptional:
                case Kind.RegexpRepeat:
                case Kind.RegexpRepeatMin:
                case Kind.RegexpRepeatMinMax:
                case Kind.RegexpComplement:
                    _Exp1.GetIdentifiers(set);
                    break;

                case Kind.RegexpAutomaton:
                    set.Add(_S);
                    break;
            }
        }

        private RegExp ParseUnionExp()
        {
            var e = ParseInterExp();
            if (Match('|'))
            {
                e = MakeUnion(e, ParseUnionExp());
            }

            return e;
        }

        private bool Match(char @char)
        {
            if (_Pos >= _B.Length)
            {
                return false;
            }

            if (_B[_Pos] == @char)
            {
                _Pos++;
                return true;
            }

            return false;
        }

        private RegExp ParseInterExp()
        {
            var e = ParseConcatExp();
            if (Check(RegExpSyntaxOptions.Intersection) && Match('&'))
            {
                e = MakeIntersection(e, ParseInterExp());
            }

            return e;
        }

        private bool Check(RegExpSyntaxOptions flag) => (_Flags & flag) != 0;

        private RegExp ParseConcatExp()
        {
            var e = ParseRepeatExp();
            if (More() && !Peek(")|") && (!Check(RegExpSyntaxOptions.Intersection) || !Peek("&")))
            {
                e = MakeConcatenation(e, ParseConcatExp());
            }

            return e;
        }

        private bool More() => _Pos < _B.Length;

        private bool Peek(string @string) => More() && @string.IndexOf(_B[_Pos]) != -1;

        private RegExp ParseRepeatExp()
        {
            var e = ParseComplExp();
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
                    var start = _Pos;
                    while (Peek("0123456789"))
                    {
                        Next();
                    }

                    if (start == _Pos)
                    {
                        throw new ArgumentException("integer expected at position " + _Pos);
                    }

                    var n = int.Parse(_B[start.._Pos]);
                    var m = -1;
                    if (Match(','))
                    {
                        start = _Pos;
                        while (Peek("0123456789"))
                        {
                            Next();
                        }

                        if (start != _Pos)
                        {
                            m = int.Parse(_B[start.._Pos]);
                        }
                    }
                    else
                    {
                        m = n;
                    }

                    if (!Match('}'))
                    {
                        throw new ArgumentException("expected '}' at position " + _Pos);
                    }

                    e = m == -1 ? MakeRepeat(e, n) : MakeRepeat(e, n, m);
                }
            }

            return e;
        }

        private char Next()
        {
            if (!More())
            {
                throw new InvalidOperationException("unexpected end-of-string");
            }

            return _B[_Pos++];
        }

        private RegExp ParseComplExp()
        {
            if (Check(RegExpSyntaxOptions.Complement) && Match('~'))
            {
                return MakeComplement(ParseComplExp());
            }

            return ParseCharClassExp();
        }

        private RegExp ParseCharClassExp()
        {
            if (Match('['))
            {
                var negate = false;
                if (Match('^'))
                {
                    negate = true;
                }

                var e = ParseCharClasses();
                if (negate)
                {
                    e = ExcludeChars(e, MakeAnyPrintableASCIIChar());
                }

                if (!Match(']'))
                {
                    throw new ArgumentException("expected ']' at position " + _Pos);
                }

                return e;
            }

            return ParseSimpleExp();
        }

        private RegExp ParseSimpleExp()
        {
            if (Match('.'))
            {
                return MakeAnyPrintableASCIIChar();
            }

            if (Check(RegExpSyntaxOptions.Empty) && Match('#'))
            {
                return MakeEmpty();
            }

            if (Check(RegExpSyntaxOptions.Anystring) && Match('@'))
            {
                return MakeAnyString();
            }

            if (Match('"'))
            {
                var start = _Pos;
                while (More() && !Peek("\""))
                {
                    Next();
                }

                if (!Match('"'))
                {
                    throw new ArgumentException("expected '\"' at position " + _Pos);
                }

                return MakeString(_B[start.._Pos - 1]);
            }

            if (Match('('))
            {
                if (Match('?'))
                {
                    SkipNonCapturingSubpatternExp();
                }

                if (Match(')'))
                {
                    return MakeString(string.Empty);
                }

                var e = ParseUnionExp();
                if (!Match(')'))
                {
                    throw new ArgumentException("expected ')' at position " + _Pos);
                }

                return e;
            }

            if ((Check(RegExpSyntaxOptions.Automaton) || Check(RegExpSyntaxOptions.Interval)) && Match('<'))
            {
                var start = _Pos;
                while (More() && !Peek(">"))
                {
                    Next();
                }

                if (!Match('>'))
                {
                    throw new ArgumentException("expected '>' at position " + _Pos);
                }

                var str = _B[start.._Pos - 1];
                var i = str.IndexOf('-');
                if (i == -1)
                {
                    if (!Check(RegExpSyntaxOptions.Automaton))
                    {
                        throw new ArgumentException("interval syntax error at position " + (_Pos - 1));
                    }

                    return MakeAutomaton(str);
                }

                if (!Check(RegExpSyntaxOptions.Interval))
                {
                    throw new ArgumentException("illegal identifier at position " + (_Pos - 1));
                }

                try
                {
                    if (i == 0 || i == str.Length - 1 || i != str.LastIndexOf('-'))
                    {
                        throw new FormatException();
                    }

                    var smin = str[..i];
                    var smax = str[i + 1..];
                    var imin = int.Parse(smin);
                    var imax = int.Parse(smax);
                    var numdigits = smin.Length == smax.Length ? smin.Length : 0;
                    if (imin > imax)
                    {
                        var t = imin;
                        imin = imax;
                        imax = t;
                    }

                    return MakeInterval(imin, imax, numdigits);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("interval syntax error at position " + (_Pos - 1));
                }
            }

            if (Match('\\'))
            {
                // Escaped '\' character.
                if (Match('\\'))
                {
                    return MakeChar('\\');
                }

                bool inclusion;

                // Digits.
                if ((inclusion = Match('d')) || Match('D'))
                {
                    var digitChars = MakeCharRange('0', '9');
                    return inclusion ? digitChars : ExcludeChars(digitChars, MakeAnyPrintableASCIIChar());
                }

                // Whitespace chars only.
                if ((inclusion = Match('s')) || Match('S'))
                {
                    // Do not add line breaks, as usually RegExp is single line.
                    var whitespaceChars = MakeUnion(MakeChar(' '), MakeChar('\t'));
                    return inclusion ? whitespaceChars : ExcludeChars(whitespaceChars, MakeAnyPrintableASCIIChar());
                }

                // Word character. Range is [A-Za-z0-9_]
                if ((inclusion = Match('w')) || Match('W'))
                {
                    var ranges = new[] { MakeCharRange('A', 'Z'), MakeCharRange('a', 'z'), MakeCharRange('0', '9') };
                    var wordChars = ranges.Aggregate(MakeChar('_'), MakeUnion);

                    return inclusion ? wordChars : ExcludeChars(wordChars, MakeAnyPrintableASCIIChar());
                }
            }

            return MakeChar(ParseCharExp());
        }

        private void SkipNonCapturingSubpatternExp()
        {
            RegExpMatchingOptions.All().Any(Match);
            Match(':');
        }

        private char ParseCharExp()
        {
            Match('\\');
            return Next();
        }

        private RegExp ParseCharClasses()
        {
            var e = ParseCharClass();
            while (More() && !Peek("]"))
            {
                e = MakeUnion(e, ParseCharClass());
            }

            return e;
        }

        private RegExp ParseCharClass()
        {
            var @char = ParseCharExp();
            if (Match('-'))
            {
                if (Peek("]"))
                {
                    return MakeUnion(MakeChar(@char), MakeChar('-'));
                }

                return MakeCharRange(@char, ParseCharExp());
            }

            return MakeChar(@char);
        }

        private static RegExp ExcludeChars(RegExp exclusion, RegExp allChars) => MakeIntersection(allChars, MakeComplement(exclusion));

        private enum Kind
        {
            RegexpUnion,
            RegexpConcatenation,
            RegexpIntersection,
            RegexpOptional,
            RegexpRepeat,
            RegexpRepeatMin,
            RegexpRepeatMinMax,
            RegexpComplement,
            RegexpChar,
            RegexpCharRange,
            RegexpAnyChar,
            RegexpEmpty,
            RegexpString,
            RegexpAnyString,
            RegexpAutomaton,
            RegexpInterval
        }
    }
}