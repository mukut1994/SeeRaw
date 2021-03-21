using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MK94.SeeRaw.UT
{
    public class Tests
    {
        RendererContext context = new RendererContext();
        Serializer serializer = new Serializer();

        RenderRoot root;

        void AssertMatches([CallerMemberName]string caller = "")
        {
            var stream = new MemoryStream();
            serializer.Serialize(root.Targets[0].Value, stream, context, new SerializerContext());

            var actual = Encoding.UTF8.GetString(stream.ToArray());

            actual = Regex.Replace(actual, "[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}", Guid.Empty.ToString());

            var testDataPath = Directory.GetCurrentDirectory()
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .TakeWhile(x => x != "C#")
                .Concat(new[] { "Testdata" })
                .Aggregate(Path.Combine);
           
            // uncomment to update Testdata files; do not check in uncommented!!!
             File.WriteAllText(Path.Combine(testDataPath, $"{caller}.json"), actual);

            var expected = File.ReadAllText(Path.GetFullPath(Path.Combine("/", testDataPath, $"{caller}.json")));

            Assert.AreEqual(expected, actual);
        }

        [SetUp]
        public void Setup()
        {
            root = new RenderRoot();
        }

        [Test]
        public void CustomType()
        {
            root.Render(new { PropString = "A", PropInt = 1, PropObj = new { PropInt2 = 2 } });

            AssertMatches();
        }

        [Test]
        public void String()
        {
            root.Render("string test");

            AssertMatches();
        }

        [Test]
        public void Number()
        {
            root.Render(1);

            AssertMatches();
        }

        [Test]
        public void List()
        {
            root.Render(new List<int> { 1, 2, 3 });

            AssertMatches();
        }

        [Test]
        public void Progress()
        {
            root.Render(SeeRawTypes.Progress());

            AssertMatches();
        }

        [Test]
        public void Navigation()
        {
            root.Render(SeeRawTypes.Navigation().WithAction("Navication Test", () => { }));

            AssertMatches();
        }

        [Test]
        public void Link()
        {
            root.Render(SeeRawTypes.Form("Link test", c => { }));

            AssertMatches();
        }

        [Test]
        public void Form()
        {
            root.Render(SeeRawTypes.Form("Form test", c => { }).WithInput("Text input", "default text"));

            AssertMatches();
        }

        [Test]
        public void Horizontal()
        {
            root.Render(SeeRawTypes.HorizontalRun("Horizontal Test"));

            AssertMatches();
        }

        [Test]
        public void Vertical()
        {
            root.Render(SeeRawTypes.VerticalRun("Vertical Test"));

            AssertMatches();
        }

        [Test]
        public void Log()
        {
            root.Render(SeeRawTypes.Logger().WithMessage("Logger test").CreateChild("Child 1"));

            AssertMatches();
        }
    }
}