#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Struct.cs" company="Ian Horswill">
// Copyright (C) 2018 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// The VariableType (essentially the domain) for CatSAT variables containing other variables
    /// </summary>
    public class Struct : VariableType
    {
        private readonly Member[] memberVariables;
        private readonly Action<Problem, StructVar> constrainer;

        /// <summary>
        /// Creates a new Struct  with the specified member variables
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="memberVariables">Variables to be created inside this variable</param>
        /// <param name="constrainer">Procedure to add constraints to a newly created instance, if desired.</param>
        public Struct(string name, Member[] memberVariables, Action<Problem, StructVar> constrainer = null) : base(name)
        {
            this.memberVariables = memberVariables;
            this.constrainer = constrainer;
        }

        /// <inheritdoc />
        public override Variable Instantiate(object name, Problem p, Literal conditionMustBeNull = null)
        {
            if ((object)conditionMustBeNull != null)
                throw new ArgumentException("Conditions are not supported for struct variables");

            Dictionary<object, Variable> vars = new Dictionary<object, Variable>();
            foreach (var v in memberVariables)
            {
                Literal c = null;
                if (v.ConditionName != null)
                {
                    c = vars[v.ConditionName].EqualityProposition(v.ConditionValue);
                }
                vars[v.Name] = v.Type.Instantiate(new VariableNamePath(v.Name, name), p, c);
            }

            var sv = new StructVar(name, p, vars);
            // ReSharper disable once UseNullPropagation
            if (constrainer != null)
                constrainer(p, sv);
            return sv;
        }
    }

    /// <summary>
    /// A CatSAT Variable that constains over Variables
    /// </summary>
    public class StructVar : Variable
    {
        /// <summary>
        /// Dictionary holding the member variables
        /// </summary>
        private readonly Dictionary<object, Variable> variables;
        internal StructVar(object name, Problem p, Dictionary<object, Variable> vars) : base(name, p, null)
        {
            variables = vars;
        }

        /// <summary>
        /// Returns the member variable with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public Variable this[object name] => variables[name];

        /// <inheritdoc />
        public override bool IsDefinedIn(Solution solution)
        {
            return false;
        }

        /// <summary>
        /// Not implemented.  This operation doesn't make sense for a StructVar.
        /// </summary>
        /// <param name="s">Solution from which to get value of the variable.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override object UntypedValue(Solution s)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.  This operation doesn't make sense for a StructVar.
        /// </summary>
        /// <param name="s">Solution from which to get value of the variable.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override string ValueString(Solution s)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Represents CatSAT Variable that to be added to all instances of a given CatSAT Struct.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Name of the Variable
        /// </summary>
        public readonly object Name;
        /// <summary>
        /// Type of the Variable
        /// </summary>
        public readonly VariableType Type;
        /// <summary>
        /// Optional: specifies a Variable to check to see if this Variable should be defined in a given Solution.
        /// </summary>
        public readonly object ConditionName;
        /// <summary>
        /// Value to compare against ConditionName
        /// </summary>
        public readonly object ConditionValue;

        /// <summary>
        /// Represents CatSAT Variable that to be added to all instances of a given CatSAT Struct.
        /// </summary>
        /// <param name="name">Name to be given to the Variable inside the Struct</param>
        /// <param name="type">CatSAT VariableType of the Variable</param>
        /// <param name="conditionName">(optional) another variable to check to test whether this variable should be defined in a given solution</param>
        /// <param name="conditionValue">Value to test against ConditionName</param>
        public Member(object name, VariableType type, object conditionName = null, object conditionValue = null)
        {
            Name = name;
            Type = type;
            ConditionName = conditionName;
            ConditionValue = conditionValue;
        }

        /// <summary>
        /// Represents CatSAT FDVariable that to be added to all instances of a given CatSAT Struct.
        /// This will create a new FDomain for the value that has the same name as the variable.
        /// </summary>
        /// <param name="name">Name to be given to the Variable inside the Struct</param>
        /// <param name="condition">String encoding of a condition to test whether this variable should be defined in a given solution, e.g. "othervariable=value"</param>
        /// <param name="domainElements">Elements of the variables domain.</param>
        public Member(string name, string condition, params string[] domainElements) :
            this(name, new FDomain<string>(name, domainElements), ConditionNameFromString(condition), ConditionValueFromString(condition))
        { }

        private static object ConditionNameFromString(string condition)
        {
            return condition?.Split('=')[0].Trim();
        }

        private static object ConditionValueFromString(string condition)
        {
            return condition?.Split('=')[1].Trim();
        }
    }

    internal sealed class VariableNamePath
    {
        public readonly object Name;
        public readonly object Parent;

        public VariableNamePath(object name, object parent)
        {
            Name = name;
            Parent = parent;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            BuildName(b);
            return b.ToString();
        }

        private void BuildName(StringBuilder b)
        {
            switch (Parent)
            {
                case null:
                    break;

                case VariableNamePath p:
                    p.BuildName(b);
                    b.Append('.');
                    break;

                default:
                    b.Append(Parent);
                    b.Append('.');
                    break;
            }

            b.Append(Name);
        }
    }
}
