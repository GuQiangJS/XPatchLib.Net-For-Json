// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using XPatchLib.Json.UnitTest;
#if NUNIT
using NUnit.Framework;

#elif XUNIT
using Test = Xunit.FactAttribute;
using Assert = XPatchLib.Json.UnitTest.XUnitAssert;
using Xunit.Abstractions;
#endif

namespace XPatchLib.UnitTest.ForXml
{
    [TestFixture]
    public class TestIncompleteXml : TestBase
    {
#if XUNIT
        public TestIncompleteXml(ITestOutputHelper output) : base(output)
        {
        }
#endif
        [Test]
        public void TestDeserializeIncompleteXml()
        {
            Serializer serializer = new Serializer(typeof(List<SimpleClass>));
            List<SimpleClass> list = new List<SimpleClass>();
            list.Add(new SimpleClass {S = "ssss"});
            list.Add(new SimpleClass {S = "eeeee"});
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    serializer.Divide(writer, null, list);
                }
            }
            Debug.WriteLine(sb.ToString());
            using (StringReader sr = new StringReader(sb.ToString(0, sb.Length - 3)))
            {
                using (XmlTextReader reader = new XmlTextReader(sr))
                {
                    Assert.Throws<XmlException>(() => serializer.Combine(reader, null));
                }
            }
        }
    }
}