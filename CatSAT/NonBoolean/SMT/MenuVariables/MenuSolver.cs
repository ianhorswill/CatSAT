using System;
using System.Collections.Generic;

namespace CatSAT.NonBoolean.SMT.MenuVariables
{
    class MenuSolver<T> : TheorySolver
    {
        internal readonly List<MenuVariable<T>> variables = new List<MenuVariable<T>>();
        internal readonly List<MenuProposition<T>> Propositions = new List<MenuProposition<T>>();

        public override bool Solve(Solution s)
        {
            foreach (var v in variables)
                v.MenuInclusions.Clear();

            foreach (var p in Propositions)
            {
                if (!s[p])
                    continue;

                var c = (Call)p.Name;
                switch (c.Name)
                {
                    case "In":
                        var v = (MenuVariable<T>)c.Args[0];
                        var m = (Menu<T>) c.Args[1];
                        v.MenuInclusions.Add(m);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown MenuProposition type {c.Name}");
                }
            }

            // Assign variables
            foreach (var v in variables)
            {
                if (ReferenceEquals(v.Condition, null) || s[v.Condition])
                    if (!SelectValue(v))
                        return false;
            }

            return true;
        }

        private static bool SelectValue(MenuVariable<T> v)
        {
            if (v.BaseMenu != null)
                v.CurrentValue = v.BaseMenu.Selections.RandomElement();
            else
            {
                if (v.MenuInclusions.Count == 0)
                    return false;
                v.CurrentValue = v.MenuInclusions.RandomElement().Selections.RandomElement();
            }

            return true;
        }
    }
}
