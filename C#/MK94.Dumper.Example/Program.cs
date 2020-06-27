using System;
using System.Collections.Generic;
using System.Reflection;

namespace MK94.SirUI.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            new List<object> { new Test { I = 2, A = "OK" }, 2, 3 }.Render(out var x);
            "ok".Render();
            1.Render();

            new Link("set x to 1", () => x.Value = 1).Render();

            while (true)
            {
                x.Value = Console.ReadLine();
            }
        }
    }

    class Test
    {
        public int I { get; set; }

        public string A { get; set; }
    }
}
