using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NVespa;

namespace NShared;

public class TableDataAutoLoader
{
	public static void Load(GameRoot gameRoot, TableUseProjectType projectType, IJsonLoader jsonLoader, Func<Type, IDictionary, object> parser)
	{
		if (jsonLoader != null)
		{
			gameRoot.SetJsonLoader(jsonLoader);
		}
		MethodInfo[] methods = gameRoot.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
		for (int i = 0; i < methods.Length; i++)
		{
			try
			{
				MethodInfo methodInfo = methods[i];
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(TableLoadAttribute), inherit: false);
				if (customAttributes != null && customAttributes.Length != 0 && (((TableLoadAttribute)customAttributes[0]).Project & projectType) != TableUseProjectType.None)
				{
					Type type = methodInfo.ReturnType.GetInterfaces().SingleOrDefault((Type e) => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IBaseDataContainer<, >)).GetGenericArguments()[1];
					Type typeFromHandle = typeof(Func<, >);
					Type[] typeArguments = new Type[2]
					{
						typeof(IDictionary),
						type
					};
					typeFromHandle.MakeGenericType(typeArguments);
					MethodInfo method = typeof(TableDataAutoLoader).GetMethod("CreateParserFunc", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[1] { typeof(Func<Type, IDictionary, object>) }, null).MakeGenericMethod(type);
					Type type2 = typeof(Func<, , >).MakeGenericType(typeof(Type), typeof(IDictionary), typeof(object));
					Type type3 = typeof(Func<, >).MakeGenericType(typeof(IDictionary), type);
					Delegate obj = Delegate.CreateDelegate(typeof(Func<, >).MakeGenericType(type2, type3), method);
					object[] args = new Func<Type, IDictionary, object>[1] { parser };
					object obj2 = obj.DynamicInvoke(args);
					methodInfo.Invoke(gameRoot, new object[1] { obj2 });
				}
			}
			catch (Exception obj3)
			{
				VespaDebug.LogError(obj3);
			}
		}
	}

	private static Func<IDictionary, T> CreateParserFunc<T>(Func<Type, IDictionary, object> parser)
	{
		return (IDictionary dictionary) => (T)parser(typeof(T), dictionary);
	}
}
