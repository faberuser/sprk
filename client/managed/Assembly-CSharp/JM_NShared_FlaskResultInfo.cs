using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FlaskResultInfo
{
	public static FlaskResultInfo Parse(string s)
	{
		return Parse(s, new FlaskResultInfo());
	}

	public static FlaskResultInfo Parse(string s, FlaskResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FlaskResultInfo Parse(IDictionary d)
	{
		return Parse(d, new FlaskResultInfo());
	}

	public static FlaskResultInfo Parse(IDictionary d, FlaskResultInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.AddExp = BaseJsonMarshaler.ParseValueInt(d, "AddExp");
		o.AddExpRatio = BaseJsonMarshaler.ParseValueInt(d, "AddExpRatio");
		o.NewExp = BaseJsonMarshaler.ParseValueInt(d, "NewExp");
		return o;
	}

	public static string EncodeJsonString(FlaskResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FlaskResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("AddExp", o.AddExp.ToString());
		dictionary.Add("AddExpRatio", o.AddExpRatio.ToString());
		dictionary.Add("NewExp", o.NewExp.ToString());
		return dictionary;
	}
}
