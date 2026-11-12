using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MessagePack;
using NShared.NJit.NCommon;
using NShared.NJit.NGenerator;
using NShared.NJit.NResolver;

namespace NShared.NJit;

public class UsingReflection
{
	public static class PrimitiveHashCodeCalc
	{
		public interface IHashCalc
		{
		}

		public class HashCalcT<T> : IHashCalc
		{
			public Func<T, int> Func { get; private set; }

			public HashCalcT(Func<T, int> func)
			{
				Func = func;
			}
		}

		private static Dictionary<Type, IHashCalc> _dic;

		static PrimitiveHashCodeCalc()
		{
			_dic = new Dictionary<Type, IHashCalc>();
			Add((sbyte value) => value);
			Add((byte value) => value);
			Add((short value) => value);
			Add((ushort value) => value);
			Add((int value) => value);
			Add((uint value) => (int)value);
			Add((long value) => (int)value);
			Add((ulong value) => (int)value);
		}

		private static void Add<T>(Func<T, int> func)
		{
			_dic.Add(typeof(T), new HashCalcT<T>(func));
		}

		public static Func<T, int> Get<T>()
		{
			if (_dic.TryGetValue(typeof(T), out var value))
			{
				return (value as HashCalcT<T>).Func;
			}
			return null;
		}
	}

	public class UnknownComparerUsingReflection<T> : IKeyComparerT<T>, IKeyComparer, IEqualityComparer<T>
	{
		private EqualityComparer<T> _default = EqualityComparer<T>.Default;

		Type IKeyComparer.GetCompareType()
		{
			return typeof(T);
		}

		bool IEqualityComparer<T>.Equals(T x, T y)
		{
			return _default.Equals(x, y);
		}

		int IEqualityComparer<T>.GetHashCode(T obj)
		{
			return _default.GetHashCode(obj);
		}
	}

	public class UnknownResolverUsingReflection<T> : IDataResolverT<T>, IDataResolver
	{
		private Type HandleType => typeof(T);

		Type IDataResolver.GetResolveType()
		{
			return HandleType;
		}

		byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
		{
			return SimpleBufferWriter.DirectWriteByMsgPack(0);
		}

		public byte[] SerializeValue(T value, JitSerializer jitSerializer)
		{
			return SimpleBufferWriter.DirectWriteByMsgPack(0);
		}

		public byte[] SerializeReference(T value, JitSerializer jitSerializer)
		{
			return SimpleBufferWriter.DirectWriteByMsgPack(0);
		}

		public T Evaluate(ByteSpan span, JitPacker packer)
		{
			return default(T);
		}

		public void EvaluateLater(T target, ByteSpan span, JitPacker packer)
		{
		}

		public T EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
		{
			span.ErrorRead(0, out nextSpan);
			return default(T);
		}

		public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
		{
			span.ErrorRead(0, out nextSpan);
		}
	}

	public class PrimitiveComparerUsingReflection<T> : IKeyComparerT<T>, IKeyComparer, IEqualityComparer<T> where T : struct
	{
		private EqualityComparer<T> _default = EqualityComparer<T>.Default;

		private Func<T, int> _hashFunc;

		public PrimitiveComparerUsingReflection()
		{
			_hashFunc = PrimitiveHashCodeCalc.Get<T>();
		}

		Type IKeyComparer.GetCompareType()
		{
			return typeof(T);
		}

		bool IEqualityComparer<T>.Equals(T x, T y)
		{
			return _default.Equals(x, y);
		}

		int IEqualityComparer<T>.GetHashCode(T obj)
		{
			if (_hashFunc != null)
			{
				return _hashFunc(obj);
			}
			return _default.GetHashCode(obj);
		}
	}

	public class PrimitiveValueResolver<T> : IDataResolverT<T>, IDataResolver where T : struct
	{
		private Type HandleType => typeof(T);

		Type IDataResolver.GetResolveType()
		{
			return HandleType;
		}

		byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
		{
			if (obj == null || obj.GetType() != typeof(T))
			{
				return SimpleBufferWriter.ErrorWrite(0);
			}
			return SerializeValue((T)obj, jitSerializer);
		}

		public byte[] SerializeValue(T value, JitSerializer jitSerializer)
		{
			using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
			simpleBufferWriter.WriteByMsgPack(value);
			return simpleBufferWriter.ToArray();
		}

		public byte[] SerializeReference(T value, JitSerializer jitSerializer)
		{
			using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
			simpleBufferWriter.WriteByMsgPack(value);
			return simpleBufferWriter.ToArray();
		}

		public T Evaluate(ByteSpan span, JitPacker packer)
		{
			ByteSpan nextSpan;
			return span.ReadByMsgPackT<T>(0, out nextSpan);
		}

