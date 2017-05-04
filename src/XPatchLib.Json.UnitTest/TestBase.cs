// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XPatchLib.Json.UnitTest
{
    public abstract class TestBase
    {
        protected virtual void TraceResult(string testMethodName, string testResult)
        {
            Trace.WriteLine("**************");
            Trace.WriteLine(testMethodName);
            Trace.WriteLine("Result:");
            Trace.WriteLine(testResult);
            Trace.WriteLine("**************");
        }

        public void AssertStringEqual(string expected, string actual)
        {
            Assert.AreEqual(expected, actual);
            TraceResult(new StackTrace().GetFrame(1).GetMethod().Name, actual);
        }
    }
}