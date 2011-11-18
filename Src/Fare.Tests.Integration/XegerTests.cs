using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace NAutomaton.Tests.Integration
{
    public sealed class XegerTests
    {
        [Theory]
        [InlineData("[ab]{4,6}c")]
        [InlineData("(a|b)*ab")]
        [InlineData("[A-Za-z0-9]")]
        [InlineData("[A-Za-z0-9_]")]
        [InlineData("[A-Za-z]")]
        [InlineData("[ \t]")]
        [InlineData(@"[(?<=\W)(?=\w)|(?<=\w)(?=\W)]")]
        [InlineData("[\x00-\x1F\x7F]")]
        [InlineData("[0-9]")]
        [InlineData("[^0-9]")]
        [InlineData("[\x21-\x7E]")]
        [InlineData("[a-z]")]
        [InlineData("[\x20-\x7E]")]
        [InlineData(@"[\]\[!\""#$%&'()*+,./:;<=>?@\^_`{|}~-]")]
        [InlineData("[ \t\r\n\v\f]")]
        [InlineData("[^ \t\r\n\v\f]")]
        [InlineData("[A-Z]")]
        [InlineData("[A-Fa-f0-9]")]
        [InlineData("in[du]")]
        [InlineData("x[0-9A-Z]")]
        [InlineData("[^A-M]in")]
        [InlineData(".gr")]
        [InlineData(@"\(.*l")]
        [InlineData("W*in")]
        [InlineData("[xX][0-9a-z]")]
        [InlineData(@"\(\(\(ab\)*c\)*d\)\(ef\)*\(gh\)\{2\}\(ij\)*\(kl\)*\(mn\)*\(op\)*\(qr\)*")]
        public void GeneratedTextIsCorrect(string pattern)
        {
            var sut = new Xeger(pattern);
            var result = Enumerable.Range(1, 3).Select(i => sut.Generate()).ToArray();
            Array.ForEach(result, regex => Assert.True(Regex.IsMatch(regex, pattern)));
        }
    }
}