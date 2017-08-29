// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
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
    public class JsonTextWriterTest : TestBase
    {
#if XUNIT
        public JsonTextWriterTest(ITestOutputHelper output) : base(output)
        {
        }
#endif

        private void TestJsonTextWriterProperties(TestClass instance, string context,
            JsonSerializeSetting setting = null)
        {
            string xmlContext = DivideXml(instance.GetType(), null, instance, setting);
            string convertContext = Convert(xmlContext);

            StringBuilder sb = new StringBuilder();
            Serializer serializer = new Serializer(typeof(TestClass));
            using (ExtentedStringWriter strWriter = new ExtentedStringWriter(sb, new UTF8Encoding(false)))
            {
                using (JsonTextWriter writer = new JsonTextWriter(strWriter))
                {
                    if (setting != null)
                        writer.Setting = setting;
                    serializer.Divide(writer, null, instance);
                }
            }
            DebugWriteLine(sb.ToString());
            Assert.AreEqual(context, sb.ToString());

            TestClass newInstance = CombineJson<TestClass>(sb.ToString(), null, setting);

            Assert.IsTrue(string.Equals(instance.Str, newInstance.Str, StringComparison.Ordinal));
            Assert.IsTrue(decimal.Equals(instance.Number, newInstance.Number));
            Assert.IsTrue(DateTime.Equals(instance.DT.ToUniversalTime(), newInstance.DT.ToUniversalTime()));
        }

        public class TestClass
        {
            public DateTime DT { get; set; }
            public decimal Number { get; set; }
            public string Str { get; set; }

            public static TestClass CreateInstance()
            {
                DateTime now = DateTime.Now;
                decimal d = decimal.MaxValue;
                string s = TestHelper.RandomString(50, true, true, true, true, String.Empty);
                TestClass result = new TestClass
                {
                    DT = now,
                    Number = d,
                    Str = s
                };
                return result;
            }
        }

        [Test]
        public void TestActionName()
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

            string jsonContext = DivideJson(s1.GetType(), s1, s2, new JsonSerializeSetting {ActionName = "ACT"});
            string xmlContext = DivideXml(s1.GetType(), s1, s2, new JsonSerializeSetting {ActionName = "ACT"});
            AssertJsonString(xmlContext, jsonContext);

            string[] s3 = CombineJson<string[]>(jsonContext, s1, new JsonSerializeSetting {ActionName = "ACT"});

            Assert.AreEqual(s3.Length, s2.Length);
            for (int i = 0; i < s3.Length; i++)
                Assert.AreEqual(s3[i], s2[i]);
        }

        [Test]
        public void TestJsonTextWriterPropertiesDefault()
        {
            TestClass c = TestClass.CreateInstance();
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.RoundtripKind) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context);
        }

        [Test]
        public void TestJsonTextWriterPropertiesLoaclDateTimeMode()
        {
            TestClass c = TestClass.CreateInstance();
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.Local) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting {Mode = DateTimeSerializationMode.Local});
        }

        [Test]
        public void TestJsonTextWriterPropertiesSpecialChar()
        {
            TestClass c = TestClass.CreateInstance();
            c.Str = TestHelper.RandomString(20, true, true, true, true, @"\");
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.Local) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting {Mode = DateTimeSerializationMode.Local});
        }

        [Test]
        public void TestJsonTextWriterPropertiesUnspecifiedDateTimeMode()
        {
            TestClass c = TestClass.CreateInstance();
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.Unspecified) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting {Mode = DateTimeSerializationMode.Unspecified});
        }

        [Test]
        public void TestJsonTextWriterPropertiesUtcDateTimeMode()
        {
            TestClass c = TestClass.CreateInstance();
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.Utc) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting {Mode = DateTimeSerializationMode.Utc});
        }

        [Test]
        public void TestNotIndent()
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

            Serializer serializer = new Serializer(typeof(string[]));
            StringBuilder jsonResult = new StringBuilder();
            using (ExtentedStringWriter sw = new ExtentedStringWriter(jsonResult, new UTF8Encoding(false)))
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.Formatting = Formatting.None;
                    serializer.Divide(jsonWriter, s1, s2);
                }
            }
            DebugWriteLine(jsonResult.ToString());
            string jsonContext = jsonResult.ToString();

            StringBuilder xmlResult = new StringBuilder();
            using (ExtentedStringWriter sw = new ExtentedStringWriter(xmlResult, new UTF8Encoding(false)))
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                {
                    xmlWriter.Formatting = Formatting.None;
                    serializer.Divide(xmlWriter, s1, s2);
                }
            }
            DebugWriteLine(xmlResult.ToString());
            string xmlContext = xmlResult.ToString();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContext.Substring(xmlContext.IndexOf("<", 1)));
            string jsonXmlContext = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None);
            Assert.AreEqual(jsonXmlContext, jsonContext);

            string[] s3 = CombineJson<string[]>(jsonContext, s1);

            Assert.AreEqual(s3.Length, s2.Length);
            for (int i = 0; i < s3.Length; i++)
                Assert.AreEqual(s3[i], s2[i]);
        }

        [Test]
        public void TestSAIN()
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

            string jsonContext = DivideJson(s1.GetType(), s1, s2, new JsonSerializeSetting {SAIN = "VALUE"});
            string xmlContext = DivideXml(s1.GetType(), s1, s2, new JsonSerializeSetting {SAIN = "VALUE"});

            xmlContext = xmlContext.Substring(40);
            string jsonXmlContext = Convert(xmlContext);
            Assert.AreEqual(jsonXmlContext.Replace("#text", "VALUE"), jsonContext);

            string[] s3 = CombineJson<string[]>(jsonContext, s1, new JsonSerializeSetting {SAIN = "VALUE"});

            Assert.AreEqual(s3.Length, s2.Length);
            for (int i = 0; i < s3.Length; i++)
                Assert.AreEqual(s3[i], s2[i]);
        }
    }
}