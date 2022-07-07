using System;

namespace Eva.Util.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class InspectHiddenAttribute : Attribute
    {
        public InspectHiddenAttribute()
        {
        }
    }
}