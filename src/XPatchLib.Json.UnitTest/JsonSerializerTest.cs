using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace XPatchLib.Json.UnitTest
{
    [TestClass]
    public class JsonSerializerTest:TestBase
    {

        [TestInitialize]
        public void Init() { }

        private Account CreateDefaultAccount()
        {
            return new Account
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
        }

        [TestMethod]
        [Description("简单对象设置值序列化测试，测试完全新增的对象序列化结果")]
        public void TestSimpleObjectJsonSerializer()
        {
/*
{
  "Account": {
    "Active": "true",
    "CreatedDate": "2013-01-20T00:00:00Z",
    "Email": "xpatchlib@example.com",
    "Roles": [
      {
        "Action": "Add",
        "": "User"
      },
      {
        "Action": "Add",
        "": "Admin"
      }
    ]
  }
}
*/
            Account account1 = new Account();

            Account account2 = CreateDefaultAccount();

            var serializer = new Serializer(typeof(Account));
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                using (var jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                    using (var jsonWriter = new JsonTextWriter(jsonTextWriter))
                    {
                        serializer.Divide(jsonWriter, account1, account2);
                    }
                }
            }

            AssertEqual(@"{
  ""Account"": {
    ""Active"": ""true"",
    ""CreatedDate"": ""2013-01-20T00:00:00Z"",
    ""Email"": ""xpatchlib@example.com"",
    ""Roles"": [
      {
        ""Action"": ""Add"",
        """": ""User""
      },
      {
        ""Action"": ""Add"",
        """": ""Admin""
      }
    ]
  }
}", sb.ToString());


        }

        [TestMethod]
        [Description("简单对象设置值序列化测试，测试对象简单属性变更序列化结果")]
        public void TestSimpleObjectUpdateSimplePropertyJsonSerializer()
        {
            /*
            {
              "Account": {
                "Active": "false"
              }
            }
            */
            Account account1 = CreateDefaultAccount();

            Account account2 = CreateDefaultAccount();
            account2.Active = false;

            var serializer = new Serializer(typeof(Account));
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                using (var jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                    using (var jsonWriter = new JsonTextWriter(jsonTextWriter))
                    {
                        serializer.Divide(jsonWriter, account1, account2);
                    }
                }
            }
            AssertEqual(@"{
  ""Account"": {
    ""Active"": ""false""
  }
}", sb.ToString());
        }



        [TestMethod]
        [Description("简单对象设置值序列化测试，测试对集合属性增加项变更序列化结果")]
        public void TestSimpleObjectSimpleListPropertyAddItemJsonSerializer()
        {
            #region 测试增加
            /*
            {
              "Account": {
                "Roles": [
                  {
                    "Action": "Add",
                    "": "Guest"
                  }
                ]
              }
            }
            */
            Account account1 = CreateDefaultAccount();

            Account account2 = CreateDefaultAccount();
            account2.Roles.Add("Guest");

            var serializer = new Serializer(typeof(Account));
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                using (var jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                    using (var jsonWriter = new JsonTextWriter(jsonTextWriter))
                    {
                        serializer.Divide(jsonWriter, account1, account2);
                    }
                }
            }
            AssertEqual(@"{
  ""Account"": {
    ""Roles"": [
      {
        ""Action"": ""Add"",
        """": ""Guest""
      }
    ]
  }
}", sb.ToString());
            #endregion
        }

        [TestMethod]
        [Description("简单对象设置值序列化测试，测试对集合属性删除项变更序列化结果")]
        public void TestSimpleObjectSimpleListPropertyRemoveItemJsonSerializer()
        {
            #region 测试增加
            /*
            {
              "Account": {
                "Roles": [
                  {
                    "Action": "Remove",
                    "": "Admin"
                  }
                ]
              }
            }
            */
            Account account1 = CreateDefaultAccount();

            Account account2 = CreateDefaultAccount();
            account2.Roles.Remove("Admin");

            var serializer = new Serializer(typeof(Account));
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                using (var jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                    using (var jsonWriter = new JsonTextWriter(jsonTextWriter))
                    {
                        serializer.Divide(jsonWriter, account1, account2);
                    }
                }
            }
            AssertEqual(@"{
  ""Account"": {
    ""Roles"": [
      {
        ""Action"": ""Remove"",
        """": ""Admin""
      }
    ]
  }
}", sb.ToString());
            #endregion
        }

        [TestMethod]
        [Description("简单对象设置值序列化测试，测试对集合属性删除项变更序列化结果")]
        public void TestSimpleObjectSimpleListPropertyUpdateItemJsonSerializer()
        {
            #region 测试增加
            /*
            {
              "Account": {
                "Roles": [
                  {
                    "Action": "Remove",
                    "": "Admin"
                  },
                  {
                    "Action": "Add",
                    "": "Guest"
                  }
                ]
              }
            }
            */
            Account account1 = CreateDefaultAccount();

            Account account2 = CreateDefaultAccount();
            account2.Roles.Remove("Admin");
            account2.Roles.Add("Guest");

            var serializer = new Serializer(typeof(Account));
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                using (var jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                    using (var jsonWriter = new JsonTextWriter(jsonTextWriter))
                    {
                        serializer.Divide(jsonWriter, account1, account2);
                    }
                }
            }
            AssertEqual(@"{
  ""Account"": {
    ""Roles"": [
      {
        ""Action"": ""Remove"",
        """": ""Admin""
      },
      {
        ""Action"": ""Add"",
        "": ""Guest""
      }
    ]
  }
}", sb.ToString());
            #endregion
        }

        public class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public DateTime CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }
    }
}