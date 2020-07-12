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
            SeeRaw.OpenDefaultBrowser();

            var list = new List<object> { new Test { I = 2, A = "OK" }, 2, 3 };

            // Render out some things
            "ok".Render();
            1.Render();

            // We can optionally render things and receive a reference to update it later
            list.Render(out var renderTarget);

            // Render a link to update the list
            var link = new Link("set list to '1'", () => renderTarget.Value = 1);
            link.Render();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    class Test
    {
        public int I { get; set; }

        public string A { get; set; }
    }
}
