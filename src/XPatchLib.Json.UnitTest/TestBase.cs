// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XPatchLib.Json.UnitTest
{
    public abstract class TestBase
    {
        private const string XML_HEADER = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
        private const int XML_HEADER_LENGHT = 39;

        protected virtual void TraceResult(string testMethodName, string testResult)
        {
            Trace.WriteLine("**************");
            Trace.WriteLine(testMethodName);
            Trace.WriteLine("Result:");
            Trace.WriteLine(testResult);
            Trace.WriteLine("**************");
        }

        protected string ConvertToJson(string xmlString) {

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString.ToString());
            string result = JsonConvert.SerializeXmlNode(doc);
            Debug.WriteLine("使用NewtonSoft转换xmlString后的Json:{0}{1}", System.Environment.NewLine, result);

            return result;
        }

        protected string ConvertToXml(string jsonString)
        {
            XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonString);
            Debug.WriteLine("使用NewtonSoft转换jsonString后的XML:{0}{1}", System.Environment.NewLine, doc.InnerText);

            return doc.InnerText;
        }

        protected string NewtonJsonSerializer(object value)
        {
            StringBuilder result = new StringBuilder();
            using (TextWriter jsonWriter = new StringWriter(result))
            {
                using (var newTonJsonTextWriter = new Newtonsoft.Json.JsonTextWriter(jsonWriter))
                {
                    new JsonSerializer().Serialize(newTonJsonTextWriter, value);
                }
            }
            Debug.WriteLine("NewtonJson序列化内容:{0}{1}", System.Environment.NewLine, result);
            return result.ToString();
        }

        /// <summary>
        /// 使用<see cref="JsonTextWriter"/>调用<see cref="Serializer"/>的序列化。
        /// </summary>
        /// <param name="pSerializer">The p serializer.</param>
        /// <param name="pOriValue">The p ori value.</param>
        /// <param name="pRevValue">The p rev value.</param>
        /// <returns></returns>
        protected string JsonSerializer(Serializer pSerializer, object pOriValue, object pRevValue) {
            StringBuilder result = new StringBuilder();
            using (TextWriter jsonWriter = new StringWriter(result)) {
                using (var newTonJsonTextWriter = new Newtonsoft.Json.JsonTextWriter(jsonWriter)) {
                    using (var jsonTextWriter = new JsonTextWriter(newTonJsonTextWriter)) {
                        pSerializer.Divide(jsonTextWriter, pOriValue, pRevValue);
                    }
                }
            }
            Debug.WriteLine("Json序列化内容:{0}{1}", System.Environment.NewLine, result);
            return result.ToString();
        }

        protected T JsonDesrializer<T>(Serializer pSerializer, T pOriValue, string pJsonString) {
            using (StringReader jsonReader = new StringReader(pJsonString)) {
                using (Newtonsoft.Json.JsonTextReader newTonJsonTextReader = new Newtonsoft.Json.JsonTextReader(jsonReader)) {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(newTonJsonTextReader)) {
                        return (T) pSerializer.Combine(jsonTextReader, pOriValue, false);
                    }
                }
            }
            return default(T);
        }

        /// <summary>
        /// 使用<see cref="XmlTextWriter"/>调用<see cref="Serializer"/>的序列化。
        /// </summary>
        /// <param name="pSerializer">The p serializer.</param>
        /// <param name="pOriValue">The p ori value.</param>
        /// <param name="pRevValue">The p rev value.</param>
        /// <returns></returns>
        protected string XmlSerializer(Serializer pSerializer, object pOriValue, object pRevValue) {
            StringBuilder result = new StringBuilder();
            using (TextWriter xmlWriter = new StringWriter(result))
            {
                using (var msXmlTextWriter = XmlWriter.Create(xmlWriter))
                {
                    using (var xmlTextWriter = new XmlTextWriter(msXmlTextWriter))
                    {
                        pSerializer.Divide(xmlTextWriter, pOriValue, pRevValue);
                    }
                }
            }
            Debug.WriteLine("Xml序列化内容:{0}{1}", System.Environment.NewLine, result.Remove(0, XML_HEADER_LENGHT));
            return result.ToString();
        }

        public void AssertStringEqual(string expected, string actual)
        {
            Assert.AreEqual(expected, actual);
            TraceResult(new StackTrace().GetFrame(1).GetMethod().Name, actual);
        }

        internal static XmlWriterSettings FlagmentSetting
        {
            get
            {
                var settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                settings.OmitXmlDeclaration = false;
                return settings;
            }
        }
    }
}