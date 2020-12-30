using System;
using System.IO;

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
            if(SeeRawContext.Current == null)
                throw new InvalidProgramException($"Default renderer is not set for extension method. Call {nameof(SeeRawSetup)}.{nameof(SeeRawSetup.WithServer)}().{nameof(SeeRawSetup.WithGlobalRenderer)}() first");

            target = SeeRawContext.RenderRoot.Render(obj);

            return obj;
        }

        public static T Render<T>(this T obj, string name)
        {
            if (SeeRawContext.Current == null)
                throw new InvalidProgramException($"Default renderer is not set for extension method. Call {nameof(SeeRawSetup)}.{nameof(SeeRawSetup.WithServer)}().{nameof(SeeRawSetup.WithGlobalRenderer)}() first");

            SeeRawContext.RenderRoot.Render(obj, name);

            return obj;
        }
    }
}
