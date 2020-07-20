using System;

namespace MK94.SeeRaw
{
    public static class Extensions
    {
        internal static Lazy<Renderer> instance = new Lazy<Renderer>(() => new Renderer());

        public static T Render<T>(this T obj)
        {
            return obj.Render(out _);
        }

        public static T Render<T>(this T obj, out RenderTarget target)
        {
            instance.Value.Render(obj, out target);

            return obj;
        }
    }

    public static class SeeRawDefault
    {
        public static void OpenBrowser()
        {
            Extensions.instance.Value.OpenBrowser();
        }

        public static void SetRenderer(Renderer renderer)
        {
            Extensions.instance = new Lazy<Renderer>(renderer);
        }
    }
}
