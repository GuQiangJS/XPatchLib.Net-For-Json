// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace XPatchLib.Json.UnitTest.TestClass
{
    public class Person
    {
        public string Name { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime LastModified { get; set; }

        [JsonIgnore]
        public string Department { get; set; }
    }
}