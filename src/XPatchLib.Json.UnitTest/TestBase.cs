// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
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
    public abstract class TestBase
    {
        protected void AssertJsonString(string xml, string json)
        {
            xml = xml.Substring(xml.IndexOf("<", 1));
            string jsonXmlContext = Convert(xml);
            Assert.AreEqual(jsonXmlContext, json);
        }

        protected string Convert(string xmlContext)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContext);
            string jsonXmlContext = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);

            DebugWriteLine(jsonXmlContext);
            return jsonXmlContext;
        }

        protected string SerializeJson(object value)
        {
            JsonSerializer serializer = new JsonSerializer();
            StringBuilder result = new StringBuilder();
            using (ExtentedStringWriter sw = new ExtentedStringWriter(result, new UTF8Encoding(false)))
            {
                serializer.Serialize(sw, value);
            }
            DebugWriteLine(result.ToString());
            return result.ToString();
        }

        protected string DivideJson(Type t, object oriValue, object revValue, JsonSerializeSetting setting = null)
        {
            Serializer serializer = new Serializer(t);
            StringBuilder result = new StringBuilder();
            using (ExtentedStringWriter sw = new ExtentedStringWriter(result, new UTF8Encoding(false)))
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    if (setting != null)
                        jsonWriter.Setting = setting;
                    serializer.Divide(jsonWriter, oriValue, revValue);
                }
            }
            DebugWriteLine(result.ToString());
            return result.ToString();
        }

        protected string DivideXml(Type t, object oriValue, object revValue, JsonSerializeSetting setting = null)
        {
            Serializer serializer = new Serializer(t);
            StringBuilder result = new StringBuilder();
            using (ExtentedStringWriter sw = new ExtentedStringWriter(result, new UTF8Encoding(false)))
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                {
                    if (setting != null)
                        xmlWriter.Setting = setting;
                    serializer.Divide(xmlWriter, oriValue, revValue);
                }
            }
            DebugWriteLine(result.ToString());
            return result.ToString();
        }

        protected void DebugWriteLine(string s)
        {
#if !XUNIT
            TestContext.WriteLine(s);
#else
            output.WriteLine(s);
#endif
            Console.WriteLine(s);
        }

#if XUNIT
        private readonly ITestOutputHelper output;
        public TestBase(ITestOutputHelper output)
        {
            this.output = output;
            TestInitialize();
        }
#endif
#if NUNIT
        [SetUp]
#endif
        public virtual void TestInitialize()
        {
        }


        protected T CombineJson<T>(string context, object oriValue, JsonSerializeSetting setting = null,
            bool createnew = true)
        {
            Serializer serializer = new Serializer(typeof(T));
            using (StringReader sr = new StringReader(context))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(sr))
                {
                    if (setting != null)
                        jsonReader.Setting = setting;
                    return (T) serializer.Combine(jsonReader, oriValue, !createnew);
                }
            }
        }

        protected T CombineJson<T>(string context, object oriValue)
        {
            return CombineJson<T>(context, oriValue, null, true);
        }

        protected T CombineJson<T>(string context, object oriValue, JsonSerializeSetting setting = null)
        {
            return CombineJson<T>(context, oriValue, setting, true);
        }

        protected void JsonRead(string context)
        {
            using (StringReader sr = new StringReader(context))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(sr))
                {
                    while (jsonReader.Read())
                    {
                    }
                }
            }
        }
    }

    internal sealed class ExtentedStringWriter : StringWriter
    {
        public ExtentedStringWriter(StringBuilder builder, Encoding desiredEncoding)
            : base(builder)
        {
            Encoding = desiredEncoding;
        }

        public override Encoding Encoding { get; }
    }
}