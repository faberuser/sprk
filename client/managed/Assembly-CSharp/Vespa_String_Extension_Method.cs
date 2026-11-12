using System;
using System.Collections.Generic;
using System.Text;
using NVespa;
using UnityEngine;

public static class Vespa_String_Extension_Method
{
	public static char[] TrimChars = new char[4] { '\n', '\r', ' ', '\t' };

	public static string Join(this string[] This, string separator)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(This))
		{
			return string.Empty;
		}
		return string.Join(separator, This);
	}

	public static string ToStringJoin<T>(this IList<T> This, string separator)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(This))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(This[0].ToString());
		for (int i = 1; i < This.Count; i++)
		{
			stringBuilder.Append(", ");
			stringBuilder.Append(This[i].ToString());
		}
		return stringBuilder.ToString();
	}

	public static string GetEnumString<T>(this T This) where T : struct
	{
		EnumLookup<T>.ValueData byEnum = EnumLookup<T>.GetByEnum(This, null);
		if (byEnum != null)
		{
			return byEnum.Name;
		}
		return string.Empty;
	}

	public static string ReplaceFirst(this string This, char[] anyOf, char to)
	{
		int num = This.IndexOfAny(anyOf);
		if (num < 0)
		{
			return This;
		}
		return $"{This.Substring(0, num)}{to}{This.Substring(num + 1)}";
	}

	public static bool TryToEnum<EnumType>(this string This, out EnumType o_output)
	{
		return EnumTools.TryParse<EnumType>(This, out o_output);
	}

	public static EnumType TOENUM<EnumType>(this string This, EnumType iDefault)
	{
		if (!EnumTools.TryParse<EnumType>(This, out var o_value))
		{
			return iDefault;
		}
		return o_value;
	}

	public static bool TOBOOL(this string This, bool i_default)
	{
		if (!bool.TryParse(This, out var result))
		{
			return i_default;
		}
		return result;
	}

	public static int TOINT(this string This, int i_default)
	{
		if (!int.TryParse(This, out var result))
		{
			return i_default;
		}
		return result;
	}

	public static int? TONULLABLEINT(this string This)
	{
		if (!int.TryParse(This, out var result))
		{
			return null;
		}
		return result;
	}

	public static long TOLONG(this string This, long defaultValue)
	{
		if (!long.TryParse(This, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public static ulong TOULONG(this string This, ulong defaultValue)
	{
		if (!ulong.TryParse(This, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public static decimal TODECIMAL(this string This, decimal defaultValue)
	{
		if (!decimal.TryParse(This, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public static float TOFLOAT(this string This, float i_default)
	{
		if (!float.TryParse(This, out var result))
		{
			return i_default;
		}
		return result;
	}

	public static float PERCENTTOFLOAT(this string This, float i_default)
	{
		if (!float.TryParse(This, out var result))
		{
			return i_default;
		}
		return result * 0.01f;
	}

	public static byte[] ToByteArray(this string This)
	{
		return Encoding.UTF8.GetBytes(This);
	}

	public static string FixFolderSeparator(this string This)
	{
		return This.Replace('\\', '/');
	}

	public static int ComparePath(this string This, string i_rhs)
	{
		string strA = FixFolderSeparator(This);
		string strB = FixFolderSeparator(i_rhs);
		return string.CompareOrdinal(strA, strB);
	}

	public static string CombineUrl(this string This, string i_rhs)
	{
		bool flag = This.EndsWith("/");
		bool flag2 = i_rhs.StartsWith("/");
		if (flag != flag2)
		{
			return This + i_rhs;
		}
		if (!flag)
		{
			return This + "/" + i_rhs;
		}
		return This + "/" + i_rhs.Substring(1);
	}

	public static string Ellipsis(this string This, int i_length)
	{
		if (This.Length > i_length)
		{
			return This.Substring(0, i_length) + "...";
		}
		return This;
	}

	public static string NewlineDelete(this string This)
	{
		if (String_Extension_Method.IsNullOrEmpty(This))
		{
			return This;
		}
		return This.Replace("\r", "").Replace("\n", "");
	}

	public static string ColorTag(this string This, Color i_color)
	{
		return Vespa_Gui_RichTextBuilder_Extension_Method.RichText(This).Color(i_color).m_text;
	}

	public static string SizeTag(this string This, int size)
	{
		return Vespa_Gui_RichTextBuilder_Extension_Method.RichText(This).Size(size).m_text;
	}

	public static string NguiColor(this string This, Color color, bool isAbsolute = false)
	{
		string arg = NGUIText.EncodeColor(color);
		if (isAbsolute)
		{
			return $"[c][{arg}]{This}[-][/c]";
		}
		return $"[{arg}]{This}[-]";
	}

	public static string AppendRandomNumber(this string This, string splitter = "", int min = int.MinValue, int max = int.MaxValue)
	{
		UnityEngine.Random.seed = (int)DateTime.UtcNow.Ticks;
		return This + splitter + UnityEngine.Random.Range(min, max);
	}

	public static string TRIM(this string This)
	{
		if (String_Extension_Method.IsNullOrEmpty(This))
		{
			return This;
		}
		return This.Trim(TrimChars);
	}

	public static string ToSlash(this string This)
	{
		if (String_Extension_Method.IsNullOrEmpty(This))
		{
			return This;
		}
		return This.Replace("\\", "/");
	}

	public static string ArrayToString(this string[] This)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string value in This)
		{
			stringBuilder.Append(value);
			stringBuilder.Append('.');
		}
		return stringBuilder.ToString();
	}
}
