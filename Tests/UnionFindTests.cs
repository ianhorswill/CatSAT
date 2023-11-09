#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoryTests.cs" company="Ian Horswill">
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

using CatSAT;
using CatSAT.SAT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnionFindTests
    {
        [TestMethod]
        public void OneNodeTest()
        {
            var p = new Problem();
            var graph = new Graph(p, 1);
            var partition = new SpanningForest(graph);
            Assert.IsTrue(partition.ConnectedComponentCount == 1);
        }

        [TestMethod]
        public void TwoNodesTest()
        {
            var p = new Problem();
            var graph = new Graph(p, 2);
            var partition = new SpanningForest(graph);
            partition.Union(0, 1);
            Assert.IsTrue(partition.ConnectedComponentCount == 1);
        }

        [TestMethod]
        public void TenNodesTest()
        {
            var p = new Problem();
            var graph = new Graph(p, 10);
            var partition = new SpanningForest(graph);
            partition.Union(0, 1);
            partition.Union(0, 2);
            partition.Union(0, 3);
            partition.Union(0, 4);
            partition.Union(0, 5);
            partition.Union(0, 6);
            partition.Union(0, 7);
            partition.Union(0, 8);
            partition.Union(0, 9);
            Assert.IsTrue(partition.ConnectedComponentCount == 1);
        }
    }
}