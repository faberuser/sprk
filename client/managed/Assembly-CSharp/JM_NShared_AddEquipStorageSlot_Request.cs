using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AddEquipStorageSlot;

public static class JM_NShared_AddEquipStorageSlot_Request
{
	public static NShared.AddEquipStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.AddEquipStorageSlot.Request());
	}

	public static NShared.AddEquipStorageSlot.Request Parse(string s, NShared.AddEquipStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AddEquipStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AddEquipStorageSlot.Request());
	}

	public static NShared.AddEquipStorageSlot.Request Parse(IDictionary d, NShared.AddEquipStorageSlot.Request o)
	{
		o.OldEquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "OldEquipStorageSlotIndex");
		o.EquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.HeroPartIndex = BaseJsonMarshaler.ParseArrayCast(d, "HeroPartIndex", BaseJsonMarshaler.CastToInt);
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndex", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AddEquipStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AddEquipStorageSlot.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OldEquipStorageSlotIndex", o.OldEquipStorageSlotIndex.ToString());
		dictionary.Add("EquipStorageSlotIndex", o.EquipStorageSlotIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("HeroPartIndex", BaseJsonMarshaler.EncodeArray(o.HeroPartIndex));
		dictionary.Add("EquipItemSlotIndex", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndex));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
