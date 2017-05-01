using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XPatchLib.Json.UnitTest
{
    [TestClass]
    public class TestJsonWriter
    {
        [TestMethod]
        public void SimpleJsonWriteTest()
        {
            var context = "{\"Active\":\"true\",\"CreatedDate\":\"2013-01-20T00:00:00Z\",\"Email\":\"xpatchlib@example.com\",\"Roles\":[\"User\",\"Admin\"]}";

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
            Assert.AreEqual(context, sb.ToString());
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