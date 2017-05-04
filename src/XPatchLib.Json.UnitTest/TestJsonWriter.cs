// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using XPatchLib.Json.UnitTest.TestClass;

namespace XPatchLib.Json.UnitTest
{
    [TestClass]
    public class TestJsonWriter : TestBase
    {
        [TestMethod]
        public void Example()
        {
            var context =
                "{\"Active\":\"true\",\"CreatedDate\":\"2013-01-20T00:00:00Z\",\"Email\":\"xpatchlib@example.com\",\"Roles\":[{\"Action\":\"Add\",\"\":\"User\"},{\"Action\":\"Add\",\"\":\"Admin\"}]}";

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
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                using (var jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    using (var jsonWriter = new JsonTextWriter(jsonTextWriter))
                    {
                        serializer.Divide(jsonWriter, account1, account2);
                    }
                }
            }

            //{"Active":"true","CreatedDate":"2013-01-20T00:00:00Z","Email":"xpatchlib@example.com","Roles":[{"Action":"Add","":"User"},{"Action":"Add","":"Admin"}]}

            AssertStringEqual(context, sb.ToString());
        }

        [TestMethod]
        public void PreservingObjectReferencesOff()
        {
            #region PreservingObjectReferencesOff

            Person p = new Person
            {
                BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
                Name = "James"
            };

            List<Person> people = new List<Person>();
            people.Add(p);
            people.Add(p);

            var serializer = new Serializer(typeof(List<Person>));

            IDictionary<Type,string[]> keys=new Dictionary<Type, string[]>(1);
            keys.Add(typeof(Person),new string[] { "Name", "BirthDate", "LastModified" });
            
            serializer.RegisterTypes(keys);
            ;
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                using (var jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    using (var jsonWriter = new JsonTextWriter(jsonTextWriter))
                    {
                        serializer.Divide(jsonWriter, new List<Person>(), people);
                    }
                }
            }

            //{"List_Person":[{"Action":"Add","BirthDate":"1980-12-23T00:00:00Z","LastModified":"2009-02-20T12:59:21Z","Name":"James"}]}
            AssertStringEqual("{\"List_Person\":[{\"Action\":\"Add\",\"BirthDate\":\"1980-12-23T00:00:00Z\",\"LastModified\":\"2009-02-20T12:59:21Z\",\"Name\":\"James\"}]}", sb.ToString());

            #endregion
            
        }
    }
}