using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FlaskEndCampaignResultInfo
{
	public static FlaskEndCampaignResultInfo Parse(string s)
	{
		return Parse(s, new FlaskEndCampaignResultInfo());
	}

	public static FlaskEndCampaignResultInfo Parse(string s, FlaskEndCampaignResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FlaskEndCampaignResultInfo Parse(IDictionary d)
	{
		return Parse(d, new FlaskEndCampaignResultInfo());
	}

	public static FlaskEndCampaignResultInfo Parse(IDictionary d, FlaskEndCampaignResultInfo o)
	{
		o.FilledFlaskIndex = BaseJsonMarshaler.ParseValueInt(d, "FilledFlaskIndex");
		o.FilledFlaskCount = BaseJsonMarshaler.ParseValueInt(d, "FilledFlaskCount");
		o.Info = BaseJsonMarshaler.ParseObject(d, "Info", JM_NShared_FlaskResultInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(FlaskEndCampaignResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FlaskEndCampaignResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("FilledFlaskIndex", o.FilledFlaskIndex.ToString());
		dictionary.Add("FilledFlaskCount", o.FilledFlaskCount.ToString());
		dictionary.Add("Info", JM_NShared_FlaskResultInfo.EncodeJsonObject(o.Info));
		return dictionary;
	}
}
