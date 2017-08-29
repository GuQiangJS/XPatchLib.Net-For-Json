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
    internal class SimpleBasicTest : TestBase
    {
#if XUNIT
        public SimpleBasicTest(ITestOutputHelper output) : base(output)
        {
        }
#endif

        [Test]
        public void TestBasic()
        {
            string s = TestHelper.RandomString(10, false, true, true, false, string.Empty);

            string jsonContext = DivideJson(s.GetType(), string.Empty, s);
            string xmlContext = DivideXml(s.GetType(), string.Empty, s);
            AssertJsonString(xmlContext, jsonContext);

            string n = CombineJson<string>(jsonContext, string.Empty);

            Assert.AreEqual(s, n);
        }

        [Test]
        public void TestStringArray()
        {
            string[] s1 = new string[2]
            {
                TestHelper.RandomString(10, false, true, true, false, string.Empty),
                TestHelper.RandomString(10, false, true, true, false, string.Empty)
            };
            string[] s2 = new string[2]
            {
                s1[0],
                TestHelper.RandomString(10, false, true, true, false, string.Empty)
            };

            string jsonContext = DivideJson(s1.GetType(), s1, s2);
            string xmlContext = DivideXml(s1.GetType(), s1, s2);
            AssertJsonString(xmlContext, jsonContext);

            string[] s3 = CombineJson<string[]>(jsonContext, s1);

            Assert.AreEqual(s3.Length, s2.Length);
            for (int i = 0; i < s3.Length; i++)
                Assert.AreEqual(s3[i], s2[i]);
        }

        [Test]
        public void TestStringArrayAdd()
        {
            string[] s = new string[2]
            {
                TestHelper.RandomString(10, false, true, true, false, string.Empty),
                TestHelper.RandomString(10, false, true, true, false, string.Empty)
            };

            string jsonContext = DivideJson(s.GetType(), null, s);
            string xmlContext = DivideXml(s.GetType(), null, s);
            AssertJsonString(xmlContext, jsonContext);

            string[] n = CombineJson<string[]>(jsonContext, null);

            Assert.AreEqual(s.Length, n.Length);
            for (int i = 0; i < s.Length; i++)
                Assert.AreEqual(s[i], n[i]);
        }
    }
}