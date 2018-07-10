namespace PicoSAT
{
    /// <summary>
    /// Base class for non-Boolean variables
    /// </summary>
    public abstract class Variable
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
    }
}
