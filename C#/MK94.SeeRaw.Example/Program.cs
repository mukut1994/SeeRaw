using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MK94.SeeRaw.Example
{
    class Program
    {
        static Progress progress = new Progress { Speed = "10kb/s", Max = "10MB" };
        static Lazy<Task> progressTask = new Lazy<Task>(() => UpdateFakeProgress(progress));

        static void Main()
        {
            var opt = @"[{""jsonPath"":""$..*"",""typeOptions"":{""dataType"":""Map"",""value"":[[""string"",{""renderer"":""value""}],[""bool"",{""renderer"":""value""}],[""number"",{""renderer"":""value""}],[""enum"",{""renderer"":""value""}],[""datetime"",{""renderer"":""value""}],[""object"",{""renderer"":""table""}],[""array"",{""renderer"":""table""}],[""link"",{""renderer"":""link""}],[""progress"",{""renderer"":""progress""}]]}},{""jsonPath"":""$"",""typeOptions"":{""dataType"":""Map"",""value"":[[""object"",{""renderer"":""navigation""}]]}},{""jsonPath"":""$.NestedList"",""typeOptions"":{""dataType"":""Map"",""value"":[[""array"",{""renderer"":""navigation"",""mergeWithParent"":true}]]}}]";

            SeeRawSetup
                .WithServer()
                .WithErrorHandler(x => Console.WriteLine(x))
                .WithGlobalRenderer(RenderClientMenu)
                .OpenBrowserAfterWait(TimeSpan.FromSeconds(15))
                .RunInBackground();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            Console.WriteLine("Enter");
        }

        static void RenderClientMenu()
        {
            var x = new
            {
                Child1_1 = 11,
                Child1_2 = 12
            };

            new
            {
                Child1 = x,
                Child2 = new
                {
                    Child2_1 = 21,
                    Child2_2 = 22
                },
                Enum = Title.Mr,
                Link = SeeRawTypes.Form("Link", (x) => Console.WriteLine("Actionable")),
                ID = Guid.NewGuid(),
                Time = DateTime.Now,
                Dict = new Dictionary<Guid, string> { { Guid.NewGuid(), "OK" } },
                Prog = progress,
                NestedList = new List<object>
                {
                    null,
                    new List<object>
                    {
                        1, 2, 3
                    },
                    new List<object>
                    {
                        1, 2, 3
                    },
                },
                ByKey = new List<X>
                {
                    new X { Name = "Key 1" },
                    new X { Name = "Key 2" }
                },
                Ref = x
            }.Render();

            return;

            // Show a basic hello world
            "Hello World".Render();

            // Show a simple menu
            // a. create a placeholder and keep a reference to the renderTarget so we can update it later
            "Loading Menu...".Render(out var menuTarget);
            "Click on a menu item above".Render(out var contentTarget);

            // b. Create actions/menu items to update the contentTarget depending on whats invoked
            menuTarget.Value = SeeRawTypes.Navigation()
                .WithAction("Say Hi", () => SayHi(contentTarget))
                .WithAction("Calculator", () => Calc(contentTarget))
                .WithAction("Show file copy progress", () => ShowProgress(contentTarget));
        }

        class X
        {
            public string Name { get; set; }

            public int PropA => 1;
            public int PropB => 2;
        }

        enum Title
        {
            Mr, Mrs
        }

        static void SayHi(RenderTarget contentTarget)
        {
            contentTarget.Value = SeeRawTypes.Action("Please enter your details", (string name, Title title) => contentTarget.Value = $"Hi {title} {name}");
        }

        static void Calc(RenderTarget contentTarget)
        {
            contentTarget.Value = SeeRawTypes.Action("Add", (int a, int b) => contentTarget.Value = $"Result: {a + b}");
        }

        static void ShowProgress(RenderTarget contentTarget)
        {
            // initialize the lazy; starts up a background thread to update the progress
            _ = progressTask.Value;
            contentTarget.Value = progress;
        }

        static Task UpdateFakeProgress(Progress progress)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    progress.Percent = (progress.Percent + 1) % 100;
                    Thread.Sleep(100);
                }
            });

        }
    }
}