		public void EvaluateLater(T target, ByteSpan span, JitPacker packer)
		{
		}

		public T EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
		{
			return span.ReadByMsgPackT<T>(0, out nextSpan);
		}

		public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
		{
			span.ReadByMsgPackT<T>(0, out nextSpan);
		}
	}

	public class EnumValueResolver<T, TUnderlying> : IDataResolverT<T>, IDataResolver where T : struct, IConvertible where TUnderlying : struct
	{
		private Type HandleType => typeof(T);

		Type IDataResolver.GetResolveType()
		{
			return HandleType;
		}

		byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
		{
			if (obj == null || obj.GetType() != typeof(T))
			{
				return SimpleBufferWriter.ErrorWrite(0);
			}
			return SerializeValue((T)obj, jitSerializer);
		}

		public byte[] SerializeValue(T value, JitSerializer jitSerializer)
		{
			if (typeof(T) == typeof(TUnderlying))
			{
				return SimpleBufferWriter.ErrorWrite(0);
			}
			return Proxy<TUnderlying>.DataResolver.SerializeValue((TUnderlying)(object)value, jitSerializer);
		}

		public byte[] SerializeReference(T value, JitSerializer jitSerializer)
		{
			return SerializeValue(value, jitSerializer);
		}

		public T Evaluate(ByteSpan span, JitPacker packer)
		{
			if (typeof(T) == typeof(TUnderlying))
			{
				return default(T);
			}
			return (T)(object)Proxy<TUnderlying>.DataResolver.Evaluate(span, packer);
		}

		public void EvaluateLater(T target, ByteSpan span, JitPacker packer)
		{
		}

		public T EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
		{
			if (typeof(T) == typeof(TUnderlying))
			{
				span.ErrorRead(0, out nextSpan);
				return default(T);
			}
			return (T)(object)Proxy<TUnderlying>.DataResolver.EvaluateReference(span, packer, out nextSpan);
		}

		public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
		{
			if (typeof(T) == typeof(TUnderlying))
			{
				span.ErrorRead(0, out nextSpan);
			}
			else
			{
				Proxy<TUnderlying>.DataResolver.SkipEvaluateReference(span, out nextSpan);
			}
		}
	}

	public class JitKeyObjectComparerUsingReflection<T> : IKeyComparerT<T>, IKeyComparer, IEqualityComparer<T>
	{
		Type IKeyComparer.GetCompareType()
		{
			return typeof(T);
		}

		bool IEqualityComparer<T>.Equals(T x, T y)
		{
			Type typeFromHandle = typeof(T);
			MethodInfo method = typeFromHandle.GetMethod("Equals", new Type[1] { typeFromHandle });
			if (method == null)
			{
				return false;
			}
			object obj = method.Invoke(x, new object[1] { y });
			if (obj is bool)
			{
				return (bool)obj;
			}
			return false;
		}

		int IEqualityComparer<T>.GetHashCode(T obj)
		{
			MethodInfo method = typeof(T).GetMethod("GetHashCode", Type.EmptyTypes);
			if (method == null)
			{
				return obj.GetHashCode();
			}
			object obj2 = method.Invoke(obj, new object[0]);
			if (obj2 is int)
			{
				return (int)obj2;
			}
			return obj.GetHashCode();
		}
	}

