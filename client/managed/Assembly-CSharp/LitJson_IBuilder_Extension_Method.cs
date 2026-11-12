using LitJson;

public static class LitJson_IBuilder_Extension_Method
{
	public static bool LOADFROMJSON(this ILoadJson This, JsonData json)
	{
		if (json.isNone() || This == null)
		{
			return false;
		}
		return This.Load(json);
	}

	public static JsonData MAKEJSON(this IMakeJson This)
	{
		return This?.MakeJson();
	}
}
