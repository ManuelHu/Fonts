// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.IO;
using SixLabors.Fonts.Unicode;
using Xunit;

namespace SixLabors.Fonts.Tests.Unicode
{
    /// <summary>
    /// Tests adapted from:
    /// https://github.com/unicode-org/icu/blob/master/icu4j/main/tests/core/src/com/ibm/icu/dev/test/util/Trie2Test.java
    /// </summary>
    public class UnicodeTrieBuilderTests
    {
        private static readonly TestRange[] TestRanges1 = new TestRange[]
        {
             new TestRange(0,        0,        0,      false),
             new TestRange(0,        0x40,     0,      false),
             new TestRange(0x40,     0xe7,     0x1234, false),
             new TestRange(0xe7,     0x3400,   0,      false),
             new TestRange(0x3400,   0x9fa6,   0x6162, false),
             new TestRange(0x9fa6,   0xda9e,   0x3132, false),
             new TestRange(0xdada,   0xeeee,   0x87ff, false),
             new TestRange(0xeeee,   0x11111,  1,      false),
             new TestRange(0x11111,  0x44444,  0x6162, false),
             new TestRange(0x44444,  0x60003,  0,      false),
             new TestRange(0xf0003,  0xf0004,  0xf,    false),
             new TestRange(0xf0004,  0xf0006,  0x10,   false),
             new TestRange(0xf0006,  0xf0007,  0x11,   false),
             new TestRange(0xf0007,  0xf0040,  0x12,   false),
             new TestRange(0xf0040,  0x110000, 0,      false)
        };

        private static readonly CheckValue[] CheckValues1 = new CheckValue[]
        {
            new CheckValue(0,        0),
            new CheckValue(0x40,     0),
            new CheckValue(0xe7,     0x1234),
            new CheckValue(0x3400,   0),
            new CheckValue(0x9fa6,   0x6162),
            new CheckValue(0xda9e,   0x3132),
            new CheckValue(0xdada,   0),
            new CheckValue(0xeeee,   0x87ff),
            new CheckValue(0x11111,  1),
            new CheckValue(0x44444,  0x6162),
            new CheckValue(0xf0003,  0),
            new CheckValue(0xf0004,  0xf),
            new CheckValue(0xf0006,  0x10),
            new CheckValue(0xf0007,  0x11),
            new CheckValue(0xf0040,  0x12),
            new CheckValue(0x110000, 0),
        };

        private static readonly TestRange[] TestRanges2 = new TestRange[]
        {
            new TestRange(0,        0,        0,      false),
            new TestRange(0x21,     0x7f,     0x5555, true),
            new TestRange(0x2f800,  0x2fedc,  0x7a,   true),
            new TestRange(0x72,     0xdd,     3,      true),
            new TestRange(0xdd,     0xde,     4,      false),
            new TestRange(0x201,    0x240,    6,      true),  // 3 consecutive blocks with the same pattern but
            new TestRange(0x241,    0x280,    6,      true),  // discontiguous value ranges, testing utrie2_enum()
            new TestRange(0x281,    0x2c0,    6,      true),
            new TestRange(0x2f987,  0x2fa98,  5,      true),
            new TestRange(0x2f777,  0x2f883,  0,      true),
            new TestRange(0x2f900,  0x2ffaa,  1,      false),
            new TestRange(0x2ffaa,  0x2ffab,  2,      true),
            new TestRange(0x2ffbb,  0x2ffc0,  7,      true),
        };

        private static readonly CheckValue[] CheckValues2 = new CheckValue[]
        {
            new CheckValue(0,        0),
            new CheckValue(0x21,     0),
            new CheckValue(0x72,     0x5555),
            new CheckValue(0xdd,     3),
            new CheckValue(0xde,     4),
            new CheckValue(0x201,    0),
            new CheckValue(0x240,    6),
            new CheckValue(0x241,    0),
            new CheckValue(0x280,    6),
            new CheckValue(0x281,    0),
            new CheckValue(0x2c0,    6),
            new CheckValue(0x2f883,  0),
            new CheckValue(0x2f987,  0x7a),
            new CheckValue(0x2fa98,  5),
            new CheckValue(0x2fedc,  0x7a),
            new CheckValue(0x2ffaa,  1),
            new CheckValue(0x2ffab,  2),
            new CheckValue(0x2ffbb,  0),
            new CheckValue(0x2ffc0,  7),
            new CheckValue(0x110000, 0),
        };

        private static readonly TestRange[] TestRanges3 = new TestRange[]
        {
            new TestRange(0,        0,        9, false), // non-zero initial value.
            new TestRange(0x31,     0xa4,     1, false),
            new TestRange(0x3400,   0x6789,   2, false),
            new TestRange(0x8000,   0x89ab,   9, true),
            new TestRange(0x9000,   0xa000,   4, true),
            new TestRange(0xabcd,   0xbcde,   3, true),
            new TestRange(0x55555,  0x110000, 6, true), // highStart<U+ffff with non-initialValue
            new TestRange(0xcccc,   0x55555,  6, true),
        };

