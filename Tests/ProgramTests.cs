#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProgramTests.cs" company="Ian Horswill">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;

namespace Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void AddClauseTest()
        {
            var p = new Problem("Add clause test");
            var clause = p.AddClause("x", "y");
            Assert.AreEqual("x", p.KeyOf(clause, 0).Name);
            Assert.AreEqual("y", p.KeyOf(clause, 1));
        }

        [TestMethod]
        public void EmptyProgramTest()
        {
            new Problem("Empty program test").Solve();
            Assert.IsTrue(true);
        }
    }
}