	public static object GetMemberValue(MemberInfo info, object obj)
	{
		if (info == null)
		{
			return null;
		}
		PropertyInfo propertyInfo = info as PropertyInfo;
		FieldInfo fieldInfo = info as FieldInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(obj, null);
		}
		if (fieldInfo != null)
		{
			return fieldInfo.GetValue(obj);
		}
		return null;
	}

	public static Type GetMemberValueType(MemberInfo info)
	{
		if (info == null)
		{
			return null;
		}
		PropertyInfo propertyInfo = info as PropertyInfo;
		FieldInfo fieldInfo = info as FieldInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.PropertyType;
		}
		if (fieldInfo != null)
		{
			return fieldInfo.FieldType;
		}
		return typeof(object);
	}

	public static List<Type> GetDataContainerList()
	{
		Type dataContainerType = typeof(IJitDataContainer);
		return dataContainerType.Assembly.GetTypes().Where(delegate(Type type)
		{
			if (!dataContainerType.IsAssignableFrom(type))
			{
				return false;
			}
			return !type.IsAbstract;
		}).ToList();
	}

	public static Type[] GetGenericArgs(Type dataContainerType, Type genericTypeDef)
	{
		Type type = dataContainerType;
		while (type != typeof(object))
		{
			Type type2 = type;
			type = type2.BaseType;
			if (type2.IsGenericType)
			{
				Type genericTypeDefinition = type2.GetGenericTypeDefinition();
				if (!(genericTypeDefinition == null) && !(genericTypeDefinition != genericTypeDef))
				{
					return type2.GetGenericArguments();
				}
			}
		}
		return null;
	}

	public static bool IsJitKeyObject(Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(JitKeyAttribute), inherit: false);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return false;
		}
		MethodInfo method = type.GetMethod("Equals", new Type[1] { type });
		if (method == null || method.DeclaringType == typeof(ValueType) || method.DeclaringType == typeof(object))
		{
			return false;
		}
		MethodInfo method2 = type.GetMethod("GetHashCode", Type.EmptyTypes);
		if (method2 == null || method2.DeclaringType == typeof(ValueType) || method2.DeclaringType == typeof(object))
		{
			return false;
		}
		return true;
	}

	public static bool IsMessagePackObject(Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(MessagePackObjectAttribute), inherit: false);
		if (customAttributes != null)
		{
			return customAttributes.Length != 0;
		}
		return false;
	}

	public static IKeyComparer CreateUnknownComparer(Type type)
	{
		return CreateForGeneric<IKeyComparer>(typeof(UnknownComparerUsingReflection<>), new Type[1] { type });
	}

	public static IKeyComparer CreateComparer(Type type)
	{
		return null;
	}

	public static IKeyComparer CreateComparerForJitKey(Type type)
	{
		if (!IsJitKeyObject(type))
		{
			return null;
		}
		return CreateForGeneric<IKeyComparer>(typeof(JitKeyObjectComparerUsingReflection<>), new Type[1] { type });
	}

	public static IDataResolver CreateUnknownResolver(Type type)
	{
		return CreateForGeneric<IDataResolver>(typeof(UnknownResolverUsingReflection<>), new Type[1] { type });
	}

	public static IDataResolver CreateResolver(Type type)
	{
		return null;
	}

	public static IDataResolver CreateResolverForMessagePackObject(Type type)
	{
		if (!IsMessagePackObject(type))
		{
			return null;
		}
		if (type.IsClass)
		{
			return CreateForGeneric<IDataResolver>(typeof(MessagePackClassResolver<>), new Type[1] { type });
		}
		return CreateForGeneric<IDataResolver>(typeof(MessagePackStructResolver<>), new Type[1] { type });
	}

	private static T CreateForGeneric<T>(Type genericBase, params Type[] genericArgs) where T : class
	{
		Type type = genericBase.MakeGenericType(genericArgs);
		object[] emptyTypes = Type.EmptyTypes;
		return Activator.CreateInstance(type, emptyTypes) as T;
	}

	public static object[] GetMessagePackMember<T>(T value)
	{
		SortedList<int, object> sortedList = new SortedList<int, object>();
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);
		if (fields != null)
		{
			foreach (FieldInfo fieldInfo in fields)
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(KeyAttribute), inherit: false);
				if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is KeyAttribute { IntKey: not null } keyAttribute)
				{
					object value2 = fieldInfo.GetValue(value);
					if (value2 != null)
					{
						sortedList.Add(keyAttribute.IntKey.Value, value2);
					}
				}
			}
		}
		PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
		if (properties != null)
		{
			foreach (PropertyInfo propertyInfo in properties)
			{
				object[] customAttributes2 = propertyInfo.GetCustomAttributes(typeof(KeyAttribute), inherit: false);
				if (customAttributes2 == null || customAttributes2.Length == 0 || !(customAttributes2[0] is KeyAttribute { IntKey: not null } keyAttribute2))
				{
					continue;
				}
				object obj = propertyInfo.GetValue(value, null);
				if (obj == null)
				{
					if (!(propertyInfo.PropertyType == typeof(string)))
					{
						continue;
					}
					obj = string.Empty;
				}
				sortedList.Add(keyAttribute2.IntKey.Value, obj);
			}
		}
		return sortedList.Values.ToArray();
	}

	public static string GetErrorMsgFromKeyType(Type type)
	{
		if (type.IsPrimitive)
		{
			return "Unhandled Primitive";
		}
		object[] customAttributes = type.GetCustomAttributes(typeof(JitKeyAttribute), inherit: false);
		if (customAttributes != null && customAttributes.Length != 0)
		{
			string text = "Invalid JitKey";
			MethodInfo method = type.GetMethod("Equals", new Type[1] { type });
			if (method == null || method.DeclaringType == typeof(ValueType) || method.DeclaringType == typeof(object))
			{
				text += ": need \"Equals(...)\" Function";
			}
			MethodInfo method2 = type.GetMethod("GetHashCode", Type.EmptyTypes);
			if (method2 == null || method2.DeclaringType == typeof(ValueType) || method2.DeclaringType == typeof(object))
			{
				text += ": need \"GetHashCode()\" Function";
			}
			return text;
		}
		return "Unknown";
	}

	public static bool EqualsMessagePack<T>(T x, T y)
	{
		if (!IsMessagePackObject(typeof(T)))
		{
			return x.Equals(y);
		}
		object[] messagePackMember = GetMessagePackMember(x);
		object[] messagePackMember2 = GetMessagePackMember(y);
		return EqualsObject(messagePackMember, messagePackMember2);
	}

	public static bool EqualsObject(object x, object y)
	{
		if (x == null && y == null)
		{
			return true;
		}
		if (x != null && y == null)
		{
			return false;
		}
		if (x == null && y != null)
		{
			return false;
		}
		Type type = x.GetType();
		Type type2 = y.GetType();
		if (type != type2)
		{
			return false;
		}
		object[] arrayFromEnumerable = GetArrayFromEnumerable(x);
		object[] arrayFromEnumerable2 = GetArrayFromEnumerable(y);
		if (arrayFromEnumerable == null || arrayFromEnumerable2 == null)
		{
			return x.Equals(y);
		}
		if (arrayFromEnumerable.Length != arrayFromEnumerable2.Length)
		{
			return false;
		}
		for (int i = 0; i < arrayFromEnumerable.Length; i++)
		{
			object x2 = arrayFromEnumerable[i];
			object y2 = arrayFromEnumerable2[i];
			if (!EqualsObject(x2, y2))
			{
				return false;
			}
		}
		return true;
	}

	private static object[] GetArrayFromEnumerable(object obj)
	{
		if (!(obj is IEnumerable enumerable))
		{
			return null;
		}
		List<object> list = new List<object>();
		foreach (object item in enumerable)
		{
			list.Add(item);
		}
		return list.ToArray();
	}

	public static void WriteTypeDefinition(Type type, StringBuilder sb, string prefix)
	{
		sb.AppendLine(prefix + type.ToString());
		if (GeneratorRoot.PrimitiveTypes.Contains(type))
		{
			sb.AppendLine(prefix + " is Primitive");
		}
		else if (type.IsEnum)
		{
			sb.AppendLine(prefix + " is Enum");
		}
		else if (type == typeof(string))
		{
			sb.AppendLine(prefix + " is String");
		}
		else if (IsMessagePackObject(type))
		{
			WriteMessagePackDefinition(type, sb, prefix + "  ");
		}
		else
		{
			if (!type.IsClass || !type.IsGenericType)
			{
				return;
			}
			Type[] genericArguments = type.GetGenericArguments();
			if (genericArguments != null)
			{
				for (int i = 0; i < genericArguments.Length; i++)
				{
					WriteTypeDefinition(genericArguments[i], sb, prefix + "  ");
				}
			}
		}
	}

	private static void WriteMessagePackDefinition(Type type, StringBuilder sb, string prefix)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		if (fields != null)
		{
			SortedList<int, FieldInfo> sortedList = new SortedList<int, FieldInfo>();
			foreach (FieldInfo fieldInfo in fields)
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(KeyAttribute), inherit: false);
				if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is KeyAttribute { IntKey: var intKey } keyAttribute)
				{
					if (!intKey.HasValue)
					{
						sb.AppendLine(prefix + $"?: {fieldInfo.FieldType} {fieldInfo.Name}");
					}
					else
					{
						sortedList.Add(keyAttribute.IntKey.Value, fieldInfo);
					}
				}
			}
			foreach (KeyValuePair<int, FieldInfo> item in sortedList)
			{
				sb.AppendLine(prefix + $"{item.Key}: {item.Value.FieldType} {item.Value.Name}");
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		if (properties == null)
		{
			return;
		}
		SortedList<int, PropertyInfo> sortedList2 = new SortedList<int, PropertyInfo>();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object[] customAttributes2 = propertyInfo.GetCustomAttributes(typeof(KeyAttribute), inherit: false);
			if (customAttributes2 != null && customAttributes2.Length != 0 && customAttributes2[0] is KeyAttribute { IntKey: var intKey2 } keyAttribute2)
			{
				if (!intKey2.HasValue)
				{
					sb.AppendLine(prefix + $"?: {propertyInfo.PropertyType} {propertyInfo.Name}");
				}
				else
				{
					sortedList2.Add(keyAttribute2.IntKey.Value, propertyInfo);
				}
			}
		}
		foreach (KeyValuePair<int, PropertyInfo> item2 in sortedList2)
		{
			sb.AppendLine(prefix + $"{item2.Key}: {item2.Value.PropertyType} {item2.Value.Name}");
		}
	}
}
