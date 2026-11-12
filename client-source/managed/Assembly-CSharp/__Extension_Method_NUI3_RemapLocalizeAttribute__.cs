using NGame2.NUI3;
using NShared;
using NVespa.NGlobal;

public static class __Extension_Method_NUI3_RemapLocalizeAttribute__
{
	public static string GetRemapLocalizeKey<T>(this T This) where T : struct
	{
		if (!typeof(T).IsEnum)
		{
			return This.ToString();
		}
		RemapLocalizeAttribute remapLocalizeAttribute = EnumAttributeContainer<T, RemapLocalizeAttribute>.Get(This);
		if (remapLocalizeAttribute == null)
		{
			return This.ToString();
		}
		return remapLocalizeAttribute.Key;
	}

	public static string GetRemapLocalizeString<T>(this T This) where T : struct
	{
		return LocalizationManager.GetStringDirect(GetRemapLocalizeKey(This));
	}
}
