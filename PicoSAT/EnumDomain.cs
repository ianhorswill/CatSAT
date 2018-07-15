using System.Diagnostics;

namespace PicoSAT
{
    public class EnumDomain<T> : FDomain<T>
    {
        private EnumDomain() : base(typeof(T).Name, (T[]) typeof(T).GetEnumValues())
        {
            Debug.Assert(typeof(T).IsEnum);
        }

        public static readonly EnumDomain<T> Singleton = new EnumDomain<T>();
    }
}
