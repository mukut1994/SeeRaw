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
            SeeRawSetup
                .WithServer()
                .WithGlobalRenderer(RenderClientMenu)
                .OpenBrowserAfterWait(TimeSpan.FromSeconds(15))
                .RunInBackground();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void RenderClientMenu()
        {
            new List<object>
            {
                new List<int> { 1 },
                new List<int> { 2 }
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
