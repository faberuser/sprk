using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniJSON;
using NShared;
using NShared.NUtil;
using NVespa;

public class JsonMarshalerUsingReflection
{
	public static T Clone<T>(T source)
	{
		return Parse<T>(EncodeJsonString(source));
	}

	public static object Parse(Type t, string s)
	{
		IDictionary src = Json.Deserialize(s) as IDictionary;
		return Parse(t, src);
	}

	public static T ParseOrDefaultPrimitive<T>(IDictionary jsonDic, string keyword) where T : struct
	{
		if (!TryParsePrimitive<T>(jsonDic, keyword, out var value))
		{
			value = default(T);
		}
		return value;
	}

	public static bool TryParsePrimitive<T>(IDictionary jsonDic, string keyword, out T value) where T : struct
	{
		if (jsonDic == null)
		{
			value = default(T);
			return false;
		}
		if (!jsonDic.Contains(keyword))
		{
			value = default(T);
			return false;
		}
		object obj = Parse(typeof(T), jsonDic[keyword]);
		if (!(obj is T))
		{
			value = default(T);
			return false;
		}
		value = (T)obj;
		return true;
	}

	public static object Parse(Type type, object src)
	{
		if (src == null)
		{
			return null;
		}
		if (type.IsArray && !typeof(IList).IsAssignableFrom(src.GetType()))
		{
			return null;
		}
		if (typeof(IList).IsAssignableFrom(src.GetType()))
		{
			return Parse(type, (IList)src);
		}
		if (typeof(IDictionary).IsAssignableFrom(type))
		{
			return ParseDictionary(type, (IDictionary)src);
		}
		if (typeof(IDictionary).IsAssignableFrom(src.GetType()))
		{
			return Parse(type, (IDictionary)src);
		}
		if (type == typeof(HashString))
		{
			return BaseJsonMarshaler.CastToHashString(src);
		}
		if (type == typeof(Vector3))
		{
			IDictionary dictionary = Json.Deserialize(src as string) as IDictionary;
			return new Vector3
			{
				Mx = BaseJsonMarshaler.ParseValueLong(dictionary, "Mx"),
				My = BaseJsonMarshaler.ParseValueLong(dictionary, "My"),
				Mz = BaseJsonMarshaler.ParseValueLong(dictionary, "Mz")
			};
		}
		try
		{
			if (type.IsEnum)
			{
				return Convert.ChangeType(Enum.Parse(type, src.ToString()), type);
			}
			return Convert.ChangeType(src, type);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static object Parse(Type type, IDictionary src)
	{
		object obj = null;
		try
		{
			obj = Activator.CreateInstance(type);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to activate type: " + type.Name + " error=" + ex);
			return null;
		}
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			Type propertyType = propertyInfo.PropertyType;
			string name = propertyInfo.Name;
			if (!src.Contains(name))
			{
				continue;
			}
			object obj2 = src[name];
			if (obj2 != null)
			{
				object obj3 = Parse(propertyType, obj2);
				if (obj3 != null && propertyInfo.CanWrite)
				{
					propertyInfo.SetValue(obj, obj3, null);
				}
			}
		}
		return obj;
	}

	public static T Parse<T>(IDictionary src, string prefix, string postfix) where T : class
	{
		return Parse(typeof(T), src, prefix, postfix) as T;
	}

	public static object Parse(Type type, IDictionary src, string prefix, string postfix)
	{
		object obj = null;
		try
		{
			obj = Activator.CreateInstance(type);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to activate type: " + type.Name + " error=" + ex);
			return null;
		}
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			Type propertyType = propertyInfo.PropertyType;
			string name = propertyInfo.Name;
			string key = prefix + name + postfix;
			if (!src.Contains(key))
			{
				continue;
			}
			object obj2 = src[key];
			if (obj2 != null)
			{
				object obj3 = Parse(propertyType, obj2);
				if (obj3 != null && propertyInfo.CanWrite)
				{
					propertyInfo.SetValue(obj, obj3, null);
				}
			}
		}
		return obj;
	}

	public static object ParseDictionary(Type type, IDictionary src)
	{
		IDictionary dictionary = null;
		try
		{
			dictionary = Activator.CreateInstance(type) as IDictionary;
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to activate type: " + type.Name + " error=" + ex);
			return null;
		}
		Type[] genericArguments = type.GetGenericArguments();
		if (genericArguments.Count() < 2)
		{
			return null;
		}
		Type type2 = genericArguments[0];
		Type type3 = genericArguments[1];
		foreach (object key in src.Keys)
		{
			object obj = src[key];
			if (obj == null)
			{
				continue;
			}
			object obj2 = Parse(type2, key);
			if (obj2 != null)
			{
				object obj3 = Parse(type3, obj);
				if (obj3 != null)
				{
					dictionary.Add(obj2, obj3);
				}
			}
		}
		return dictionary;
	}

