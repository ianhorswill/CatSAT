#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuVariableTest.cs" company="Ian Horswill">
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