        private static readonly CheckValue[] CheckValues3 = new CheckValue[]
        {
            new CheckValue(0,        9),  // non-zero initialValue
            new CheckValue(0x31,     9),
            new CheckValue(0xa4,     1),
            new CheckValue(0x3400,   9),
            new CheckValue(0x6789,   2),
            new CheckValue(0x9000,   9),
            new CheckValue(0xa000,   4),
            new CheckValue(0xabcd,   9),
            new CheckValue(0xbcde,   3),
            new CheckValue(0xcccc,   9),
            new CheckValue(0x110000, 6),
        };

        private static readonly TestRange[] TestRanges4 = new TestRange[]
        {
            new TestRange(0,        0,        3, false), // Only the element with the initial value.
        };

        private static readonly CheckValue[] CheckValues4 = new CheckValue[]
        {
            new CheckValue(0,        3),
            new CheckValue(0x110000, 3),
        };

        private static readonly TestRange[] TestRanges5 = new TestRange[]
        {
            new TestRange(0,        0,        3,  false), // Initial value = 3
            new TestRange(0,        0x110000, 5, true)
        };

        private static readonly CheckValue[] CheckValues5 = new CheckValue[]
        {
            new CheckValue(0,        3),
            new CheckValue(0x110000, 5),
        };

        [Fact]
        public void Set()
        {
            var builder = new UnicodeTrieBuilder(10, 666);
            builder.Set(0x4567, 99);

            Assert.Equal(10u, builder.Get(0x4566));
            Assert.Equal(99u, builder.Get(0x4567));
            Assert.Equal(666u, builder.Get(-1));
            Assert.Equal(666u, builder.Get(0x110000));
        }

        [Fact]
        public void SetRange()
        {
            var builder = new UnicodeTrieBuilder(10, 666);
            builder.SetRange(13, 6666, 7788, false);
            builder.SetRange(6000, 7000, 9900, true);

            Assert.Equal(10u, builder.Get(12));
            Assert.Equal(7788u, builder.Get(13));
            Assert.Equal(7788u, builder.Get(5999));
            Assert.Equal(9900u, builder.Get(6000));
            Assert.Equal(9900u, builder.Get(7000));
            Assert.Equal(10u, builder.Get(7001));
            Assert.Equal(666u, builder.Get(0x110000));
        }

        [Fact]
        public void SetRangeCompacted()
        {
            var builder = new UnicodeTrieBuilder(10, 666);
            builder.SetRange(13, 6666, 7788, false);
            builder.SetRange(6000, 7000, 9900, true);

            UnicodeTrie trie = builder.Freeze();
            Assert.Equal(10u, trie.Get(12));
            Assert.Equal(7788u, trie.Get(13));
            Assert.Equal(7788u, trie.Get(5999));
            Assert.Equal(9900u, trie.Get(6000));
            Assert.Equal(9900u, trie.Get(7000));
            Assert.Equal(10u, trie.Get(7001));
            Assert.Equal(666u, trie.Get(0x110000));
        }

        [Fact]
        public void SetRangeSerialized()
        {
            var builder = new UnicodeTrieBuilder(10, 666);
            builder.SetRange(13, 6666, 7788, false);
            builder.SetRange(6000, 7000, 9900, true);

            using var ms = new MemoryStream();
            builder.Freeze().Save(ms);
            ms.Position = 0;

            var trie = new UnicodeTrie(ms);
            Assert.Equal(10u, trie.Get(12));
            Assert.Equal(7788u, trie.Get(13));
            Assert.Equal(7788u, trie.Get(5999));
            Assert.Equal(9900u, trie.Get(6000));
            Assert.Equal(9900u, trie.Get(7000));
            Assert.Equal(10u, trie.Get(7001));
            Assert.Equal(666u, trie.Get(0x110000));
        }

        public struct TestRange
        {
            public TestRange(int start, int end, uint value, bool overwrite)
            {
                this.Start = start;
                this.End = end;
                this.Value = value;
                this.Overwrite = overwrite;
            }

            public int Start { get; }

            public int End { get; }

            public uint Value { get; }

            public bool Overwrite { get; }
        }

        public struct CheckValue
        {
            public CheckValue(int codePoint, uint value)
            {
                this.CodePoint = codePoint;
                this.Value = value;
            }

            public int CodePoint { get; }

            public uint Value { get; }
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { TestRanges1, CheckValues1 };
            yield return new object[] { TestRanges2, CheckValues2 };
            yield return new object[] { TestRanges3, CheckValues3 };
            yield return new object[] { TestRanges4, CheckValues4 };
            yield return new object[] { TestRanges5, CheckValues5 };
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void RunRangeChecks(TestRange[] testRanges, CheckValue[] checkValues)
        {
            uint initialValue = testRanges[0].Value;
            const uint errorValue = 0x0bad;

            var builder = new UnicodeTrieBuilder(initialValue, errorValue);
            for (int i = 1; i < testRanges.Length; i++)
            {
                TestRange r = testRanges[i];
                builder.SetRange(r.Start, r.End - 1, r.Value, r.Overwrite);
            }

            UnicodeTrie frozen = builder.Freeze();

            int cp = 0;
            for (int i = 0; i < checkValues.Length; i++)
            {
                CheckValue v = checkValues[i];

                while (cp < v.CodePoint)
                {
                    Assert.Equal(v.Value, builder.Get(cp));
                    Assert.Equal(v.Value, frozen.Get((uint)cp));
                    cp++;
                }
            }
        }
    }
}
