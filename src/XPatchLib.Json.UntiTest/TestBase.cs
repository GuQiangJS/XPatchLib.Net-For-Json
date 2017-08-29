// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
#if NUNIT
using NUnit.Framework;

#elif XUNIT
using Test = Xunit.FactAttribute;
using Assert = XPatchLib.Json.UntiTest.XUnitAssert;
using Xunit.Abstractions;

#endif

namespace XPatchLib.Json.UntiTest
{
    public abstract class TestBase
    {
        protected void AssertJsonString(string xml, string json)
        {
            xml = xml.Substring(40);
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

    public static class TestHelper
    {
        private static readonly DateTime _minDate = new DateTime(1990, 1, 1);
        private static readonly DateTime _maxDate = DateTime.Now.Date;

        private static readonly Random _random = new Random((int) DateTime.Now.Ticks);

        /// <summary>
        ///     生成随机字符串
        /// </summary>
        /// <param name="length">目标字符串的长度</param>
        /// <param name="useNum">是否包含数字，1=包含，默认为包含</param>
        /// <param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        /// <param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        /// <param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        /// <param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        /// <returns>指定长度的随机字符串</returns>
        public static string RandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            string s = null, str = custom;
            if (useNum) str += "0123456789";
            if (useLow) str += "abcdefghijklmnopqrstuvwxyz";
            if (useUpp) str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (useSpe) str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
            for (int i = 0; i < length; i++)
                s += str.Substring(_random.Next(0, str.Length - 1), 1);
            return s;
        }

        /// <summary>
        ///     随机datetime
        /// </summary>
        /// <returns></returns>
        public static DateTime RandomDate()
        {
            return RandomDate(_minDate, _maxDate);
        }

        /// <summary>
        ///     随机datetime
        /// </summary>
        /// <returns></returns>
        public static DateTime RandomDate(DateTime minDate, DateTime maxDate)
        {
            int totalDays = (int) maxDate.Subtract(minDate).TotalDays;
            int randomDays = _random.Next(0, totalDays);
            return minDate.AddDays(randomDays);
        }

        /// <summary>
        ///     随机bool
        /// </summary>
        /// <returns></returns>
        public static bool RandomBoolean()
        {
            return DateTime.Now.Second % 2 > 0;
        }

        /// <summary>
        ///     随机char
        /// </summary>
        /// <returns></returns>
        public static char RandomChar()
        {
            return Convert.ToChar(Convert.ToInt32(26 * _random.NextDouble() + 64));
        }

        /// <summary>
        ///     随机byte
        /// </summary>
        /// <returns></returns>
        public static byte RandomByte()
        {
            return RandomByte(0, byte.MaxValue);
        }

        /// <summary>
        ///     随机byte
        /// </summary>
        /// <returns></returns>
        public static byte RandomByte(byte min, byte max)
        {
            return (byte) RandomNumber(min, max);
        }

        /// <summary>
        ///     随机shrot
        /// </summary>
        /// <returns></returns>
        public static short RandomShort()
        {
            return RandomShort(0, short.MaxValue);
        }

        /// <summary>
        ///     随机short
        /// </summary>
        /// <returns></returns>
        public static short RandomShort(short min, short max)
        {
            return (short) RandomNumber(min, max);
        }

        /// <summary>
        ///     随机int
        /// </summary>
        /// <returns></returns>
        public static int RandomNumber()
        {
            return RandomNumber(int.MinValue, int.MaxValue);
        }

        public static double RandomDoubleNumber()
        {
            return _random.NextDouble();
        }

        /// <summary>
        ///     随机int
        /// </summary>
        /// <returns></returns>
        public static int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}