// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Reflection;
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
    public class JsonSerializeSettingTest
    {
        [Test]
        public void TestJsonSerializeSetting()
        {
            JsonSerializeSetting setting = new JsonSerializeSetting();
            setting.SAIN = "SAIN";
            setting.ActionName = "ACTIONNAME";
            setting.Mode = DateTimeSerializationMode.Local;
            setting.SerializeDefalutValue = true;
            Assert.AreEqual("SAIN", setting.SAIN);
            Assert.AreEqual("ACTIONNAME", setting.ActionName);
            Assert.IsTrue(setting.SerializeDefalutValue);
            Assert.AreEqual(DateTimeSerializationMode.Local, setting.Mode);
            JsonSerializeSetting newSetting = setting.Clone() as JsonSerializeSetting;
            Assert.IsNotNull(newSetting);
            PropertyInfo[] pi =
                typeof(JsonSerializeSetting).GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.GetProperty);
            foreach (PropertyInfo info in pi)
                Assert.AreEqual(info.GetValue(setting, null), info.GetValue(newSetting, null));
        }
    }
}