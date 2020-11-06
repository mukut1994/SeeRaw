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
            if(SeeRawContext.localSeeRawContext == null)
                throw new InvalidProgramException($"Default renderer is not set for extension method. Call {nameof(SeeRawSetup)}.{nameof(SeeRawSetup.WithServer)}().{nameof(SeeRawSetup.WithGlobalRenderer)}() first");

            target = SeeRawContext.localSeeRawContext.Value.RenderRoot.Render(obj);

            return obj;
        }

        public static T Render<T>(this T obj, string name)
        {
            if (SeeRawContext.localSeeRawContext == null)
                throw new InvalidProgramException($"Default renderer is not set for extension method. Call {nameof(SeeRawSetup)}.{nameof(SeeRawSetup.WithServer)}().{nameof(SeeRawSetup.WithGlobalRenderer)}() first");

            SeeRawContext.localSeeRawContext.Value.RenderRoot.Render(obj, name);

            return obj;
        }
    }
}
