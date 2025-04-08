using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Service.Helper
{
    public static class EnumHelper
    {
        public static string GetEnumDescription<TEnum>(int value) where TEnum : Enum
        {
            TEnum enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value);
            return GetEnumDescription(enumValue);
        }

        private static string GetEnumDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute = (DescriptionAttribute)field.GetCustomAttribute(typeof(DescriptionAttribute));

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static T GetEnumValueFromDescription<T>(string description) where T : Enum
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException($"{type} is not an enum type");

            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null && attribute.Description.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException($"No enum value found for description '{description}' in {type}");
        }
    }
}
