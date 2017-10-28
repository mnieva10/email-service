using System;
using System.Collections.Generic;
using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusExtensionsTest
  {
    [TestCase(typeof(object[]), true)]
    [TestCase(typeof(string), false)]
    [TestCase(typeof(object), false)]
    public void TestCases_ImplementsIList(Type type, bool expected)
    {
      Assert.AreEqual(expected, type.ImplementsIList());
    }

    [TestCase(typeof(List<string>), true)]
    [TestCase(typeof(List<object>), true)]
    [TestCase(typeof(List), false)]
    [TestCase(typeof(string), false)]
    public void TestCases_ImplementsGenericEnumerable(Type type, bool expected)
    {
      Assert.AreEqual(expected, type.ImplementsGenericEnumerable());
    }
  }
}
