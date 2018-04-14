using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicoSAT;

namespace Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void AddClauseTest()
        {
            var p = new Problem();
            var clause = p.AddClause("x", "y");
            Assert.AreEqual("x", p.KeyOf(clause, 0).Name);
            Assert.AreEqual("y", p.KeyOf(clause, 1));
        }

        [TestMethod]
        public void EmptyProgramTest()
        {
            new Problem().Solve();
            Assert.IsTrue(true);
        }
    }
}
