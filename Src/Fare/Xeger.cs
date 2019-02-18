/*
 * Copyright 2009 Wilfred Springer
 * http://github.com/moodmosaic/Fare/
 * Original Java code:
 * http://code.google.com/p/xeger/
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Text;

namespace Fare
{
    /// <summary>
    /// An object that will generate text from a regular expression. In a way, it's the opposite of a
    /// regular expression matcher: an instance of this class will produce text that is guaranteed to
    /// match the regular expression passed in.
    /// </summary>
    public class Xeger
    {
        private const RegExpSyntaxOptions _AllExceptAnyString = RegExpSyntaxOptions.All & ~RegExpSyntaxOptions.Anystring;

        private readonly Automaton _Automaton;
        private readonly IRandom _Random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xeger"/> class.
        /// </summary>
        /// <param name="regex">
        /// The regex.
        /// </param>
        /// <param name="random">
        /// The random.
        /// </param>
        public Xeger(string regex, IRandom random)
        {
            if (string.IsNullOrEmpty(regex))
            {
                throw new ArgumentNullException(nameof(regex));
            }

            regex = RemoveStartEndMarkers(regex);
            _Automaton = new RegExp(regex, _AllExceptAnyString).ToAutomaton();
            _Random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xeger"/> class.
        /// </summary>
        /// <param name="regex">
        /// The regex.
        /// </param>
        public Xeger(string regex)
            : this(regex, new HardRandom())
        {
        }

        /// <summary>
        /// Generates a random String that is guaranteed to match the regular expression passed to
        /// the constructor.
        /// </summary>
        /// <returns>
        /// </returns>
        public string Generate()
        {
            var builder = new StringBuilder();
            Generate(builder, _Automaton.Initial);
            return builder.ToString();
        }

        private static int GetRandomInt(int min, int max, IRandom random)
        {
            var maxForRandom = max - min + 1;
            return random.NextInt32(0, maxForRandom) + min;
        }

        private void AppendChoice(StringBuilder builder, Transition transition)
        {
            var c = (char)GetRandomInt(transition.Min, transition.Max, _Random);
            builder.Append(c);
        }

        private void Generate(StringBuilder builder, State state)
        {
            var transitions = state.GetSortedTransitions(true);
            if (transitions.Count == 0)
            {
                if (!state.Accept)
                {
                    throw new InvalidOperationException("state");
                }

                return;
            }

            var nroptions = state.Accept ? transitions.Count : transitions.Count - 1;
            var option = GetRandomInt(0, nroptions, _Random);
            if (state.Accept && option == 0)
            {
                // 0 is considered stop.
                return;
            }

            // Moving on to next transition.
            var transition = transitions[option - (state.Accept ? 1 : 0)];
            AppendChoice(builder, transition);
            Generate(builder, transition.To);
        }

        private string RemoveStartEndMarkers(string regExp)
        {
            if (regExp.StartsWith("^"))
            {
                regExp = regExp.Substring(1);
            }

            if (regExp.EndsWith("$"))
            {
                regExp = regExp.Substring(0, regExp.Length - 1);
            }

            return regExp;
        }
    }
}