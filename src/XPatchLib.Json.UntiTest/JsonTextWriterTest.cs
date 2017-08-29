using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
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
    public class JsonSerializeSettingTest
    {
        [Test]
        public void TestJsonSerializeSetting()
        {
            JsonSerializeSetting setting = new JsonSerializeSetting();
            setting.SAIN = "SAIN";
            setting.ActionName = "ACTIONNAME";
            setting.Mode=DateTimeSerializationMode.Local;
            setting.SerializeDefalutValue = true;
            Assert.AreEqual("SAIN", setting.SAIN);
            Assert.AreEqual("ACTIONNAME", setting.ActionName);
            Assert.IsTrue(setting.SerializeDefalutValue);
            Assert.AreEqual(DateTimeSerializationMode.Local, setting.Mode);
            JsonSerializeSetting newSetting=setting.Clone() as JsonSerializeSetting;
            Assert.IsNotNull(newSetting);
            PropertyInfo[] pi =
                typeof(JsonSerializeSetting).GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.GetProperty);
            foreach (PropertyInfo info in pi)
            {
                Assert.AreEqual(info.GetValue(setting, null), info.GetValue(newSetting, null));
            }
        }
    }

    [TestFixture]
    public class JsonTextWriterTest:TestBase
    {
#if XUNIT
        public JsonTextWriterTest(ITestOutputHelper output):base(output)
        {
        }
#endif

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

            string jsonContext = DivideJson(s1.GetType(), s1, s2, new JsonSerializeSetting() {ActionName = "ACT"});
            string xmlContext = DivideXml(s1.GetType(), s1, s2, new JsonSerializeSetting() { ActionName = "ACT" });
            AssertJsonString(xmlContext, jsonContext);

            string[] s3 = CombineJson<string[]>(jsonContext, s1, new JsonSerializeSetting() { ActionName = "ACT" });

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

            string jsonContext = DivideJson(s1.GetType(), s1, s2, new JsonSerializeSetting() { SAIN = "VALUE" });
            string xmlContext = DivideXml(s1.GetType(), s1, s2, new JsonSerializeSetting() { SAIN = "VALUE" });

            xmlContext = xmlContext.Substring(40);
            string jsonXmlContext = Convert(xmlContext);
            Assert.AreEqual(jsonXmlContext.Replace("#text","VALUE"), jsonContext);

            string[] s3 = CombineJson<string[]>(jsonContext, s1, new JsonSerializeSetting() { SAIN = "VALUE" });

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
    ""DT"": """+XmlConvert.ToString(c.DT,XmlDateTimeSerializationMode.RoundtripKind)+@""",
    ""Number"": """+c.Number + @""",
    ""Str"": " + Newtonsoft.Json.JsonConvert.ToString(c.Str) +  @"
  }
}";
            TestJsonTextWriterProperties(c, context);
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
    ""Str"": " + Newtonsoft.Json.JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting() { Mode = DateTimeSerializationMode.Local });
        }

        [Test]
        public void TestJsonTextWriterPropertiesLoaclDateTimeMode()
        {
            TestClass c = TestClass.CreateInstance();
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.Local) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + Newtonsoft.Json.JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting() {Mode = DateTimeSerializationMode.Local});
        }

        [Test]
        public void TestJsonTextWriterPropertiesUnspecifiedDateTimeMode()
        {
            TestClass c = TestClass.CreateInstance();
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.Unspecified) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + Newtonsoft.Json.JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting() { Mode = DateTimeSerializationMode.Unspecified });
        }

        [Test]
        public void TestJsonTextWriterPropertiesUtcDateTimeMode()
        {
            TestClass c = TestClass.CreateInstance();
            string context = @"{
  ""TestClass"": {
    ""DT"": """ + XmlConvert.ToString(c.DT, XmlDateTimeSerializationMode.Utc) + @""",
    ""Number"": """ + c.Number + @""",
    ""Str"": " + Newtonsoft.Json.JsonConvert.ToString(c.Str) + @"
  }
}";
            TestJsonTextWriterProperties(c, context,
                new JsonSerializeSetting() { Mode = DateTimeSerializationMode.Utc });
        }
        
        void TestJsonTextWriterProperties(TestClass instance, string context,JsonSerializeSetting setting=null)
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
            public static TestClass CreateInstance()
            {
                DateTime now = DateTime.Now;
                decimal d = decimal.MaxValue;
                string s = TestHelper.RandomString(50, true, true, true, true, String.Empty);
                TestClass result = new TestClass()
                {
                    DT = now,
                    Number = d,
                    Str = s
                };
                return result;
            }
            public DateTime DT { get; set; }
            public decimal Number { get; set; }
            public string Str { get; set; }
        }
    }
}
