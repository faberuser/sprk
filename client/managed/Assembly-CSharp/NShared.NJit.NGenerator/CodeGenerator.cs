using System;
using System.Collections.Generic;
using System.Text;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class CodeGenerator
{
	public HashSet<Type> ComparerFilterSet = new HashSet<Type>();

	public List<string> ComparerNameList = new List<string>();

	public List<string> ComparerCodeList = new List<string>();

	public HashSet<string> ComparerDefineSet = new HashSet<string>();

	public HashSet<Type> ResolverFilterSet = new HashSet<Type>();

	public List<string> ResolverNameList = new List<string>();

	public List<string> ResolverCodeList = new List<string>();

	public HashSet<string> ResolverDefineSet = new HashSet<string>();

	private Dictionary<Type, string> _classNameDic = new Dictionary<Type, string>();

	public void Write(StringBuilder sb)
	{
		sb.AppendLine("#if (KINGSRAID_CLIENT == false) || USE_AUTO_JSON_MARSHALER");
		sb.AppendLine("#else");
		sb.Append(CodeSample.FileHead);
		sb.Append(GenCode_GenCodeContainer());
		for (int i = 0; i < ComparerCodeList.Count; i++)
		{
			sb.Append(ComparerCodeList[i]);
		}
		for (int j = 0; j < ResolverCodeList.Count; j++)
		{
			sb.Append(ResolverCodeList[j]);
		}
		sb.Append(CodeSample.FileTail);
		sb.AppendLine("#endif");
	}

	private string GenCode_GenCodeContainer()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string comparerName in ComparerNameList)
		{
			stringBuilder.Append(CodeSample.NewCallCode.Replace("___instance___", comparerName));
		}
		foreach (string item in ComparerDefineSet)
		{
			stringBuilder.Append(CodeSample.NewCallCode.Replace("___instance___", item));
		}
		string newValue = stringBuilder.ToString();
		stringBuilder.Length = 0;
		foreach (string resolverName in ResolverNameList)
		{
			stringBuilder.Append(CodeSample.NewCallCode.Replace("___instance___", resolverName));
		}
		foreach (string item2 in ResolverDefineSet)
		{
			stringBuilder.Append(CodeSample.NewCallCode.Replace("___instance___", item2));
		}
		string newValue2 = stringBuilder.ToString();
		return CodeSample.GenCodeContainerBody.Replace("___resolver___", newValue2).Replace("___comparer___", newValue);
	}

	public void GenCode_String()
	{
		if (!ResolverFilterSet.Contains(typeof(string)))
		{
			ResolverFilterSet.Add(typeof(string));
			ResolverDefineSet.Add("global::NShared.NJit.NResolver.StringResolver");
		}
		if (!ComparerFilterSet.Contains(typeof(string)))
		{
			ComparerFilterSet.Add(typeof(string));
			ComparerDefineSet.Add("global::NShared.NJit.NResolver.StringComparer");
		}
	}

	public void GenCode_Primitive<T>()
	{
		Type typeFromHandle = typeof(T);
		GenCode_Primitive(typeFromHandle);
	}

	public void GenCode_Bool()
	{
		Type typeFromHandle = typeof(bool);
		if (!ResolverFilterSet.Contains(typeFromHandle))
		{
			string item = "global::NShared.NJit.NResolver.BooleanValueResolver";
			ResolverFilterSet.Add(typeFromHandle);
			ResolverNameList.Add(item);
		}
		if (!ComparerFilterSet.Contains(typeFromHandle))
		{
			string item2 = "global::NShared.NJit.NResolver.BooleanValueComparer";
			ComparerFilterSet.Add(typeFromHandle);
			ComparerNameList.Add(item2);
		}
	}

	public void GenCode_Primitive(Type type)
	{
		if (!ResolverFilterSet.Contains(type))
		{
			string item = CodeSample.PrimitiveResolverName.Replace("___Primitive___", type.Name);
			string item2 = CodeSample.PrimitiveResolverCode.Replace("___Primitive___", type.Name);
			ResolverFilterSet.Add(type);
			ResolverNameList.Add(item);
			ResolverCodeList.Add(item2);
		}
		if (!ComparerFilterSet.Contains(type))
		{
			string item3 = CodeSample.PrimitiveComparerName.Replace("___Primitive___", type.Name);
			string item4 = CodeSample.PrimitiveComparerCode.Replace("___Primitive___", type.Name);
			ComparerFilterSet.Add(type);
			ComparerNameList.Add(item3);
			ComparerCodeList.Add(item4);
		}
	}

	public void GenCode_Enum(Type enumType)
	{
		string name = Enum.GetUnderlyingType(enumType).Name;
		string newValue = enumType.FullName.Replace('.', '_').Replace('+', '_');
		string newValue2 = "global::" + enumType.FullName.Replace('+', '.');
		if (!ResolverFilterSet.Contains(enumType))
		{
			string item = CodeSample.EnumResolverName.Replace("___EnumName___", newValue);
			string item2 = CodeSample.EnumResolverCode.Replace("___EnumName___", newValue).Replace("___EnumGlobalType___", newValue2).Replace("___UnderlyingType___", name);
			ResolverFilterSet.Add(enumType);
			ResolverNameList.Add(item);
			ResolverCodeList.Add(item2);
		}
		if (!ComparerFilterSet.Contains(enumType))
		{
			string item3 = CodeSample.EnumComparerName.Replace("___EnumName___", newValue);
			string item4 = CodeSample.EnumComparerCode.Replace("___EnumName___", newValue).Replace("___EnumGlobalType___", newValue2).Replace("___UnderlyingType___", name);
			ComparerFilterSet.Add(enumType);
			ComparerNameList.Add(item3);
			ComparerCodeList.Add(item4);
		}
	}

	public void GenResolverCode_Generic(Type type, Type resolverGenericBase)
	{
		Type[] genericArguments = type.GetGenericArguments();
		if (genericArguments != null && genericArguments.Length != 0 && !ResolverFilterSet.Contains(type))
		{
			ResolverFilterSet.Add(type);
			Type type2 = resolverGenericBase.MakeGenericType(genericArguments);
			string classNameForCodeGen = GetClassNameForCodeGen(type2);
			ResolverDefineSet.Add(classNameForCodeGen);
		}
	}

	public void GenResolverCode_MessagePack(Type type)
	{
		Type type2 = ((!type.IsClass) ? typeof(MessagePackStructResolver<>) : typeof(MessagePackClassResolver<>));
		if (!ResolverFilterSet.Contains(type))
		{
			ResolverFilterSet.Add(type);
			Type type3 = type2.MakeGenericType(type);
			string classNameForCodeGen = GetClassNameForCodeGen(type3);
			ResolverDefineSet.Add(classNameForCodeGen);
		}
	}

	public bool GenComparerCode_JitKeyObject(Type type)
	{
		if (!UsingReflection.IsJitKeyObject(type))
		{
			return false;
		}
		if (ComparerFilterSet.Contains(type))
		{
			return true;
		}
		string newValue = type.FullName.Replace('.', '_').Replace('+', '_');
		string newValue2 = "global::" + type.FullName.Replace('+', '.');
		string item = CodeSample.JitKeyComparerName.Replace("___JitKeyName___", newValue);
		string item2 = CodeSample.JitKeyComparerCode.Replace("___JitKeyName___", newValue).Replace("___JitKeyGlobalType___", newValue2);
		ComparerFilterSet.Add(type);
		ComparerNameList.Add(item);
		ComparerCodeList.Add(item2);
		return true;
	}

	private string GetClassNameForCodeGen(Type type)
	{
		if (_classNameDic.TryGetValue(type, out var value))
		{
			return value;
		}
		value = type.ToString();
		while (true)
		{
			int num = value.IndexOf('`');
			if (num < 0)
			{
				break;
			}
			value = value.Remove(num, 2);
		}
		value = value.Replace('[', '<').Replace(']', '>').Replace('+', '.');
		_classNameDic.Add(type, value);
		return value;
	}
}
