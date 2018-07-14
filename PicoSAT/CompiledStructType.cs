using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PicoSAT
{
    public class CompiledStructType : VariableType
    {
        private readonly Type type;
        public CompiledStructType(Type t) : base(t.Name)
        {
            Debug.Assert(t.IsSubclassOf(typeof(CompiledStruct)));
            type = t;
        }

        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            return (Variable) type.InvokeMember(null, BindingFlags.CreateInstance, null, null,
                new[] {name, p, condition});
        }
    }
}
