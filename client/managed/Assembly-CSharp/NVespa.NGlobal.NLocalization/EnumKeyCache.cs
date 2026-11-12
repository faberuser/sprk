using System;
using System.Collections.Generic;
using System.Text;

namespace NVespa.NGlobal.NLocalization;

public class EnumKeyCache : SingletonAuto<EnumKeyCache>
{
	private Dictionary<Enum, string> _enumDic = new Dictionary<Enum, string>();

	protected override bool ThreadSafe => true;

	public string GetFormat(Enum enumValue)
	{
		lock (_enumDic)
		{
			if (_enumDic.TryGetValue(enumValue, out var value))
			{
				return value;
			}
			value = ConvertEnumKeyFormat(enumValue.GetType().Name, enumValue.ToString());
			_enumDic.Add(enumValue, value);
			return value;
		}
	}

	public static string ConvertEnumKeyFormat(Enum enumValue)
	{
		return ConvertEnumKeyFormat(enumValue.GetType().Name, enumValue.ToString());
	}

	public static string ConvertEnumKeyFormat(string enumTypeName, string valueName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < enumTypeName.Length; i++)
		{
			if (i > 0 && char.IsUpper(enumTypeName[i]) && char.IsLower(enumTypeName[i - 1]))
			{
				stringBuilder.Append('_');
			}
			stringBuilder.Append(char.ToUpper(enumTypeName[i]));
		}
		stringBuilder.Append('.');
		for (int j = 0; j < valueName.Length; j++)
		{
			if (j > 0 && char.IsUpper(valueName[j]) && char.IsLower(valueName[j - 1]))
			{
				stringBuilder.Append('_');
			}
			stringBuilder.Append(char.ToUpper(valueName[j]));
		}
		return stringBuilder.ToString();
	}
}
