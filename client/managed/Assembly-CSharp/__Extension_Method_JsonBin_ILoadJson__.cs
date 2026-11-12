using JsonBin;

public static class __Extension_Method_JsonBin_ILoadJson__
{
	public static bool LOADFROMJSON<TJsonLoad>(this TJsonLoad This, JsonRead json) where TJsonLoad : ILoadJson
	{
		if (json.IsNone() || This == null)
		{
			return false;
		}
		return This.Load(json);
	}
}
