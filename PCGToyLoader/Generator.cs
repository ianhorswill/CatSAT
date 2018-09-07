#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Generator.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using System.Linq;
using CatSAT;

namespace PCGToy
{
    /// <summary>
    /// Utilities for loading PCGToy files.
    /// </summary>
    public static class Generator
    {
        private static readonly Dictionary<string,Problem> Shared = new Dictionary<string, Problem>();

        /// <summary>
        /// Returns a Problem object with the specified PCGToy file loaded into it.
        /// Calling this twice with the same argument will return the same Problem object.
        /// </summary>
        /// <param name="path">Path to PCGToy file</param>
        /// <returns>CatSAT problem containing the generator</returns>
        // ReSharper disable once UnusedMember.Global
        public static Problem SharedFromFile(string path)
        {
            if (Shared.TryGetValue(path, out Problem p))
                return p;
            return Shared[path] = FromFile(path);
        }

        /// <summary>
        /// Makes a new Problem object and loads a PCGToy file into it
        /// </summary>
        /// <param name="path">Path to PCGToy file</param>
        /// <returns>CatSAT problem containing the generator</returns>
        public static Problem FromFile(string path)
        {
            var p = new Problem(path);
            LoadFromFile(path, p);

            return p;
        }

        /// <summary>
        /// Add the information in the PCGToy file to the specified problem.
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="problem">Problem to add the contents to</param>
        /// <exception cref="FileFormatException">If the file is not a valid PCGToy file.</exception>
        public static void LoadFromFile(string path, Problem problem)
        {
            Dictionary<string, FDomain<object>> domains = new Dictionary<string, FDomain<object>>();
            Dictionary<string, FDVariable<object>> variables = new Dictionary<string, FDVariable<object>>();

            var f = System.IO.File.OpenText(path);
            while (f.Peek() >= 0)
            {
                var exp = SExpression.Read(f);
                if (!(exp is List<object> l) || l.Count == 0 || !(l[0] is string tag))
                {
                    throw new FileFormatException($"Unknown declaration {exp}");
                }

                switch (tag)
                {
                    case "domain":
                        if (l.Count < 2 || !(l[1] is string domainName))
                            throw new FileFormatException("Malformed domain declaration");
                        if (l.Count < 3)
                            throw new FileFormatException($"Domain {domainName} has no elements");
                        var elements = l.Skip(2).ToArray();
                        domains[domainName] = new FDomain<object>(domainName, elements);
                        break;

                    case "variable":
                        if (l.Count < 3
                            || l.Count > 4 
                            || !(l[1] is string varName)
                            || !(l[2] is string domain))
                            throw new FileFormatException("Malformed variable declaration");
                        if (!domains.ContainsKey(domain))
                            throw new FileFormatException(
                                $"Unknown domain name: {domain} in declaration of variable {varName}");
                        Literal c = null;

                        if (l.Count == 4)
                            c = ConditionFromSExpression(l[3], variables);
                        var v = new FDVariable<object>(varName, domains[domain], c);
                        variables[varName] = v;
                        break;

                    case "nogood":
                        problem.Inconsistent(l.Skip(1).Select(sexp => ConditionFromSExpression(sexp, variables)).ToArray());
                        break;

                    default:
                        throw new FileFormatException($"Unknown declaraction {tag}");
                }
            }
        }

        private  static Literal ConditionFromSExpression(object sexp, Dictionary<string, FDVariable<object>> variables)
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
            if (condExp == null || !(condExp[0] is string condVarName))
                throw new FileFormatException($"Malformed condition expression {sexp}");

            if (!variables.ContainsKey(condVarName))
                throw new FileFormatException($"Unknown variable name {condVarName} in condition expression {sexp}");
            var c = variables[condVarName] == condExp[1];
            return positive ? c : Language.Not(c);
        }
    }
}
