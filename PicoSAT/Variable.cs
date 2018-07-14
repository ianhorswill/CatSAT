using System;

namespace PicoSAT
{
    /// <summary>
    /// Base class for non-Boolean variables
    /// </summary>
#pragma warning disable 660,661
    public abstract class Variable
#pragma warning restore 660,661
    {
        /// <summary>
        /// Object that names the variable
        /// </summary>
        public readonly object Name;

        /// <summary>
        /// Problem to which this variable is attached
        /// </summary>
        public readonly Problem Problem;
        /// <summary>
        /// Condition under which this variable is defined.
        /// If true or if Condition is null, the variable must have a value.
        /// If false, it must not have a value.
        /// </summary>
        public readonly Literal Condition;

        /// <summary>
        /// Make a new variable
        /// </summary>
        /// <param name="name">"Name" for the variable (arbitrary object)</param>
        /// <param name="problem">Problem of which this variable is a part</param>
        /// <param name="condition">Condition under which this variable is to have a value; if null, it's always defined.</param>
        protected Variable(object name, Problem problem, Literal condition)
        {
            Name = name;
            Problem = problem;
            Condition = condition;
            problem.AddVariable(this);
        }

        public virtual string ValueString(Solution s)
        {
            if (IsDefinedIn(s))
                return $"{Name}={UntypedValue(s)}";
            return $"{Name} undefined";
        }

        public abstract object UntypedValue(Solution s);

        public abstract bool IsDefinedIn(Solution solution);

        public override string ToString()
        {
            return Name.ToString();
        }

        /// <summary>
        /// A Proposition asserting that the variable has a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator ==(Variable var, object value)
        {
            return var.EqualityProposition(value);
        }

        /// <summary>
        /// A Proposition asserting that the variable does not have a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator !=(Variable var, object value)
        {
            return Language.Not(var == value);
        }



        public virtual Literal EqualityProposition(object vConditionValue)
        {
            throw new NotImplementedException();
        }
    }
}
