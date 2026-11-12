using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GameGuideData
{
	public static GameGuideData Parse(string s)
	{
		return Parse(s, new GameGuideData());
	}

	public static GameGuideData Parse(string s, GameGuideData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GameGuideData Parse(IDictionary d)
	{
		return Parse(d, new GameGuideData());
	}

	public static GameGuideData Parse(IDictionary d, GameGuideData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.GuideType = BaseJsonMarshaler.ParseValueInt(d, "GuideType");
		o.GuideImage = BaseJsonMarshaler.ParseValueString(d, "GuideImage");
		o.MoveTaskType = BaseJsonMarshaler.ParseValueString(d, "MoveTaskType");
		o.MoveTaskValue = BaseJsonMarshaler.ParseArrayCast(d, "MoveTaskValue", BaseJsonMarshaler.CastToString);
		o.SummaryKEY = BaseJsonMarshaler.ParseValueString(d, "SummaryKEY");
		o.Question1 = BaseJsonMarshaler.ParseValueString(d, "Question1");
		o.Answer1 = BaseJsonMarshaler.ParseValueString(d, "Answer1");
		o.Question2 = BaseJsonMarshaler.ParseValueString(d, "Question2");
		o.Answer2 = BaseJsonMarshaler.ParseValueString(d, "Answer2");
		o.Question3 = BaseJsonMarshaler.ParseValueString(d, "Question3");
		o.Answer3 = BaseJsonMarshaler.ParseValueString(d, "Answer3");
		o.Question4 = BaseJsonMarshaler.ParseValueString(d, "Question4");
		o.Answer4 = BaseJsonMarshaler.ParseValueString(d, "Answer4");
		o.Question5 = BaseJsonMarshaler.ParseValueString(d, "Question5");
		o.Answer5 = BaseJsonMarshaler.ParseValueString(d, "Answer5");
		o.Question6 = BaseJsonMarshaler.ParseValueString(d, "Question6");
		o.Answer6 = BaseJsonMarshaler.ParseValueString(d, "Answer6");
		o.Question7 = BaseJsonMarshaler.ParseValueString(d, "Question7");
		o.Answer7 = BaseJsonMarshaler.ParseValueString(d, "Answer7");
		o.Question8 = BaseJsonMarshaler.ParseValueString(d, "Question8");
		o.Answer8 = BaseJsonMarshaler.ParseValueString(d, "Answer8");
		return o;
	}

	public static string EncodeJsonString(GameGuideData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GameGuideData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("GuideType", o.GuideType.ToString());
		if (o.GuideImage != null)
		{
			dictionary.Add("GuideImage", o.GuideImage);
		}
		if (o.MoveTaskType != null)
		{
			dictionary.Add("MoveTaskType", o.MoveTaskType);
		}
		dictionary.Add("MoveTaskValue", BaseJsonMarshaler.EncodeArray(o.MoveTaskValue));
		if (o.SummaryKEY != null)
		{
			dictionary.Add("SummaryKEY", o.SummaryKEY);
		}
		if (o.Question1 != null)
		{
			dictionary.Add("Question1", o.Question1);
		}
		if (o.Answer1 != null)
		{
			dictionary.Add("Answer1", o.Answer1);
		}
		if (o.Question2 != null)
		{
			dictionary.Add("Question2", o.Question2);
		}
		if (o.Answer2 != null)
		{
			dictionary.Add("Answer2", o.Answer2);
		}
		if (o.Question3 != null)
		{
			dictionary.Add("Question3", o.Question3);
		}
		if (o.Answer3 != null)
		{
			dictionary.Add("Answer3", o.Answer3);
		}
		if (o.Question4 != null)
		{
			dictionary.Add("Question4", o.Question4);
		}
		if (o.Answer4 != null)
		{
			dictionary.Add("Answer4", o.Answer4);
		}
		if (o.Question5 != null)
		{
			dictionary.Add("Question5", o.Question5);
		}
		if (o.Answer5 != null)
		{
			dictionary.Add("Answer5", o.Answer5);
		}
		if (o.Question6 != null)
		{
			dictionary.Add("Question6", o.Question6);
		}
		if (o.Answer6 != null)
		{
			dictionary.Add("Answer6", o.Answer6);
		}
		if (o.Question7 != null)
		{
			dictionary.Add("Question7", o.Question7);
		}
		if (o.Answer7 != null)
		{
			dictionary.Add("Answer7", o.Answer7);
		}
		if (o.Question8 != null)
		{
			dictionary.Add("Question8", o.Question8);
		}
		if (o.Answer8 != null)
		{
			dictionary.Add("Answer8", o.Answer8);
		}
		return dictionary;
	}
}
