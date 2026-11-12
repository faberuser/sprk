using System;
using System.Globalization;
using System.Text;
using NClientData;
using NShared;
using NVespa.NGlobal.NLocalization;

namespace NVespa.NGlobal;

internal class LocalGetter
{
	private const string token_of_remap_begin = "{@";

	private const char token_of_remap_end = '}';

	private static readonly string[] FormatSequence = new string[20]
	{
		"{1}", "{2}", "{3}", "{4}", "{5}", "{6}", "{7}", "{8}", "{9}", "{10}",
		"{11}", "{12}", "{13}", "{14}", "{15}", "{16}", "{17}", "{18}", "{19}", "{20}"
	};

	public static string Remap(string orig_value)
	{
		if (String_Extension_Method.IsNullOrEmpty(orig_value))
		{
			return orig_value;
		}
		string text = orig_value;
		for (int i = 0; i < 10; i++)
		{
			int num = text.IndexOf("{@");
			if (num < 0)
			{
				return text;
			}
			int num2 = text.IndexOf('}', num + "{@".Length);
			if (num2 < 0)
			{
				return text;
			}
			int num3 = num + "{@".Length;
			int num4 = num2;
			string text2 = text.Substring(num3, num4 - num3);
			string direct = GetDirect(text2);
			string oldValue = string.Format("{0}{1}{2}", "{@", text2, '}');
			text = text.Replace(oldValue, direct);
		}
		return text;
	}

	public static string GetDirect(string key)
	{
		if (!SingletonAuto<LocalizationLoader>.instance.TryGetString(SingletonAuto<LocalizationLoader>.instance.ValidateKey(key), out var value))
		{
			return key;
		}
		if (string.IsNullOrEmpty(value))
		{
			return key;
		}
		return value;
	}

	public static bool TryGetDirect(string key, out string value)
	{
		bool num = SingletonAuto<LocalizationLoader>.instance.TryGetString(SingletonAuto<LocalizationLoader>.instance.ValidateKey(key), out value);
		if (!num || string.IsNullOrEmpty(value))
		{
			value = key;
		}
		return num;
	}

	public static string GetWithArgs(string key, object[] args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!TryGetDirect(key, out var value) || String_Extension_Method.IsNullOrEmpty(value))
		{
			stringBuilder.Append(key);
			int num = 0;
			while (args != null && num < __Extension_Method_ICollection__.COUNT(args))
			{
				stringBuilder.Append(',');
				stringBuilder.Append(Vespa_Common_Class_Extension_Method.TOSTRING(args[num]));
				num++;
			}
			return stringBuilder.ToString();
		}
		stringBuilder.Append(value);
		CultureInfo cultureInfo = GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
		int num2 = 0;
		while (args != null && num2 < __Extension_Method_ICollection__.COUNT(args))
		{
			stringBuilder.Replace(FormatSequence[num2], Convert.ToString(args[num2], cultureInfo) ?? "null");
			num2++;
		}
		return stringBuilder.ToString();
	}

	public static string GetEnumWithArgs(Enum key, params object[] args)
	{
		return GetWithArgs(SingletonAuto<EnumKeyCache>.instance.GetFormat(key), args);
	}
}
