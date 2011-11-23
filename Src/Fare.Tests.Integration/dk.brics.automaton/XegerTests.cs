using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace Fare.Tests.Integration.Dk.Brics.Automaton
{
    public sealed class XegerTests
    {
        [Theory]
        [ClassData(typeof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrect(string pattern)
        {
            var sut = new Xeger(pattern);
            var result = Enumerable.Range(1, 3).Select(i => sut.Generate()).ToArray();
            Array.ForEach(result, regex => Assert.True(Regex.IsMatch(regex, pattern)));
        }

        #region Nested type: Xeger

        /// <summary>
        /// An object that will generate text from a regular expression. In a way, 
        /// it's the opposite of a regular expression matcher: an instance of this class
        /// will produce text that is guaranteed to match the regular expression passed in.
        /// </summary>
        private sealed class Xeger
        {
            private readonly global::dk.brics.automaton.Automaton automaton;
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

                automaton = new global::dk.brics.automaton.RegExp(regex).toAutomaton();
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

            private static int GetRandomInt(int min, int max, Random random)
            {
                int dif = max - min;
                double number = random.NextDouble();
                return min + (int)Math.Round(number * dif);
            }

            private void Generate(StringBuilder builder, global::dk.brics.automaton.State state)
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
                var transition = (global::dk.brics.automaton.Transition)transitions.get(option - (state.isAccept() ? 1 : 0));
                AppendChoice(builder, transition);
                Generate(builder, transition.getDest());
            }

            private void AppendChoice(StringBuilder builder, global::dk.brics.automaton.Transition transition)
            {
                var c = (char)GetRandomInt(transition.getMin(), transition.getMax(), random);
                builder.Append(c);
            }
        }

        #endregion
    }
}