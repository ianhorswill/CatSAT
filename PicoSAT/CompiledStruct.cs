using System.Reflection;

namespace PicoSAT
{
    public abstract class CompiledStruct : Variable
    {
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

        public override object UntypedValue(Solution s)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsDefinedIn(Solution solution)
        {
            return false;
        }
    }
}
