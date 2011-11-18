using System.Collections.Generic;
using System.Linq;

namespace Fare
{
    internal sealed class ListEqualityComparer<T> : IEqualityComparer<List<T>>
    {
        public bool Equals(List<T> x, List<T> y)
        {
            if (x.Count != y.Count)
            {
                return false;
            }

            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<T> obj)
        {
            return obj.Sum(o => o.GetHashCode());
        }
    }
}
