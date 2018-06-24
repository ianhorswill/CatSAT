using System.Collections.Generic;
using System.Linq;
using PicoSAT;

namespace PCGToy
{
    public class PCGProblem
    {
        public readonly Dictionary<string, object[]> Domains = new Dictionary<string, object[]>();
        public readonly Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();
        public readonly List<Condition[]> Nogoods = new List<Condition[]>();

        public Problem Problem;

        public bool IsDirty;

        public void Changed()
        {
            Problem = null;
            RebuildProblem();
            IsDirty = true;
        }

        private void RebuildProblem()
        {
            Problem = new Problem();
            foreach (var pair in Variables)
                pair.Value.CompileToProblem(Problem);

            foreach (var n in Nogoods)
                Problem.Inconsistent(n.Select(c => c.Literal));

            Solve();
        }

        public void Solve()
        {
            if (Problem == null)
                RebuildProblem();
            var s = Problem.Solve();
            foreach (var pair in Variables)
                pair.Value.UpdateFromSolution(s);
        }

        public static PCGProblem FromFile(string path)
        {
            var p = new PCGProblem();
            p.LoadFromFile(path);

            return p;
        }

        public void LoadFromFile(string path)
        {
            Domains.Clear();
            Variables.Clear();
            Nogoods.Clear();
            var f = System.IO.File.OpenText(path);
            while (f.Peek() >= 0)
            {
                var exp = SExpression.Read(f);
                if (!(exp is List<object> l) || l.Count == 0 || !(l[0] is string tag))
                {
                    throw new FileFormatException($"Unknown declaration {exp}");
                }
                else
                {
                    switch (tag)
                    {
                        case "domain":
                            if (l.Count < 2 || !(l[1] is string domainName))
                                throw new FileFormatException("Malformed domain declaration");
                            if (l.Count < 3)
                                throw new FileFormatException($"Domain {domainName} has no elements");
                            var elements = l.Skip(2).ToArray();
                            Domains[domainName] = elements;
                            break;

                        case "variable":
                            if (l.Count < 3
                                || l.Count > 4 
                                || !(l[1] is string varName)
                                || !(l[2] is string domain))
                                throw new FileFormatException("Malformed variable declaration");
                            if (!Domains.ContainsKey(domain))
                                throw new FileFormatException(
                                    $"Unknown domain name: {domain} in declaration of variable {varName}");
                            Condition c = null;

                            if (l.Count == 4)
                                c = ConditionFromSExpression(l[3]);
                            Variables[varName] = new Variable(varName, this, domain, c);
                            break;

                        case "nogood":
                            Nogoods.Add(l.Skip(1).Select(ConditionFromSExpression).ToArray());
                            break;

                        default:
                            throw new FileFormatException($"Unknown declaraction {tag}");
                    }
                }
            }
            Changed();
            IsDirty = false;
        }

        public void WriteToFile(string path)
        {
            List<string> code = new List<string>();
            void Add(params object[] exp)
            {
                code.Add(SExpression.ToSExpression(exp));
            }

            HashSet<Variable> writtenVars = new HashSet<Variable>();
            void WriteVar(Variable v)
            {
                if (writtenVars.Contains(v))
                    return;

                if (v.Condition != null)
                {
                    WriteVar(v.Condition.Variable);
                    Add("variable", v.Name, v.DomainName, ConditionToSExpression(v.Condition));
                }
                else
                    Add("variable", v.Name, v.DomainName);

                writtenVars.Add(v);
            }

            foreach (var pair in Domains)
                Add(new object[] {"domain", pair.Key }.Concat(pair.Value).ToArray());

            foreach (var pair in Variables)
            {
                var v = pair.Value;
                WriteVar(v);
            }

            foreach (var nogood in Nogoods)
                Add(new object[] { "nogood" }.Concat(nogood.Select(ConditionToSExpression)).ToArray());

            System.IO.File.WriteAllLines(path, code);
            IsDirty = false;
        }

        private static object[] ConditionToSExpression(Condition c)
        {
            var result = new[] { c.Variable.Name, c.Value };
            return c.Positive ? result : new object[] {"not", result};
        }

        public Condition ConditionFromSExpression(object sexp)
        {
            bool positive = true;
            if (!(sexp is List<object> condExp)
                || condExp.Count != 2)
                throw new FileFormatException($"Malformed condition expression {sexp}");
            if (condExp[0].Equals("not"))
            {
                condExp = condExp[1] as List<object>;
                if (condExp != null && condExp.Count != 2)
                    throw new FileFormatException($"Malformed condition expression {sexp}");
                positive = false;
            }
            if (!(condExp[0] is string condVarName))
                throw new FileFormatException($"Malformed condition expression {sexp}");

            if (!Variables.ContainsKey(condVarName))
                throw new FileFormatException($"Unknown variable name {condVarName} in condition expression {sexp}");
            return new Condition(positive, Variables[condVarName], condExp[1]);
        }

        public void AddDomain(string domainName)
        {
            Domains[domainName] = new object[0];
            Changed();
        }

        public void AddVariable(string varName, string domainName,
            string conditionVarName, object conditionValue)
        {
            Condition condition = null;
            if (!string.IsNullOrEmpty(conditionVarName))
            {
                var v = Variables[conditionVarName];
                condition = new Condition(true, v, conditionValue);
            }
            Variables[varName] = new Variable(varName, this, domainName, condition);
            Changed();
        }

        public void AddNogood(IEnumerable<Variable> vars)
        {
            Nogoods.Add(vars.Select(v => new Condition(true, v, v.Value)).ToArray());
            Changed();
        }
    }
}
