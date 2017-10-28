using System.ComponentModel;
using System.Reflection;

namespace Sovos.SvcBus.Common.Model.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDescription<T>(this T message) where T: struct
        {
            var attributes = (DescriptionAttribute[])message.GetType()
                                            .GetTypeInfo().GetField(message.ToString())
                                            .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
