using System.Collections;
using System.Collections.Generic;

namespace Fare.Tests.Integration
{
    public class RegexPatternTestCases : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "[ab]{4,6}" };
            yield return new object[] { "[ab]{4,6}c" };
            yield return new object[] { "(a|b)*ab" };
            yield return new object[] { "[A-Za-z0-9]" };
            yield return new object[] { "[A-Za-z0-9_]" };
            yield return new object[] { "[A-Za-z]" };
            yield return new object[] { "[ \t]" };
            yield return new object[] { @"[(?<=\W)(?=\w)|(?<=\w)(?=\W)]" };
            yield return new object[] { "[\x00-\x1F\x7F]" };
            yield return new object[] { "[0-9]" };
            yield return new object[] { "[^0-9]" };
            yield return new object[] { "[\x21-\x7E]" };
            yield return new object[] { "[a-z]" };
            yield return new object[] { "[\x20-\x7E]" };
            yield return new object[] { @"[\]\[!\""#$%&'()*+,./:;<=>?@\^_`{|}~-]" };
            yield return new object[] { "[ \t\r\n\v\f]" };
            yield return new object[] { "[^ \t\r\n\v\f]" };
            yield return new object[] { "[A-Z]" };
            yield return new object[] { "[A-Fa-f0-9]" };
            yield return new object[] { "in[du]" };
            yield return new object[] { "x[0-9A-Z]" };
            yield return new object[] { "[^A-M]in" };
            yield return new object[] { ".gr" };
            yield return new object[] { @"\(.*l" };
            yield return new object[] { "W*in" };
            yield return new object[] { "[xX][0-9a-z]" };
            yield return new object[] { @"\(\(\(ab\)*c\)*d\)\(ef\)*\(gh\)\{2\}\(ij\)*\(kl\)*\(mn\)*\(op\)*\(qr\)*" };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
