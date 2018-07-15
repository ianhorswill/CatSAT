using System.Diagnostics;

namespace PicoSAT
{
    public class EnumVariable<T> : FDVariable<T>
    {
        public EnumVariable(object name, Literal condition = null) : base(name, EnumDomain<T>.Singleton, condition)
        { }

        /// <summary>
        /// A Proposition asserting that the variable has a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator ==(EnumVariable<T> var, T value)
        {
            Debug.Assert((object)var != null, nameof(var) + " != null");
            return var.valuePropositions[var.domain.IndexOf(value)];
        }

        /// <summary>
        /// A Proposition asserting that the variable does not have a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator !=(EnumVariable<T> var, T value)
        {
            Debug.Assert((object)var != null, nameof(var) + " != null");
            return Language.Not(var.valuePropositions[var.domain.IndexOf(value)]);
        }
    }
}
