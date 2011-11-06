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
using System.Text;

namespace NAutomaton
{
    public class Transition
    {
        private readonly char max;
        private readonly char min;
        private readonly State to;

        public Transition(char min, char max, State to)
        {
            if (max < min)
            {
                char t = max;
                max = min;
                min = t;
            }

            this.min = min;
            this.max = max;
            this.to = to;
        }

        public Transition(char c, State to)
        {
            this.min = c;
            this.max = c;
            this.to = to;
        }

        public State To
        {
            get { return this.to; }
        }

        public virtual State Destination
        {
            get { return this.to; }
        }

        public virtual char Min
        {
            get { return this.min; }
        }

        public virtual char Max
        {
            get { return this.max; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Transition;
            if (other == null)
            {
                return false;
            }

            return other.Min == this.Min
                   && other.Max == this.Max
                   && other.To == this.To;
        }

        public override int GetHashCode()
        {
            return this.Min*2 + this.Max*3;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            Transition.AppendCharString(this.Min, b);
            if (this.Min != this.Max)
            {
                b.Append("-");
                Transition.AppendCharString(this.Max, b);
            }
            b.Append(" -> ").Append(to.Number);
            return b.ToString();
        }

        internal static void AppendCharString(char c, StringBuilder b)
        {
            if (c >= 0x21 && c <= 0x7e && c != '\\' && c != '"')
            {
                b.Append(c);
            }
            else
            {
                b.Append("\\u");

                string s = string.Format("{0:X}", Convert.ToInt32(c));

                if (c < 0x10)
                {
                    b.Append("000").Append(s);
                }
                else if (c < 0x100)
                {
                    b.Append("00").Append(s);
                }
                else if (c < 0x1000)
                {
                    b.Append("0").Append(s);
                }
                else
                {
                    b.Append(s);
                }
            }
        }

        internal void AppendDot(StringBuilder b)
        {
            b.Append(" -> ").Append(to.Number).Append(" [label=\"");
            Transition.AppendCharString(min, b);
            if (min != max)
            {
                b.Append("-");
                Transition.AppendCharString(max, b);
            }
            b.Append("\"]\n");
        }
    }
}