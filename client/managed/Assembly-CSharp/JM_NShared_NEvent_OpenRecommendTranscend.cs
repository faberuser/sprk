using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenRecommendTranscend
{
	public static OpenRecommendTranscend Parse(string s)
	{
		return Parse(s, new OpenRecommendTranscend());
	}

	public static OpenRecommendTranscend Parse(string s, OpenRecommendTranscend o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenRecommendTranscend Parse(IDictionary d)
	{
		return Parse(d, new OpenRecommendTranscend());
	}

	public static OpenRecommendTranscend Parse(IDictionary d, OpenRecommendTranscend o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenRecommendTranscend o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenRecommendTranscend o)
	{
		return new Dictionary<string, object>();
	}
}
