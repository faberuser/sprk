using System.Collections.Generic;

namespace NVespa;

public class SingletonSave<TypeCls, Delimiter> where TypeCls : class, new()
{
	public static TypeCls value;

	private static Dictionary<string, TypeCls> _dic;

	public static TypeCls autoValue
	{
		get
		{
			if (value == null)
			{
				value = new TypeCls();
			}
			return value;
		}
	}

	public static TypeCls Dic(string i_keyword)
	{
		if (_dic == null)
		{
			_dic = new Dictionary<string, TypeCls>();
		}
		if (!_dic.TryGetValue(i_keyword, out var result))
		{
			result = new TypeCls();
			_dic.Add(i_keyword, result);
		}
		return result;
	}
}
