using System.Collections;
using MiniJSON;
using NShared;
using NShared.M2G_ReloadPlayerBattleBanPick;

public static class JM_NShared_M2G_ReloadPlayerBattleBanPick_Response
{
	public static NShared.M2G_ReloadPlayerBattleBanPick.Response Parse(string s)
	{
		return Parse(s, new NShared.M2G_ReloadPlayerBattleBanPick.Response());
	}

	public static NShared.M2G_ReloadPlayerBattleBanPick.Response Parse(string s, NShared.M2G_ReloadPlayerBattleBanPick.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.M2G_ReloadPlayerBattleBanPick.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.M2G_ReloadPlayerBattleBanPick.Response());
	}

	public static NShared.M2G_ReloadPlayerBattleBanPick.Response Parse(IDictionary d, NShared.M2G_ReloadPlayerBattleBanPick.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
