// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
#if NUNIT
using NUnit.Framework;

#elif XUNIT
using Test = Xunit.FactAttribute;
using Assert = XPatchLib.Json.UntiTest.XUnitAssert;
using Xunit.Abstractions;

#endif

namespace XPatchLib.Json.UntiTest
{
    [TestFixture]
    internal class SimpleBasicTest : TestBase
    {
#if XUNIT
        public SimpleBasicTest(ITestOutputHelper output):base(output)
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

    [TestFixture]
    internal class SimpleObjectTest : TestBase
    {
#if XUNIT
        public SimpleObjectTest(ITestOutputHelper output):base(output)
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

    [TestFixture]
    public class AddressInfoTest : TestBase
    {
#if XUNIT
        public AddressInfoTest(ITestOutputHelper output):base(output)
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

    [PrimaryKey("S")]
    public class SimpleClass
    {
        public string S { get; set; }

        /// <summary>确定指定的 <see cref="T:System.Object" /> 是否等于当前的 <see cref="T:System.Object" />。</summary>
        /// <returns>如果指定的 <see cref="T:System.Object" /> 等于当前的 <see cref="T:System.Object" />，则为 true；否则为 false。</returns>
        /// <param name="obj">与当前的 <see cref="T:System.Object" /> 进行比较的 <see cref="T:System.Object" />。</param>
        public override bool Equals(object obj)
        {
            SimpleClass c = obj as SimpleClass;
            if (c == null) return false;
            return string.Equals(c.S, S, StringComparison.Ordinal);
        }

        /// <summary>用作特定类型的哈希函数。</summary>
        /// <returns>当前 <see cref="T:System.Object" /> 的哈希代码。</returns>
        public override int GetHashCode()
        {
            return S.GetHashCode();
        }
    }

    [PrimaryKey("AddressId")]
    public class AddressInfo : ICloneable
    {
        public Guid AddressId { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        /// <summary>创建作为当前实例副本的新对象。</summary>
        /// <returns>作为此实例副本的新对象。</returns>
        public object Clone()
        {
            AddressInfo result = new AddressInfo();
            result.AddressId = AddressId;
            result.City = City;
            result.Country = Country;
            result.Phone = Phone;
            result.State = State;
            result.Zip = Zip;
            return result;
        }

        /// <summary>确定指定的 <see cref="T:System.Object" /> 是否等于当前的 <see cref="T:System.Object" />。</summary>
        /// <returns>如果指定的 <see cref="T:System.Object" /> 等于当前的 <see cref="T:System.Object" />，则为 true；否则为 false。</returns>
        /// <param name="obj">与当前的 <see cref="T:System.Object" /> 进行比较的 <see cref="T:System.Object" />。</param>
        public override bool Equals(object obj)
        {
            AddressInfo c = obj as AddressInfo;
            if (c == null) return false;
            return Equals(c.AddressId, AddressId)
                   && string.Equals(c.City, City, StringComparison.Ordinal)
                   && string.Equals(c.Country, Country, StringComparison.Ordinal)
                   && string.Equals(c.Phone, Phone, StringComparison.Ordinal)
                   && string.Equals(c.State, State, StringComparison.Ordinal)
                   && string.Equals(c.Zip, Zip, StringComparison.Ordinal);
        }

        /// <summary>用作特定类型的哈希函数。</summary>
        /// <returns>当前 <see cref="T:System.Object" /> 的哈希代码。</returns>
        public override int GetHashCode()
        {
            var result = 0;
            if (AddressId != null)
                result ^= AddressId.GetHashCode();
            if (City != null)
                result ^= City.GetHashCode();
            if (State != null)
                result ^= State.GetHashCode();
            if (Zip != null)
                result ^= Zip.GetHashCode();
            if (Country != null)
                result ^= Country.GetHashCode();
            if (Phone != null)
                result ^= Phone.GetHashCode();
            return result;
        }
    }
}