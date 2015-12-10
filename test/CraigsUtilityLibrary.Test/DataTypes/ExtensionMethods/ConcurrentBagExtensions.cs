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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utilities.DataTypes;
using Utilities.DataTypes.Comparison;
using Xunit;

namespace Utilities.Tests.DataTypes.ExtensionMethods
{
    public class ConcurrentBagExtensions
    {
        [Fact]
        public void AddAndReturnNull()
        {
            ConcurrentBag<int> TestObject = null;
            int Item = 7;
            Assert.ThrowsAny<ArgumentNullException>(() => TestObject.AddAndReturn(Item));
        }

        [Fact]
        public void AddAndReturnTest()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            int Item = 7;
            Assert.Equal(Item, TestObject.AddAndReturn(Item));
        }

        [Fact]
        public void AddIfIEnumerable()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIf(x => x > 1, (IEnumerable<int>)new int[] { 1 }));
            Assert.True(TestObject.AddIf(x => x > 1, (IEnumerable<int>)new int[] { 1, 7 }));
            Assert.True(TestObject.AddIf(x => x > 7, (IEnumerable<int>)new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.Equal(8, TestObject.Count);
        }

        [Fact]
        public void AddIfParams()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIf(x => x > 1, new int[] { 1 }));
            Assert.True(TestObject.AddIf(x => x > 1, new int[] { 1, 7 }));
            Assert.True(TestObject.AddIf(x => x > 7, new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.Equal(8, TestObject.Count);
        }

        [Fact]
        public void AddIfParamsNull()
        {
            ConcurrentBag<int> TestObject = null;
            Assert.False(TestObject.AddIf(x => x > 1, new int[] { 1 }));
            Assert.False(TestObject.AddIf(x => x > 1, new int[] { 1, 7 }));
            Assert.False(TestObject.AddIf(x => x > 7, new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
        }

        [Fact]
        public void AddIfParamsNull2()
        {
            ConcurrentBag<int> TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.True(TestObject.AddIf(x => x > 1, new int[] { }));
            Assert.True(TestObject.AddIf(x => x > 1, null));
        }

        [Fact]
        public void AddIfTest()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIf(x => x > 1, 1));
            Assert.True(TestObject.AddIf(x => x > 1, 7));
            Assert.True(TestObject.AddIf(x => x > 7, new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.Equal(8, TestObject.Count);
        }

        [Fact]
        public void AddIfUniqueIEnumerable()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIfUnique((IEnumerable<int>)new int[] { 1 }));
            Assert.True(TestObject.AddIfUnique((IEnumerable<int>)new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueIEnumerableWithIEqualityComparer()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIfUnique(new GenericEqualityComparer<int>(), (IEnumerable<int>)new int[] { 1 }));
            Assert.True(TestObject.AddIfUnique(new GenericEqualityComparer<int>(), (IEnumerable<int>)new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueIEnumerableWithNullIEqualityComparer()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            GenericEqualityComparer<int> Comparer = null;
            Assert.False(TestObject.AddIfUnique(Comparer, (IEnumerable<int>)new int[] { 1 }));
            Assert.True(TestObject.AddIfUnique(Comparer, (IEnumerable<int>)new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueIEnumerableWithNullPredicate()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Func<int, int, bool> Comparer = null;
            Assert.False(TestObject.AddIfUnique(Comparer, (IEnumerable<int>)new int[] { 1 }));
            Assert.False(TestObject.AddIfUnique(Comparer, (IEnumerable<int>)new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueIEnumerableWithPredicate()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIfUnique((x, y) => x == y, (IEnumerable<int>)new int[] { 1 }));
            Assert.True(TestObject.AddIfUnique((x, y) => x == y, (IEnumerable<int>)new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueParamsWithIEqualityComparer()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIfUnique(new GenericEqualityComparer<int>(), new int[] { 1 }));
            Assert.True(TestObject.AddIfUnique(new GenericEqualityComparer<int>(), new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueParamsWithNullIEqualityComparer()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            GenericEqualityComparer<int> Comparer = null;
            Assert.False(TestObject.AddIfUnique(Comparer, new int[] { 1 }));
            Assert.True(TestObject.AddIfUnique(Comparer, new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueParamsWithNullPredicate()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Func<int, int, bool> Comparer = null;
            Assert.False(TestObject.AddIfUnique(Comparer, new int[] { 1 }));
            Assert.False(TestObject.AddIfUnique(Comparer, new int[] { 7 }));
        }

        [Fact]
        public void AddIfUniqueTest()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIfUnique(1));
            Assert.True(TestObject.AddIfUnique(7));
            Assert.True(TestObject.AddIfUnique(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.Equal(8, TestObject.Count);
            TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.False(TestObject.AddIfUnique((x, y) => x == y, 1));
            Assert.True(TestObject.AddIfUnique((x, y) => x == y, 7));
            Assert.True(TestObject.AddIfUnique((x, y) => x == y, new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.Equal(8, TestObject.Count);
        }

        [Fact]
        public void AddNull()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            TestObject.Add(null);
            Assert.Equal(6, TestObject.Count);
        }

        [Fact]
        public void AddNull2()
        {
            ConcurrentBag<int> TestObject = null;
            TestObject = TestObject.Add(1, 2, 3);
            Assert.Equal(3, TestObject.Count);
        }

        [Fact]
        public void AddRange()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            var Results = new ConcurrentBag<int>(TestObject.Add(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.Equal(14, Results.Count);
            Assert.Equal(14, TestObject.Count);
        }

        [Fact]
        public void Contains()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.True(TestObject.Contains(4));
            Assert.False(TestObject.Contains(-1));
        }

        [Fact]
        public void ContainsNull()
        {
            ConcurrentBag<int> TestObject = null;
            Assert.False(TestObject.Contains(4));
            Assert.False(TestObject.Contains(-1));
        }

        [Fact]
        public void RemoveNullObject()
        {
            ConcurrentBag<int> TestObject = null;
            Assert.Equal(0, TestObject.Remove(x => true).Count);
            TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.Equal(6, TestObject.Remove(null).Count);
        }

        [Fact]
        public void RemoveRange()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.Equal(0, TestObject.Remove(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Count);
        }

        [Fact]
        public void RemoveRange2()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.Equal(0, TestObject.Remove(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Count);
            TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.Equal(1, TestObject.Remove(new int[] { 1, 2, 3, 4, 5 }).Count);
            TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.Equal(1, TestObject.Remove(new int[] { 1, 2, 3, 4, 5 }).Count);
        }

        [Fact]
        public void RemoveRangeNullObject()
        {
            ConcurrentBag<int> TestObject = null;
            Assert.Equal(0, TestObject.Remove(new int[] { }).Count);
            TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            Assert.Equal(6, TestObject.Remove((IEnumerable<int>)null).Count);
        }

        [Fact]
        public void RemoveTest()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            TestObject = new ConcurrentBag<int>(TestObject.Remove((x) => x % 2 == 0));
            Assert.Equal(3, TestObject.Count);
            foreach (int Item in TestObject)
                Assert.False(Item % 2 == 0);
        }

        [Fact]
        public void RemoveTest2()
        {
            var TestObject = new ConcurrentBag<int>(new int[] { 1, 2, 3, 4, 5, 6 });
            TestObject = new ConcurrentBag<int>(TestObject.Remove((x) => x % 2 == 0));
            Assert.Equal(3, TestObject.Count);
            foreach (int Item in TestObject)
                Assert.False(Item % 2 == 0);
        }
    }
}