using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DropBonusEvent
{
	public static DropBonusEvent Parse(string s)
	{
		return Parse(s, new DropBonusEvent());
	}

	public static DropBonusEvent Parse(string s, DropBonusEvent o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DropBonusEvent Parse(IDictionary d)
	{
		return Parse(d, new DropBonusEvent());
	}

	public static DropBonusEvent Parse(IDictionary d, DropBonusEvent o)
	{
		o.Id = BaseJsonMarshaler.ParseValueInt(d, "Id");
		o.DayOfWeek = BaseJsonMarshaler.ParseValueString(d, "DayOfWeek");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.BeginHour = BaseJsonMarshaler.ParseValueInt(d, "BeginHour");
		o.BeginMinute = BaseJsonMarshaler.ParseValueInt(d, "BeginMinute");
		o.EndHour = BaseJsonMarshaler.ParseValueInt(d, "EndHour");
		o.EndMinute = BaseJsonMarshaler.ParseValueInt(d, "EndMinute");
		o.DungeonType = BaseJsonMarshaler.ParseArray<BattleType>(d, "DungeonType");
		o.BonusType = BaseJsonMarshaler.ParseEnum<EventBonusType>(d, "BonusType");
		o.ItemCode = BaseJsonMarshaler.ParseValueString(d, "ItemCode");
		o.ItemIndices = BaseJsonMarshaler.ParseArrayCast(d, "ItemIndices", BaseJsonMarshaler.CastToInt);
		o.BonusPercentage = BaseJsonMarshaler.ParseValueInt(d, "BonusPercentage");
		o.Condition = BaseJsonMarshaler.ParseValueString(d, "Condition");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Message = BaseJsonMarshaler.ParseArrayCast(d, "Message", BaseJsonMarshaler.CastToString);
		o.LocalInfo = BaseJsonMarshaler.ParseArray(d, "LocalInfo", JM_NShared_DropBonusEventLocalInfo.Parse);
		o.MoveTaskType = BaseJsonMarshaler.ParseEnum<ClientActivityType>(d, "MoveTaskType");
		o.MoveTaskValue = BaseJsonMarshaler.ParseValueString(d, "MoveTaskValue");
		o.MinLevel = BaseJsonMarshaler.ParseValueInt(d, "MinLevel");
		o.BuffPosition = BaseJsonMarshaler.ParseEnum<DropBonusEvnetBuffPosition>(d, "BuffPosition");
		return o;
	}

	public static string EncodeJsonString(DropBonusEvent o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DropBonusEvent o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		if (o.DayOfWeek != null)
		{
			dictionary.Add("DayOfWeek", o.DayOfWeek);
		}
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("BeginHour", o.BeginHour.ToString());
		dictionary.Add("BeginMinute", o.BeginMinute.ToString());
		dictionary.Add("EndHour", o.EndHour.ToString());
		dictionary.Add("EndMinute", o.EndMinute.ToString());
		dictionary.Add("DungeonType", BaseJsonMarshaler.EncodeArray(o.DungeonType));
		dictionary.Add("BonusType", o.BonusType.ToString());
		if (o.ItemCode != null)
		{
			dictionary.Add("ItemCode", o.ItemCode);
		}
		dictionary.Add("ItemIndices", BaseJsonMarshaler.EncodeArray(o.ItemIndices));
		dictionary.Add("BonusPercentage", o.BonusPercentage.ToString());
		if (o.Condition != null)
		{
			dictionary.Add("Condition", o.Condition);
		}
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		dictionary.Add("Message", BaseJsonMarshaler.EncodeArray(o.Message));
		dictionary.Add("LocalInfo", BaseJsonMarshaler.EncodeArray(o.LocalInfo, JM_NShared_DropBonusEventLocalInfo.EncodeJsonObject));
		dictionary.Add("MoveTaskType", o.MoveTaskType.ToString());
		if (o.MoveTaskValue != null)
		{
			dictionary.Add("MoveTaskValue", o.MoveTaskValue);
		}
		dictionary.Add("MinLevel", o.MinLevel.ToString());
		dictionary.Add("BuffPosition", o.BuffPosition.ToString());
		return dictionary;
	}
}
