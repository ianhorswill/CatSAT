using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSAT
{
    /// <summary>
    /// A finite domain defined by a list.
    /// </summary>
    /// <typeparam name="T">Base type of the domain</typeparam>
    public class FDomain<T> : Domain<T>
    {
        public readonly IList<T> Values;

        public FDomain(string name, params T[] values) : this(name, (IList<T>)values)
        { }

        public FDomain(string name, IList<T> values) : base(name)
        {
            Values = values;
        }

        public int IndexOf(T value)
        {
            var index = Values.IndexOf(value);
            if (index < 0)
                throw new ArgumentException($"{value} is not an element of the domain {Name}");
            return index;
        }

        public T this[int i] => Values[i];
    }
}
