using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NShared.NJit.NGenerator;

public class GeneratorRoot
{
	public enum ExitCode
	{
		OK
	}

	public class JitContainerCollector
	{
		private static Type type_of_IJitContainer = typeof(IJitContainer);

		private static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private HashSet<Type> _checkedSet = new HashSet<Type>();

		private List<Type> _checkList = new List<Type>();

		public List<Type> CollectFrom(Type type)
		{
			List<Type> list = new List<Type>();
			_checkList.Add(type);
			for (int i = 0; i < _checkList.Count; i++)
			{
				Type type2 = _checkList[i];
				if (type2 == null || _checkedSet.Contains(type2))
				{
					continue;
				}
				_checkedSet.Add(type2);
				if (type_of_IJitContainer.IsAssignableFrom(type2))
				{
					if (!type2.IsAbstract)
					{
						list.Add(type2);
					}
				}
				else
				{
					AppendCheckList(type2);
				}
			}
			return list;
		}

		private void Add_to_CheckList(Type type)
		{
			_ = type == type_of_IJitContainer;
			_checkList.Add(type);
		}

		private void AppendCheckList(Type type)
		{
			FieldInfo[] fields = type.GetFields(bindingFlags);
			if (fields != null)
			{
				__Extension_Method_ICollection__.ForEach_to_End(fields.Select((FieldInfo v) => v.FieldType), delegate(Type v)
				{
					Add_to_CheckList(v);
				});
			}
			PropertyInfo[] properties = type.GetProperties(bindingFlags);
			if (properties != null)
			{
				__Extension_Method_ICollection__.ForEach_to_End(properties.Select((PropertyInfo v) => v.PropertyType), delegate(Type v)
				{
					Add_to_CheckList(v);
				});
			}
			Add_to_CheckList(type.BaseType);
		}
	}

