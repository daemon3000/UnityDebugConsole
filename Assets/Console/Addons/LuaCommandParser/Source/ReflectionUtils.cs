using System;
using System.Reflection;

namespace Luminosity.Console.Reflection
{
	public static class ReflectionUtils
	{
		public static Assembly GetAssembly(Type type)
		{
#if UNITY_WSA && !UNITY_EDITOR
			var typeInfo = type.GetTypeInfo();
			return typeInfo.Assembly;
#else
			return type.Assembly;
#endif
		}

		public static bool IsAbstract(Type type)
		{
#if UNITY_WSA && !UNITY_EDITOR
			var typeInfo = type.GetTypeInfo();
			return typeInfo.IsAbstract;
#else
			return type.IsAbstract;
#endif
		}

		public static T GetCustomAttribute<T>(MethodInfo methodInfo) where T : Attribute
		{
#if UNITY_WSA && !UNITY_EDITOR
			return methodInfo.GetCustomAttribute<T>();
#else
			return Attribute.GetCustomAttribute(methodInfo, typeof(T)) as T;
#endif
		}

		public static T GetCustomAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
		{
#if UNITY_WSA && !UNITY_EDITOR
			return propertyInfo.GetCustomAttribute<T>();
#else
			return Attribute.GetCustomAttribute(propertyInfo, typeof(T)) as T;
#endif
		}
	}
}