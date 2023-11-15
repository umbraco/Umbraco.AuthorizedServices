using Umbraco.AuthorizedServices.Helpers;

namespace Umbraco.AuthorizedServices.Tests.Helpers;

[TestFixture]
internal class ReflectionHelperTests
{
    [Test]
    public void GetOptionalPropertyValue_WithPropertyFound_ReturnsPropertyuValue()
    {
        var obj = new TestClass
        {
            TestStringProperty = "test",
            TestIntProperty = 99,
            TestBoolProperty = true,
        };
        var value1 = obj.GetOptionalPropertyValue("TestStringProperty", string.Empty);
        var value2 = obj.GetOptionalPropertyValue("TestIntProperty", 0);
        var value3 = obj.GetOptionalPropertyValue("TestBoolProperty", false);

        Assert.AreEqual("test", value1);
        Assert.AreEqual(99, value2);
        Assert.True(value3);
    }

    [Test]
    public void GetOptionalPropertyValue_WithPropertyNotFound_ReturnsDefaultValue()
    {
        var obj = new TestClass
        {
            TestStringProperty = "test",
            TestIntProperty = 99,
            TestBoolProperty = true
        };
        var value1 = obj.GetOptionalPropertyValue("MissingStringProperty", "default");
        var value2 = obj.GetOptionalPropertyValue("MissingIntProperty", 10);
        var value3 = obj.GetOptionalPropertyValue("MissingBoolProperty", false);

        Assert.AreEqual("default", value1);
        Assert.AreEqual(10, value2);
        Assert.False(value3);
    }

    [Test]
    public void SetOptionalPropertyValue_WithPropertyFound_SetsValues()
    {
        var obj = new TestClass();
        obj.SetOptionalPropertyValue("TestStringProperty", "test");
        obj.SetOptionalPropertyValue("TestIntProperty", 99);
        obj.SetOptionalPropertyValue("TestBoolProperty", true);

        Assert.AreEqual("test", obj.TestStringProperty);
        Assert.AreEqual(99, obj.TestIntProperty);
        Assert.True(obj.TestBoolProperty);
    }

    private class TestClass
    {
        public string TestStringProperty { get; set; } = string.Empty;

        public int TestIntProperty { get; set; }

        public bool TestBoolProperty { get; set; }
    }
}
