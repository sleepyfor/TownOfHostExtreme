using System.Collections.Generic;

namespace TOHX.Roles.AddOns.Common
{
    public static class DoubleShot
    {
        public static List<byte> IsActive = new();
        public static void Init()
        {
            IsActive = new();
        }
    }
}