// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

#if NUNIT
using NUnit.Framework;

#elif XUNIT
using Test = Xunit.FactAttribute;
using Assert = XPatchLib.Json.UnitTest.XUnitAssert;
using Xunit.Abstractions;

#endif

namespace XPatchLib.Json.UnitTest
{
    [TestFixture]
    public class SimpleObjectTest : TestBase
    {
#if XUNIT
        public SimpleObjectTest(ITestOutputHelper output) : base(output)
        {
        }
#endif
        [Test]
        public void TestSimpleObject()
        {
            SimpleClass s = new SimpleClass {S = TestHelper.RandomString(10, false, true, true, false, string.Empty)};

            string jsonContext = DivideJson(s.GetType(), null, s);
            string xmlContext = DivideXml(s.GetType(), null, s);
            AssertJsonString(xmlContext, jsonContext);

            SimpleClass n = CombineJson<SimpleClass>(jsonContext, null);

            Assert.AreEqual(s.S, n.S);
        }

        [Test]
        public void TestSimpleObjectArray()
        {
            SimpleClass[] s1 =
            {
                new SimpleClass {S = TestHelper.RandomString(10, false, true, true, false, string.Empty)},
                new SimpleClass {S = TestHelper.RandomString(10, false, true, true, false, string.Empty)}
            };

            SimpleClass[] s2 =
            {
                s1[0],
                new SimpleClass {S = TestHelper.RandomString(10, false, true, true, false, string.Empty)}
            };

            string jsonContext = DivideJson(s1.GetType(), s1, s2);
            string xmlContext = DivideXml(s1.GetType(), s1, s2);
            AssertJsonString(xmlContext, jsonContext);

            SimpleClass[] s3 = CombineJson<SimpleClass[]>(jsonContext, s1);

            Assert.AreEqual(s3.Length, s2.Length);
            for (int i = 0; i < s3.Length; i++)
                Assert.AreEqual(s3[i].S, s2[i].S);
        }

        [Test]
        public void TestSimpleObjectArrayAdd()
        {
            SimpleClass[] s =
            {
                new SimpleClass {S = TestHelper.RandomString(10, false, true, true, false, string.Empty)},
                new SimpleClass {S = TestHelper.RandomString(10, false, true, true, false, string.Empty)}
            };

            string jsonContext = DivideJson(s.GetType(), null, s);
            string xmlContext = DivideXml(s.GetType(), null, s);
            AssertJsonString(xmlContext, jsonContext);

            SimpleClass[] n = CombineJson<SimpleClass[]>(jsonContext, null);

            Assert.AreEqual(s.Length, n.Length);
            for (int i = 0; i < s.Length; i++)
                Assert.AreEqual(n[i].S, s[i].S);
        }
    }
}