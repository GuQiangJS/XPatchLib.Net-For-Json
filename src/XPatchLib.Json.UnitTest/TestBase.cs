using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XPatchLib.Json.UnitTest
{
    public abstract class TestBase
    {
        protected virtual void TraceResult(string testMethodName,string testResult)
        {
            Trace.WriteLine("**************");
            Trace.WriteLine(testMethodName);
            Trace.WriteLine("Result:");
            Trace.WriteLine(testResult);
            Trace.WriteLine("**************");
        }

        public void AssertEqual(string expected, string actual)
        {
            Assert.AreEqual(expected, actual);
            TraceResult(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name, actual.ToString());
        }
    }
}
