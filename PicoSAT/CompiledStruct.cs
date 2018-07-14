using System.Reflection;

namespace PicoSAT
{
    public abstract class CompiledStruct : Variable
    {
        protected CompiledStruct(object name, Problem p, Literal condition) : base(name, p, condition)
        {
            foreach (var f in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var a = f.GetCustomAttribute<DomainAttribute>();
                if (a != null)
                {
                    Literal c = null;
                    var ca = f.GetCustomAttribute<ConditionAttribute>();
                    if (ca != null)
                    {
                        var v = (Variable) GetType().GetField(ca.VariableName).GetValue(this);
                        c = v == ca.VariableValue;
                    }
                    f.SetValue(this, a.Domain.Instantiate(new VariableNamePath(f.Name, name), p, c));
                }
            }
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
