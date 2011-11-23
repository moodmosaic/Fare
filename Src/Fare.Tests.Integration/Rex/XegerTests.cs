using System;
using System.Linq;
using System.Text.RegularExpressions;
using Rex;
using Xunit;
using Xunit.Extensions;

namespace Fare.Tests.Integration.Rex
{
    public class XegerTests
    {
        [Theory]
        [ClassData(typeof(RegexPatternTestCases))]
        public void GeneratedTextWithRexIsCorrect(string pattern)
        {
            var settings = new RexSettings(pattern);
            var result = Enumerable.Range(1, 3).Select(i => RexEngine.GenerateMembers(settings).Single()).ToArray();
            Array.ForEach(result, regex => Assert.True(Regex.IsMatch(regex, pattern)));
        }
    }
}