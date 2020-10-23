using System;

namespace MK94.SeeRaw
{
    public static class SeeRawDefault
    {
        internal static Lazy<Renderer> instance = new Lazy<Renderer>(() => new Renderer());

        public static void OpenBrowser()
        {
            instance.Value.OpenBrowser();
        }

        public static void SetRenderer(Renderer renderer)
        {
            instance = new Lazy<Renderer>(renderer);
        }
    }
}
