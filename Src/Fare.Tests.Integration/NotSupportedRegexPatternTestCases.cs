using System.Collections;
using System.Collections.Generic;

namespace Fare.Tests.Integration
{
    public class NotSupportedRegexPatternTestCases : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "[" };
            yield return new object[] { @"(?\[Test\]|\[Foo\]|\[Bar\])?(?:-)?(?\[[()a-zA-Z0-9_\s]+\])?(?:-)?(?\[[a-zA-Z0-9_\s]+\])?(?:-)?(?\[[a-zA-Z0-9_\s]+\])?(?:-)?(?\[[a-zA-Z0-9_\s]+\])?" };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
