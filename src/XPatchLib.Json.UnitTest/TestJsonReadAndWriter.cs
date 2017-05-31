// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XPatchLib.Json.UnitTest.TestClass;

namespace XPatchLib.Json.UnitTest
{
    [TestClass]
    public class TestJsonReadAndWriter : TestBase
    {
        [TestMethod]
        public void Example()
        {
            Account account1 = new Account();

            Account account2 = new Account
            {
                Email = "xpatchlib@example.com",
                Active = true,
                CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                Roles = new List<string>
                {
                    "User",
                    "Admin"
                }
            };

            var serializer = new Serializer(typeof(Account));

            //Xml序列化内容
            var xmlString = XmlSerializer(serializer, account1, account2);
            //使用NewtonSoft转换xmlString后的Json
            string newtonConvertJsonResult = ConvertToJson(xmlString);
            //使用NewtonSoft转换xmlString后的Json
            string newtonConvertXmlResult = ConvertToXml(newtonConvertJsonResult);
            //使用NewtonSoft序列化后的Json
            string newtonJsonString = NewtonJsonSerializer(account2);

            //Json序列化内容
            var jsonString = JsonSerializer(serializer, account1, account2);

            //{"Account":{"Active":"true","CreatedDate":"2013-01-20T00:00:00Z","Email":"xpatchlib@example.com","Roles":{"String":[{"@Action":"Add","#text":"User"},{"@Action":"Add","#text":"Admin"}]}}}

            AssertStringEqual(newtonConvertJsonResult, jsonString);

            //使用Json反序列化，创建新对象
            Account account3 = JsonDesrializer(serializer, account1, jsonString);

            using (StringReader reader = new StringReader(jsonString))
            {
                using (Newtonsoft.Json.JsonTextReader jsonTextReader = new Newtonsoft.Json.JsonTextReader(reader))
                {
                    while (jsonTextReader.Read())
                        Debug.WriteLine("TokenType:{0},ValueType:{1},Value:{2}.", jsonTextReader.TokenType,
                            jsonTextReader.ValueType, jsonTextReader.Value);
                }
            }

            Assert.AreEqual(account2, account3);
        }

        [TestMethod]
        public void PreservingObjectReferencesOff()
        {
            #region PreservingObjectReferencesOff

            Person p1 = new Person
            {
                BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
                Name = "James"
            };

            Person p2 = new Person
            {
                BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
                Name = "Luiz"
            };

            List<Person> people = new List<Person>();
            people.Add(p1);
            people.Add(p2);

            var serializer = new Serializer(typeof(List<Person>));

            IDictionary<Type, string[]> keys = new Dictionary<Type, string[]>(1);
            keys.Add(typeof(Person), new[] {"Name", "BirthDate", "LastModified"});

            serializer.RegisterTypes(keys);

            //Xml序列化内容
            var xmlString = XmlSerializer(serializer, new List<Person>(), people);
            //使用NewtonSoft转换xmlString后的Json
            string newtonConvertJsonResult = ConvertToJson(xmlString);
            //Json序列化内容
            var jsonString = JsonSerializer(serializer, new List<Person>(), people);

            AssertStringEqual(newtonConvertJsonResult, jsonString);

            #endregion
        }
    }
}