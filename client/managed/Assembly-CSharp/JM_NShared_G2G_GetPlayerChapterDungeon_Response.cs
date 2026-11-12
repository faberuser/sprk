using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2G_GetPlayerChapterDungeon;

public static class JM_NShared_G2G_GetPlayerChapterDungeon_Response
{
	public static NShared.G2G_GetPlayerChapterDungeon.Response Parse(string s)
	{
		return Parse(s, new NShared.G2G_GetPlayerChapterDungeon.Response());
	}

	public static NShared.G2G_GetPlayerChapterDungeon.Response Parse(string s, NShared.G2G_GetPlayerChapterDungeon.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2G_GetPlayerChapterDungeon.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2G_GetPlayerChapterDungeon.Response());
	}

	public static NShared.G2G_GetPlayerChapterDungeon.Response Parse(IDictionary d, NShared.G2G_GetPlayerChapterDungeon.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
