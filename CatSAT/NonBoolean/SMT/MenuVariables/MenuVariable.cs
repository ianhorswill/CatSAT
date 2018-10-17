using System;
using System.Collections.Generic;

namespace CatSAT.NonBoolean.SMT.MenuVariables
{
    public class MenuVariable<T> : TheoryVariable<T>
    {
        public MenuVariable(object name, Menu<T> baseMenu, Problem problem, Literal condition = null) : base(name, problem, condition)
        {
            BaseMenu = baseMenu;
            problem.GetSolver<MenuSolver<T>>().variables.Add(this);
        }

        internal List<Menu<T>> MenuInclusions = new List<Menu<T>>();
        internal List<Menu<T>> MenuExclusions = new List<Menu<T>>();
        internal List<T> Inclusions = new List<T>();
        internal List<T> Exclusions = new List<T>();

        public Proposition In(Menu<T> menu)
        {
            return Problem.GetSpecialProposition<MenuProposition<T>>(Call.FromArgs(Problem, "In", this, menu));
        }

        public readonly Menu<T> BaseMenu;

        internal T CurrentValue;

        /// <inheritdoc />
        public override Domain<T> Domain => BaseMenu;

        /// <inheritdoc />
        public override T Value(Solution s)
        {
            return CurrentValue;
        }

        /// <inheritdoc />
        public override T PredeterminedValue()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetPredeterminedValue(T newValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
