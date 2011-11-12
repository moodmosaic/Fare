/**
 * Copyright 2009 Wilfred Springer
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
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace NAutomaton.Tests.Integration.Java
{
    public sealed class XegerTests
    {
        [Theory]
        [InlineData("[ab]{4,6}c")]
        [InlineData("(a|b)*ab")]
        public void ShouldGenerateTextCorrectly(string regex)
        {
            var generator = new Xeger(regex);
            for (int i = 0; i < 100; i++)
            {
                string text = generator.Generate();
                Assert.True(Regex.IsMatch(text, regex));
            }
        }

        #region Nested type: Xeger

        /// <summary>
        /// An object that will generate text from a regular expression. In a way, 
        /// it's the opposite of a regular expression matcher: an instance of this class
        /// will produce text that is guaranteed to match the regular expression passed in.
        /// </summary>
        private sealed class Xeger
        {
            private readonly dk.brics.automaton.Automaton automaton;
            private readonly Random random;

            /// <summary>
            /// Initializes a new instance of the <see cref="Xeger"/> class.
            /// </summary>
            /// <param name="regex">The regex.</param>
            /// <param name="random">The random.</param>
            private Xeger(String regex, Random random)
            {
                if (string.IsNullOrEmpty(regex))
                {
                    throw new ArgumentNullException("regex");
                }

                if (random == null)
                {
                    throw new ArgumentNullException("random");
                }

                automaton = new dk.brics.automaton.RegExp(regex).toAutomaton();
                this.random = random;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Xeger"/> class.
            /// </summary>
            /// <param name="regex">The regex.</param>
            public Xeger(string regex)
                : this(regex, new Random())
            {
            }

            /// <summary>
            /// Generates a random String that is guaranteed to match the regular expression passed to the constructor.
            /// </summary>
            /// <returns></returns>
            public string Generate()
            {
                var builder = new StringBuilder();
                Generate(builder, automaton.getInitialState());
                return builder.ToString();
            }

            private void Generate(StringBuilder builder, dk.brics.automaton.State state)
            {
                java.util.List transitions = state.getSortedTransitions(true);
                if (transitions.size() == 0)
                {
                    if (!state.isAccept())
                    {
                        throw new InvalidOperationException("state");
                    }
                    return;
                }

                int nroptions = state.isAccept() ? transitions.size() : transitions.size() - 1;
                int option = GetRandomInt(0, nroptions, random);
                if (state.isAccept() && option == 0)
                {
                    // 0 is considered stop.
                    return;
                }

                // Moving on to next transition.
                var transition = (dk.brics.automaton.Transition) transitions.get(option - (state.isAccept() ? 1 : 0));
                AppendChoice(builder, transition);
                Generate(builder, transition.getDest());
            }

            private void AppendChoice(StringBuilder builder, dk.brics.automaton.Transition transition)
            {
                var c = (char) GetRandomInt(transition.getMin(), transition.getMax(), random);
                builder.Append(c);
            }

            private static int GetRandomInt(int min, int max, Random random)
            {
                int dif = max - min;
                double number = random.NextDouble();
                return min + (int) Math.Round(number*dif);
            }
        }

        #endregion
    }
}