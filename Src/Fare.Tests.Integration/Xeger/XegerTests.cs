using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace Fare.Tests.Integration.Xeger
{
    public sealed class XegerTests
    {        
        [ClassData(typeof(RegexPatternTestCases))]
        public void GeneratedTextIsCorrect(string pattern)
        {
            var sut = new nl.flotsam.xeger.Xeger(pattern);
            var result = Enumerable.Range(1, 3).Select(i => sut.generate()).ToArray();
            Array.ForEach(result, regex => Assert.True(Regex.IsMatch(regex, pattern)));
        }
    }
}