using MovieBarCodeGenerator.GUI;
using NUnit.Framework;
using System.Windows.Forms;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class TextBoxAutoScrollTests
{
    [TestCase(false, false, ScrollBars.None)]
    [TestCase(true, false, ScrollBars.Vertical)]
    [TestCase(false, true, ScrollBars.Horizontal)]
    [TestCase(true, true, ScrollBars.Both)]
    [Test]
    public void DecideWanted_Returns_Expected_ScrollBars(bool needVertical, bool needHorizontal, ScrollBars expected)
    {
        Assert.AreEqual(expected, TextBoxAutoScroll.DecideWanted(needVertical, needHorizontal));
    }
}
