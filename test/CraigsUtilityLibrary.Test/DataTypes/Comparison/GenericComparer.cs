﻿/*
Copyright (c) 2014 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using Utilities.DataTypes.Comparison;
using Xunit;

namespace Utilities.Tests.DataTypes.Comparison
{
    public class GenericComparer
    {
        [Fact]
        public void Compare()
        {
            var Comparer = new GenericComparer<string>();
            Assert.Equal(0, Comparer.Compare("A", "A"));
            Assert.Equal(-1, Comparer.Compare("A", "B"));
            Assert.Equal(1, Comparer.Compare("B", "A"));
        }

        [Fact]
        public void CompareNullNonValueType()
        {
            var Comparer = new GenericComparer<string>();
            Assert.Equal(0, Comparer.Compare(null, null));
            Assert.Equal(-1, Comparer.Compare(null, "B"));
            Assert.Equal(-1, Comparer.Compare("B", null));
        }

        [Fact]
        public void CompareValueType()
        {
            var Comparer = new GenericComparer<int>();
            Assert.Equal(0, Comparer.Compare(0, 0));
            Assert.Equal(-1, Comparer.Compare(0, 1));
            Assert.Equal(1, Comparer.Compare(1, 0));
        }
    }
}