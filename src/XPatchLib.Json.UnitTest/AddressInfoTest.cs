// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
    public class AddressInfoTest : TestBase
    {
#if XUNIT
        public AddressInfoTest(ITestOutputHelper output) : base(output)
        {
        }
#endif
        [Test]
        public void TestAddressInfoArray()
        {
            List<AddressInfo> s1 = new List<AddressInfo>();
            s1.Add(
                new AddressInfo
                {
                    AddressId = Guid.NewGuid(),
                    City = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Country = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Phone = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    State = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Zip = TestHelper.RandomString(10, false, true, true, false, string.Empty)
                });
            s1.Add(
                new AddressInfo
                {
                    AddressId = Guid.NewGuid(),
                    City = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Country = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Phone = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    State = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Zip = TestHelper.RandomString(10, false, true, true, false, string.Empty)
                });

            List<AddressInfo> s2 = new List<AddressInfo>();
            s2.Add(s1[0].Clone() as AddressInfo);
            s2.Add(s1[1].Clone() as AddressInfo);
            s2[0].City = string.Concat(s2[0].City, TestHelper.RandomString(10, false, true, true, false, string.Empty));
            s2.RemoveAt(1);
            s2.Add(
                new AddressInfo
                {
                    AddressId = Guid.NewGuid(),
                    City = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Country = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Phone = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    State = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                    Zip = TestHelper.RandomString(10, false, true, true, false, string.Empty)
                });

            string jsonContext = DivideJson(s1.GetType(), s1, s2);
            string xmlContext = DivideXml(s1.GetType(), s1, s2);
            AssertJsonString(xmlContext, jsonContext);

            List<AddressInfo> s3 = CombineJson<List<AddressInfo>>(jsonContext, s1);

            Assert.AreEqual(s3.Count, s2.Count);
            for (int i = 0; i < s3.Count; i++)
                Assert.AreEqual(s3[i], s2[i]);
        }


        [Test]
        public void TestAddressInfoEdit()
        {
            AddressInfo s1 = new AddressInfo
            {
                AddressId = Guid.NewGuid(),
                City = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                Country = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                Phone = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                State = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                Zip = TestHelper.RandomString(10, false, true, true, false, string.Empty)
            };

            AddressInfo s2 = new AddressInfo
            {
                AddressId = s1.AddressId,
                City = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                Country = s1.Country,
                Phone = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                State = s1.State,
                Zip = TestHelper.RandomString(10, false, true, true, false, string.Empty)
            };

            string jsonContext = DivideJson(typeof(AddressInfo), s1, s2);
            string xmlContext = DivideXml(typeof(AddressInfo), s1, s2);
            AssertJsonString(xmlContext, jsonContext);

            AddressInfo s3 = CombineJson<AddressInfo>(jsonContext, s1);

            Assert.AreEqual(s2, s3);
        }

        [Test]
        public void TestAddressInfoEditFormNull()
        {
            AddressInfo s = new AddressInfo
            {
                AddressId = Guid.NewGuid(),
                City = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                Country = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                Phone = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                State = TestHelper.RandomString(10, false, true, true, false, string.Empty),
                Zip = TestHelper.RandomString(10, false, true, true, false, string.Empty)
            };

            string jsonContext = DivideJson(s.GetType(), null, s);
            string xmlContext = DivideXml(s.GetType(), null, s);
            AssertJsonString(xmlContext, jsonContext);

            AddressInfo n = CombineJson<AddressInfo>(jsonContext, null);

            Assert.AreEqual(s, n);
        }
    }
}