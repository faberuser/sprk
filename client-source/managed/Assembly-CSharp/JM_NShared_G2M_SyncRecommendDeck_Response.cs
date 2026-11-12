using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2M_SyncRecommendDeck;

public static class JM_NShared_G2M_SyncRecommendDeck_Response
{
	public static NShared.G2M_SyncRecommendDeck.Response Parse(string s)
	{
		return Parse(s, new NShared.G2M_SyncRecommendDeck.Response());
	}

	public static NShared.G2M_SyncRecommendDeck.Response Parse(string s, NShared.G2M_SyncRecommendDeck.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2M_SyncRecommendDeck.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2M_SyncRecommendDeck.Response());
	}

	public static NShared.G2M_SyncRecommendDeck.Response Parse(IDictionary d, NShared.G2M_SyncRecommendDeck.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
