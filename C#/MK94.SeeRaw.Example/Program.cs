using System;
using System.Collections.Generic;
using System.Reflection;

namespace MK94.SeeRaw.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Open the browser using the default host; Optional: the browser can be opened manually
            SeeRawDefault.OpenBrowser();

            // Show a basic hello world
            "Hello World".Render();

            // Show a simple menu
            // a. create a placeholder and keep a reference to the renderTarget so we can update it later
            "Loading Menu...".Render(out var menuTarget);
            "Click on a menu item above".Render(out var contentTarget);

            // b. Create actions/menu items to update the contentTarget depending on whats invoked
            var hiAction = SeeRawTypes.Action("Say Hi", () => SayHi(contentTarget));
            var calcAction = SeeRawTypes.Action("Calculator", () => Calc(contentTarget));

            // c. Update the menuTarget to display the actions
            menuTarget.Value = new List<object> { hiAction, calcAction };

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        static void SayHi(RenderTarget contentTarget)
        {
            contentTarget.Value = SeeRawTypes.Action("Please enter your details", (string name) => contentTarget.Value = $"Hi {name}");
        }

        static void Calc(RenderTarget contentTarget)
        {
            contentTarget.Value = SeeRawTypes.Action("Add", (int a, int b) => contentTarget.Value = $"Result: {a + b}");
        }
    }
}
