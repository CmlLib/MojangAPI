using System;
using System.Collections.Generic;
using System.Text;

namespace MojangAPI.Model
{
    public enum SkinType 
    {
        Steve, 
        Alex
    }

    public static class SkinTypeExtension
    {
        public static string GetModelType(this SkinType skinType)
        {
            switch (skinType)
            {
                case SkinType.Alex:
                    return "slim";
                case SkinType.Steve:
                    return "classic";
                default:
                    return "";
            }
        }
    }
}
