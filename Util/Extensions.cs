using System;
using System.Collections.Generic;

namespace Eva.Util.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> TypeToFriendlyName = new()
        {
            [typeof(string)] = "string",
            [typeof(object)] = "object",
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(char)] = "char",
            [typeof(decimal)] = "decimal",
            [typeof(double)] = "double",
            [typeof(short)] = "short",
            [typeof(int)] = "int",
            [typeof(long)] = "long",
            [typeof(sbyte)] = "sbyte",
            [typeof(float)] = "float",
            [typeof(ushort)] = "ushort",
            [typeof(uint)] = "uint",
            [typeof(ulong)] = "ulong",
            [typeof(void)] = "void"
        };

#nullable enable
        public static string GetFriendlyName(this Type type)
        {
            if (TypeToFriendlyName.TryGetValue(type, out string? friendlyName)) return friendlyName;

            friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int backtick = friendlyName.IndexOf('`');
                if (backtick > 0)
                {
                    friendlyName = friendlyName.Remove(backtick);
                }

                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; i++)
                {
                    string typeParamName = typeParameters[i].GetFriendlyName();
                    friendlyName += (i == 0 ? typeParamName : ", " + typeParamName);
                }

                friendlyName += ">";
            }

            if (type.IsArray)
            {
                return type.GetElementType().GetFriendlyName() + "[]";
            }

            return friendlyName;
        }
    }
}