#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompiledStruct.cs" company="Ian Horswill">
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
using System.Reflection;

namespace CatSAT
{
    /// <summary>
    /// A CatSAT variable whose value is a normal C# class whose fields are automatically filled in by CatSAT.
    /// </summary>
    public abstract class CompiledStruct : Variable
    {
        /// <inheritdoc />
        protected CompiledStruct(object name, Problem p, Literal condition) : base(name, p, condition)
        {
            foreach (var f in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (f.FieldType.IsSubclassOf(typeof(Variable)))
                {
                    var vName = new VariableNamePath(f.Name, name);
                    var c = FieldCondition(f);

                    object value;

                    var a = f.GetCustomAttribute<DomainAttribute>();
                    if (a != null)
                    {
                        value = a.Domain.Instantiate(vName, p, c);
                    }
                    else
                    {
                        value = f.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null,
                            new object[] {vName, c});
                    }

                    f.SetValue(this, value);
                }
            }
        }

        private Literal FieldCondition(FieldInfo f)
        {
            var ca = f.GetCustomAttribute<ConditionAttribute>();
            if (ca != null)
            {
                var v = (Variable) GetType().GetField(ca.VariableName).GetValue(this);
                return v == ca.VariableValue;
            }

            return null;
        }

        /// <inheritdoc />
        public override object UntypedValue(Solution s)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override bool IsDefinedIn(Solution solution)
        {
            return false;
        }
    }
}