	public static object Parse(Type type, IList src)
	{
		Type type2 = null;
		IList list = null;
		try
		{
			if (type.IsArray)
			{
				type2 = type.GetElementType();
				list = Array.CreateInstance(type2, src.Count);
			}
			else
			{
				Type[] genericArguments = type.GetGenericArguments();
				if (genericArguments != null && genericArguments.Length != 0)
				{
					type2 = genericArguments[0];
					list = Activator.CreateInstance(type) as IList;
				}
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to create IList: type=" + type.Name + " src=" + src.GetType()?.ToString() + " error=" + ex);
		}
		if (list == null)
		{
			return JsonPool.instance.CreateList();
		}
		for (int i = 0; i < src.Count; i++)
		{
			object obj = src[i];
			if (obj == null)
			{
				continue;
			}
			object value = Parse(type2, obj);
			try
			{
				if (type.IsArray)
				{
					list[i] = value;
				}
				else
				{
					list.Insert(i, value);
				}
			}
			catch (Exception ex2)
			{
				VespaDebug.LogError("Failed to create IList: type=" + type.Name + " src=" + src.GetType()?.ToString() + " error=" + ex2);
			}
		}
		return list;
	}

	public static T Parse<T>(string s)
	{
		return Parse<T>(Json.Deserialize(s) as IDictionary);
	}

	public static T Parse<T>(IDictionary d)
	{
		Type typeFromHandle = typeof(T);
		return (T)Convert.ChangeType(Parse(typeFromHandle, d), typeFromHandle);
	}

	public static string EncodeJsonString<T>(T o)
	{
		return Json.Serialize(EncodeJsonObjectForString(o));
	}

	public static Dictionary<string, object> EncodeJsonObjectForString(object obj)
	{
		Dictionary<string, object> dictionary = JsonPool.instance.CreateDic();
		if (obj == null)
		{
			return dictionary;
		}
		PropertyInfo[] properties = obj.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			Type propertyType = propertyInfo.PropertyType;
			string name = propertyInfo.Name;
			if (!(propertyInfo == null) && propertyInfo.CanWrite)
			{
				object value = propertyInfo.GetValue(obj, null);
				object obj2 = EncodeJsonObject(propertyType, value);
				if (obj2 != null)
				{
					dictionary.Add(name, obj2);
				}
			}
		}
		return dictionary;
	}

	public static object EncodeJsonObject(Type type, object obj)
	{
		object obj2 = null;
		if (typeof(IList).IsAssignableFrom(type) || (obj != null && typeof(IList).IsAssignableFrom(obj.GetType())))
		{
			return EncodeJsonObjectForString(type, obj as IList);
		}
		if (obj != null && typeof(IDictionary).IsAssignableFrom(obj.GetType()))
		{
			return EncodeJsonObjectForString(type, obj as IDictionary);
		}
		if (type == typeof(HashString))
		{
			return BaseJsonMarshaler.CastToHashString(obj);
		}
		if (type == typeof(string))
		{
			return obj;
		}
		if (type == typeof(Vector3))
		{
			Vector3 vector = (Vector3)obj;
			Dictionary<string, object> dictionary = JsonPool.instance.CreateDic();
			dictionary.Add("Mx", vector.Mx.ToString());
			dictionary.Add("My", vector.My.ToString());
			dictionary.Add("Mz", vector.Mz.ToString());
			return dictionary;
		}
		if (type.IsValueType || type.IsPrimitive)
		{
			return (obj != null) ? obj.ToString() : "";
		}
		return EncodeJsonObjectForString(obj);
	}

	public static object EncodeJsonObjectForString(Type type, IDictionary src)
	{
		Dictionary<object, object> dictionary = new Dictionary<object, object>();
		if (src == null)
		{
			return dictionary;
		}
		Type[] genericArguments = type.GetGenericArguments();
		if (genericArguments.Count() < 2)
		{
			return dictionary;
		}
		Type type2 = genericArguments[0];
		Type type3 = genericArguments[1];
		foreach (object key in src.Keys)
		{
			object obj = src[key];
			if (obj == null)
			{
				continue;
			}
			object obj2 = EncodeJsonObject(type2, key);
			if (obj2 != null)
			{
				object obj3 = EncodeJsonObject(type3, obj);
				if (obj3 != null)
				{
					dictionary.Add(obj2, obj3);
				}
			}
		}
		return dictionary;
	}

	public static object[] EncodeJsonObjectForString(Type type, IList src)
	{
		object[] array = null;
		if (src == null)
		{
			return new object[0];
		}
		array = new object[src.Count];
		for (int i = 0; i < src.Count; i++)
		{
			object obj = src[i];
			if (obj != null)
			{
				array[i] = EncodeJsonObject(obj.GetType(), obj);
			}
		}
		return array;
	}

	public static Dictionary<string, object> EncodeJsonObject<T>(T obj)
	{
		Dictionary<string, object> dictionary = JsonPool.instance.CreateDic();
		if (obj == null)
		{
			return dictionary;
		}
		PropertyInfo[] properties = obj.GetType().GetProperties();
		foreach (PropertyInfo obj2 in properties)
		{
			Type propertyType = obj2.PropertyType;
			string name = obj2.Name;
			object value = obj2.GetValue(obj, null);
			if (value != null)
			{
				object obj3 = EncodeJsonObject(propertyType, value);
				if (obj3 != null)
				{
					dictionary.Add(name, obj3);
				}
			}
		}
		return dictionary;
	}
}
