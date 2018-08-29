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
    public class Struct : VariableType
    {
        private readonly Member[] variables;
        private readonly Action<Problem, StructVar> constrainer;

        public Struct(string name, Member[] variables, Action<Problem, StructVar> constrainer = null) : base(name)
        {
            this.variables = variables;
            this.constrainer = constrainer;
        }

        public override Variable Instantiate(object name, Problem p, Literal conditionMustBeNull = null)
        {
            if ((object)conditionMustBeNull != null)
                throw new ArgumentException("Conditions are not supported for struct variables");

            Dictionary<object, Variable> vars = new Dictionary<object, Variable>();
            foreach (var v in variables)
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

    public class StructVar : Variable
    {
        private readonly Dictionary<object, Variable> variables;
        public StructVar(object name, Problem p, Dictionary<object, Variable> vars) : base(name, p, null)
        {
            variables = vars;
        }

        public Variable this[object name] => variables[name];

        public override bool IsDefinedIn(Solution solution)
        {
            return false;
        }

        public override object UntypedValue(Solution s)
        {
            throw new NotImplementedException();
        }

        public override string ValueString(Solution s)
        {
            throw new NotImplementedException();
        }
    }
    
    public class Member
    {
        public readonly object Name;
        public readonly VariableType Type;
        public readonly object ConditionName;
        public readonly object ConditionValue;

        public Member(object name, VariableType type, object conditionName = null, object conditionValue = null)
        {
            Name = name;
            Type = type;
            ConditionName = conditionName;
            ConditionValue = conditionValue;
        }
        public Member(string name, string condition, params string[] values) :
            this(name, new FDomain<string>(name, values), ConditionNameFromString(condition), ConditionValueFromString(condition))
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

    public sealed class VariableNamePath
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
