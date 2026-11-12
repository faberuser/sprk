using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Reflection;

namespace NShared.NJit.NCodeMD5;

public class CodeMD5_Calculator
{
	public class ImplementInfo
	{
		public List<FieldInfo> FieldInfoList = new List<FieldInfo>();

		public List<PropertyInfo> PropertyInfoList = new List<PropertyInfo>();

		public List<MethodInfo> MethodInfoList = new List<MethodInfo>();

		public List<ConstructorInfo> ConstructorInfoList = new List<ConstructorInfo>();

		public HashSet<Type> MemberTypes_in_Class = new HashSet<Type>();

		private DumpWriter _dumpWriter = new DumpWriter();

		public Type Type { get; private set; }

		public void Build(Type type)
		{
			Type = type;
			FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(fields))
			{
				FieldInfoList.AddRange(fields);
				FieldInfoList.Sort((FieldInfo lhs, FieldInfo rhs) => lhs.Name.CompareTo(rhs.Name));
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(properties))
			{
				PropertyInfoList.AddRange(properties);
				PropertyInfoList.Sort((PropertyInfo lhs, PropertyInfo rhs) => lhs.Name.CompareTo(rhs.Name));
			}
			MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(methods))
			{
				MethodInfoList.AddRange(methods);
				MethodInfoList.Sort((MethodInfo lhs, MethodInfo rhs) => lhs.Name.CompareTo(rhs.Name));
			}
			ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(constructors))
			{
				ConstructorInfoList.AddRange(constructors);
			}
			__Extension_Method_ICollection__.ADDRANGE(MemberTypes_in_Class, FieldInfoList.Select((FieldInfo v) => v.FieldType));
			__Extension_Method_ICollection__.ADDRANGE(MemberTypes_in_Class, PropertyInfoList.Select((PropertyInfo v) => v.PropertyType));
			__Extension_Method_ICollection__.ADDRANGE(MemberTypes_in_Class, MethodInfoList.Select((MethodInfo v) => v.ReturnType));
			BuildBuffer();
			_dumpWriter.EndDump();
		}

		public void BuildBuffer()
		{
			if (Type.IsEnum)
			{
				BuildBuffer_Type_Enum();
				return;
			}
			string value = $"[Build Type] {Type.ToString()}\n";
			_dumpWriter.Write(value);
			foreach (FieldInfo fieldInfo in FieldInfoList)
			{
				string value2 = $" - [Field] {fieldInfo.Name} ({fieldInfo.FieldType})";
				_dumpWriter.Write(value2);
			}
			foreach (PropertyInfo propertyInfo in PropertyInfoList)
			{
				string value3 = $" - [Property] {propertyInfo.Name} ({propertyInfo.PropertyType})";
				_dumpWriter.Write(value3);
			}
			foreach (MethodInfo methodInfo in MethodInfoList)
			{
				string value4 = $" - [Method] {methodInfo.Name} ({methodInfo.ReturnType}): {methodInfo.DeclaringType}\n";
				_dumpWriter.Write(value4);
				BuildBuffer_for_MethodBody(methodInfo, methodInfo.ReturnType);
			}
			foreach (ConstructorInfo constructorInfo in ConstructorInfoList)
			{
				string value5 = $" - [Constructor] {constructorInfo.Name}: {constructorInfo.DeclaringType}\n";
				_dumpWriter.Write(value5);
				BuildBuffer_for_MethodBody(constructorInfo, typeof(void));
			}
		}

		private void BuildBuffer_Type_Enum()
		{
			foreach (object value in Enum.GetValues(Type))
			{
				_dumpWriter.Write(value.ToString());
			}
		}

		private void BuildBuffer_for_MethodBody(MethodBase methodBase, Type returnType)
		{
			if (Type.IsArray)
			{
				return;
			}
			string error;
			List<Instruction> instructions = Mono.Reflection.MethodBodyReader.GetInstructions(methodBase, out error);
			if (instructions == null)
			{
				string value = $" - [GetMethodBody failed] [DeclaringType={methodBase.DeclaringType}] {methodBase.Name} {returnType}";
				_dumpWriter.Write(value);
				return;
			}
			string value2 = $" - [GetMethodBody] [DeclaringType={methodBase.DeclaringType}] {methodBase.Name} {returnType}";
			_dumpWriter.Write(value2);
			foreach (Instruction item in instructions)
			{
				_dumpWriter.Write(item);
			}
		}

		public void Dump(DumpWriter writer)
		{
			writer.AppendWriter(_dumpWriter);
		}
	}

	public class TypeInfoContainer_for_Shared
	{
		private static Dictionary<Type, ImplementInfo> _dic = new Dictionary<Type, ImplementInfo>();

		public static void Reset()
		{
			lock (_dic)
			{
				_dic.Clear();
			}
		}

		public static ImplementInfo GetImplementInfo(Type type)
		{
			lock (_dic)
			{
				if (_dic.TryGetValue(type, out var value))
				{
					return value;
				}
				if (String_Extension_Method.IsNullOrEmpty(type.Namespace))
				{
					return null;
				}
				if (!type.Namespace.StartsWith("NShared"))
				{
					return null;
				}
				value = new ImplementInfo();
				value.Build(type);
				_dic.Add(type, value);
				return value;
			}
		}
	}

	private HashSet<Type> _checkedType = new HashSet<Type>();

	private List<Type> _list = new List<Type>();

	public void Calculate(Type rootType, DumpWriter writer)
	{
		_list.Add(rootType);
		for (int i = 0; i < _list.Count; i++)
		{
			Type type = _list[i];
			if (type == null || _checkedType.Contains(type))
			{
				continue;
			}
			_checkedType.Add(type);
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				string value = $" (skip) [{type.ToString()}]";
				writer.Write(value);
				Add_GenericTypeDefinition_and_Arguments(type);
				continue;
			}
			string value2 = $"[{type.ToString()}]";
			writer.Write(value2);
			ImplementInfo implementInfo = TypeInfoContainer_for_Shared.GetImplementInfo(type);
			if (implementInfo == null)
			{
				continue;
			}
			implementInfo.Dump(writer);
			if (type.IsClass)
			{
				_list.Add(type.BaseType);
			}
			foreach (Type item in implementInfo.MemberTypes_in_Class)
			{
				_list.Add(item);
			}
		}
	}

	private void Add_GenericTypeDefinition_and_Arguments(Type genericType)
	{
		if (genericType.IsClass && genericType.IsGenericType && !genericType.IsGenericTypeDefinition)
		{
			Type genericTypeDefinition = genericType.GetGenericTypeDefinition();
			_list.Add(genericTypeDefinition);
			Type[] genericArguments = genericType.GetGenericArguments();
			if (__Extension_Method_ICollection__.COUNT(genericArguments) > 0)
			{
				_list.AddRange(genericArguments);
			}
		}
	}

	public static byte[] CalcMD5(Type type, string namePostfix)
	{
		DumpWriter dumpWriter = new DumpWriter();
		new CodeMD5_Calculator().Calculate(type, dumpWriter);
		dumpWriter.EndDump();
		dumpWriter.DumpTextToFile(type, "e:/dump/CodeMD5", namePostfix);
		return dumpWriter.MD5Bytes;
	}
}
