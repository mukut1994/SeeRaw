using System;
using System.Collections.Generic;
using System.Text;

namespace MK94.SeeRaw
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SeeRawTypeAttribute : Attribute
    {
        public SeeRawTypeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
