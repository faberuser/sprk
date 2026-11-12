using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GameMiniGuideData
{
	public static GameMiniGuideData Parse(string s)
	{
		return Parse(s, new GameMiniGuideData());
	}

	public static GameMiniGuideData Parse(string s, GameMiniGuideData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GameMiniGuideData Parse(IDictionary d)
	{
		return Parse(d, new GameMiniGuideData());
	}

	public static GameMiniGuideData Parse(IDictionary d, GameMiniGuideData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.GameGuideIndex = BaseJsonMarshaler.ParseArrayCast(d, "GameGuideIndex", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(GameMiniGuideData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GameMiniGuideData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("GameGuideIndex", BaseJsonMarshaler.EncodeArray(o.GameGuideIndex));
		return dictionary;
	}
}
