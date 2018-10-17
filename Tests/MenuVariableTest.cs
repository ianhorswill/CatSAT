using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;
using CatSAT.NonBoolean.SMT.MenuVariables;

namespace Tests
{
    [TestClass]
    public class MenuVariableTest
    {
        [TestMethod]
        public void NameTest()
        {
            string[] boys = {"john", "joe", "jim", "james"};
            var bMenu = new Menu<string>("boys", boys);
            string[] girls = {"jenny", "jane", "janet", "julie"};
            var gMenu = new Menu<string>("girls", girls);
            string[] surnames = {"jones", "johnson", "jefferson", "jackson"};
            var sMenu = new Menu<string>("surnames", surnames);

            var p = new Problem("NameTest");
            var firstName = new MenuVariable<string>("first", null, p);
            var lastName = (MenuVariable<string>)sMenu.Instantiate("last", p);
            var male = (Proposition) "male";
            var female = (Proposition) "female";

            p.Assert(firstName.In(bMenu) <= male);
            p.Assert(firstName.In(gMenu) <= female);
            p.Unique(male, female);
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Console.WriteLine(m.Model);
                Assert.IsTrue(surnames.Contains(lastName.Value(m)));
                Assert.IsTrue( (m[male] && boys.Contains(firstName.Value(m)))
                    ||
                               (m[female] && girls.Contains(firstName.Value(m))));
            }
        }
    }
}