	public static List<Type> PrimitiveTypes = new List<Type>
	{
		typeof(sbyte),
		typeof(byte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(char),
		typeof(bool)
	};

	private HashSet<Type> _keyFilterSet = new HashSet<Type>();

	private List<Type> _keyTypeList = new List<Type>();

	private List<Type> _invalidKeyTypeList = new List<Type>();

	private HashSet<Type> _dataFilterSet = new HashSet<Type>();

	private List<Type> _dataTypeList = new List<Type>();

	private List<Type> _invalidDataTypeList = new List<Type>();

	public int Run(string filename, out string console, out string error)
	{
		console = string.Empty;
		error = string.Empty;
		List<Type> dataContainerList = UsingReflection.GetDataContainerList();
		for (int i = 0; i < dataContainerList.Count; i++)
		{
			Type dataContainerType = dataContainerList[i];
			CollectJitContainerFromDataContainerType(dataContainerType);
		}
		CollectJitContainerFromExternalUsage();
		for (int j = 0; j < _dataTypeList.Count; j++)
		{
			Type type = _dataTypeList[j];
			CollectChildDataTypeFromType(type);
		}
		for (int k = 0; k < _keyTypeList.Count; k++)
		{
			Type type2 = _keyTypeList[k];
			CollectChildKeyTypeFromType(type2);
		}
		CodeGenerator codeGenerator = new CodeGenerator();
		codeGenerator.GenCode_String();
		codeGenerator.GenCode_Bool();
		for (int l = 0; l < PrimitiveTypes.Count; l++)
		{
			codeGenerator.GenCode_Primitive(PrimitiveTypes[l]);
		}
		for (int m = 0; m < _dataTypeList.Count; m++)
		{
			Type type3 = _dataTypeList[m];
			if (codeGenerator.ResolverFilterSet.Contains(type3))
			{
				continue;
			}
			if (type3.IsEnum)
			{
				codeGenerator.GenCode_Enum(type3);
				continue;
			}
			if (UsingReflection.IsMessagePackObject(type3))
			{
				codeGenerator.GenResolverCode_MessagePack(type3);
				continue;
			}
			if (type3.IsClass && type3.IsGenericType)
			{
				Type resolverGenericTypeDef = JitOption.GetResolverGenericTypeDef(type3.GetGenericTypeDefinition());
				if (resolverGenericTypeDef != null)
				{
					codeGenerator.GenResolverCode_Generic(type3, resolverGenericTypeDef);
					continue;
				}
			}
			_invalidDataTypeList.Add(type3);
		}
		for (int n = 0; n < _keyTypeList.Count; n++)
		{
			Type type4 = _keyTypeList[n];
			if (!codeGenerator.ComparerFilterSet.Contains(type4))
			{
				if (type4.IsEnum)
				{
					codeGenerator.GenCode_Enum(type4);
				}
				else if (!codeGenerator.GenComparerCode_JitKeyObject(type4))
				{
					_invalidKeyTypeList.Add(type4);
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		codeGenerator.Write(stringBuilder);
		File.WriteAllText(filename, stringBuilder.ToString());
		stringBuilder.Length = 0;
		stringBuilder.AppendLine("JitResolver Generated!!!");
		stringBuilder.AppendLine($"Comparer: {codeGenerator.ComparerFilterSet.Count}");
		int num = 0;
		foreach (Type item in codeGenerator.ComparerFilterSet)
		{
			stringBuilder.AppendLine($"  [{num}] {item.ToString()}");
			num++;
		}
		stringBuilder.AppendLine($"Resolver: {codeGenerator.ResolverFilterSet.Count}");
		num = 0;
		foreach (Type item2 in codeGenerator.ResolverFilterSet)
		{
			stringBuilder.AppendLine($"  [{num}] {item2.ToString()}");
			num++;
		}
		console = stringBuilder.ToString();
		if (_invalidKeyTypeList.Count > 0 || _invalidDataTypeList.Count > 0)
		{
			stringBuilder.Length = 0;
			stringBuilder.AppendLine("[[[[ Invalid Type ]]]]");
			for (int num2 = 0; num2 < _invalidKeyTypeList.Count; num2++)
			{
				string errorMsgFromKeyType = UsingReflection.GetErrorMsgFromKeyType(_invalidKeyTypeList[num2]);
				stringBuilder.AppendLine($"KeyType - {_invalidKeyTypeList[num2]} ({errorMsgFromKeyType})");
			}
			for (int num3 = 0; num3 < _invalidDataTypeList.Count; num3++)
			{
				stringBuilder.AppendLine($"DataType - {_invalidDataTypeList[num3]}");
			}
			error = stringBuilder.ToString();
		}
		return 0;
	}

	private void CollectChildKeyTypeFromType(Type type)
	{
		Func<MemberInfo, Type> func = delegate(MemberInfo info)
		{
			if (info == null)
			{
				return (Type)null;
			}
			PropertyInfo propertyInfo = info as PropertyInfo;
			FieldInfo fieldInfo = info as FieldInfo;
			if (propertyInfo != null)
			{
				return propertyInfo.PropertyType;
			}
			return (fieldInfo != null) ? fieldInfo.FieldType : typeof(object);
		};
		object[] customAttributes = type.GetCustomAttributes(typeof(JitKeyAttribute), inherit: false);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return;
		}
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		List<MemberInfo> list = new List<MemberInfo>();
		list.AddRange(type.GetProperties(bindingAttr));
		list.AddRange(type.GetFields(bindingAttr));
		for (int num = 0; num < list.Count; num++)
		{
			MemberInfo arg = list[num];
			Type type2 = func(arg);
			if (!(type2 == null) && type2.IsEnum)
			{
				AppendKeyType(type2);
			}
		}
	}

	private void CollectChildDataTypeFromType(Type type)
	{
		if (!type.IsClass || !type.IsGenericType)
		{
			return;
		}
		Type genericTypeDefinition = type.GetGenericTypeDefinition();
		if (genericTypeDefinition == typeof(List<>))
		{
			Type[] genericArguments = type.GetGenericArguments();
			AppendDataType(genericArguments);
		}
		else if (genericTypeDefinition == typeof(HashSet<>))
		{
			Type[] genericArguments2 = type.GetGenericArguments();
			AppendDataType(genericArguments2);
		}
		else if (genericTypeDefinition == typeof(Dictionary<, >))
		{
			Type[] genericArguments3 = type.GetGenericArguments();
			AppendDataType(genericArguments3);
			if (genericArguments3 != null && genericArguments3.Length != 0)
			{
				AppendKeyType(genericArguments3[0]);
			}
		}
		else if (genericTypeDefinition == typeof(JitRef<>))
		{
			Type[] genericArguments4 = type.GetGenericArguments();
			AppendDataType(genericArguments4);
		}
		else if (genericTypeDefinition == typeof(JitList<>))
		{
			Type[] genericArguments5 = type.GetGenericArguments();
			AppendDataType(genericArguments5);
		}
		else if (genericTypeDefinition == typeof(JitDictionary<, >))
		{
			Type[] genericArguments6 = type.GetGenericArguments();
			AppendDataType(genericArguments6);
			if (genericArguments6 != null && genericArguments6.Length != 0)
			{
				AppendKeyType(genericArguments6[0]);
			}
		}
		else if (genericTypeDefinition == typeof(JitDictionaryWithKey<, >))
		{
			Type[] genericArguments7 = type.GetGenericArguments();
			if (genericArguments7 != null && genericArguments7.Length > 1)
			{
				AppendKeyType(genericArguments7[0]);
				AppendDataType(genericArguments7[1]);
			}
		}
	}

	private void AppendKeyType(Type type)
	{
		if (!(type == null) && !_keyFilterSet.Contains(type))
		{
			_keyFilterSet.Add(type);
			_keyTypeList.Add(type);
		}
	}

	private void AppendDataType(IList<Type> types)
	{
		if (types != null && types.Count != 0)
		{
			for (int i = 0; i < types.Count; i++)
			{
				AppendDataType(types[i]);
			}
		}
	}

	private void AppendDataType(Type type)
	{
		if (!(type == null) && !_dataFilterSet.Contains(type))
		{
			_dataFilterSet.Add(type);
			_dataTypeList.Add(type);
		}
	}

	private void CollectJitContainerFromDataContainerType(Type dataContainerType)
	{
		foreach (Type item in new JitContainerCollector().CollectFrom(dataContainerType))
		{
			AppendDataType(item);
		}
	}

	private void CollectJitContainerFromExternalUsage()
	{
		Type typeFromHandle = typeof(IJitContainer);
		Type[] array = new Type[2]
		{
			typeof(JitDictionary<int, string>),
			typeof(JitDictionary<string, string>)
		};
		foreach (Type type in array)
		{
			if (typeFromHandle.IsAssignableFrom(type))
			{
				AppendDataType(type);
			}
		}
	}
}
