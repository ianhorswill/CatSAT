using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSAT
{
    public class ConditionAttribute : Attribute
    {
        public readonly string VariableName;
        public readonly object VariableValue;

        public ConditionAttribute(string variableName, object variableValue)
        {
            VariableName = variableName;
            VariableValue = variableValue;
        }
    }
}
