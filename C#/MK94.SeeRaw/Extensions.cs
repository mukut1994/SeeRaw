using System;

namespace MK94.SeeRaw
{
    public static class Extensions
    {
        public static T Render<T>(this T obj)
        {
            return obj.Render(out _);
        }

        public static T Render<T>(this T obj, out RenderTarget target)
        {
            SeeRawDefault.instance.Value.Render(obj, out target);

            return obj;
        }
    }
}
