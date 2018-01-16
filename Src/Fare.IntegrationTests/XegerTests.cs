using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace Fare.IntegrationTests
{
    public sealed class XegerTests
    {
        [Theory, ClassData(typeof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrect(string pattern)
        {
            var sut = new Fare.Xeger(pattern);
            var result = Enumerable.Range(1, 3).Select(i => sut.Generate()).ToArray();
            Array.ForEach(result, regex => Assert.True(Regex.IsMatch(regex, pattern)));
        }

        [Theory, ClassData(typeof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrectWithRexEngine(string pattern)
        {
            var settings = new Rex.RexSettings(pattern) { k = 1 };
            var result = Enumerable.Range(1, 3).Select(i => Rex.RexEngine.GenerateMembers(settings).Single()).ToArray();
            Array.ForEach(result, regex => Assert.True(Regex.IsMatch(regex, pattern)));
        }
    }
}