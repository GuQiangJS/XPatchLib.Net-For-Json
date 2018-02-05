// Copyright © 2013-2018 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
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
        [Test]
        public void TestClone()
        {
            JsonSerializeSetting c = new JsonSerializeSetting();
            c.IgnoreAttributeType = typeof(string);
            c.SAIN = new Random().Next(Int32.MinValue, Int32.MaxValue).ToString();
            c.ActionName = new Random().Next(Int32.MinValue, Int32.MaxValue).ToString();
#if NET_40_UP || NETSTANDARD_2_0_UP
            c.AssemblyQualifiedName = new Random().Next(Int32.MinValue, Int32.MaxValue).ToString();
#endif
#if NET || NETSTANDARD_2_0_UP
            c.EnableOnDeserializedAttribute = !c.EnableOnDeserializedAttribute;
            c.EnableOnDeserializingAttribute = !c.EnableOnDeserializingAttribute;
            c.EnableOnSerializedAttribute = !c.EnableOnSerializedAttribute;
            c.EnableOnSerializingAttribute = !c.EnableOnSerializingAttribute;
#endif
            foreach (string name in Enum.GetNames(typeof(SerializeMemberType)))
            {
                SerializeMemberType t = (SerializeMemberType)Enum.Parse(typeof(SerializeMemberType), name);
                if (c.MemberType != t)
                    c.MemberType = t;
            }
            foreach (string name in Enum.GetNames(typeof(DateTimeSerializationMode)))
            {
                DateTimeSerializationMode t =
                    (DateTimeSerializationMode)Enum.Parse(typeof(DateTimeSerializationMode), name);
                if (c.Mode != t)
                    c.Mode = t;
            }
            foreach (string name in Enum.GetNames(typeof(SerializeMemberModifier)))
            {
                SerializeMemberModifier t = (SerializeMemberModifier)Enum.Parse(typeof(SerializeMemberModifier), name);
                if (c.Modifier != t)
                    c.Modifier = t;
            }
            c.SerializeDefalutValue = !c.SerializeDefalutValue;
            c.ActionName = new Random().Next(Int32.MinValue, Int32.MaxValue).ToString();

            JsonSerializeSetting c_new = c.Clone() as JsonSerializeSetting;
            PropertyInfo[] pis = typeof(JsonSerializeSetting).GetProperties();
            foreach (PropertyInfo pi in pis)
                Assert.AreEqual(pi.GetValue(c, null), pi.GetValue(c_new, null));
        }
    }
}