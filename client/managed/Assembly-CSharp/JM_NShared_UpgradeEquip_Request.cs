using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UpgradeEquip;

public static class JM_NShared_UpgradeEquip_Request
{
	public static NShared.UpgradeEquip.Request Parse(string s)
	{
		return Parse(s, new NShared.UpgradeEquip.Request());
	}

	public static NShared.UpgradeEquip.Request Parse(string s, NShared.UpgradeEquip.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UpgradeEquip.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UpgradeEquip.Request());
	}

	public static NShared.UpgradeEquip.Request Parse(IDictionary d, NShared.UpgradeEquip.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.MaterialSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "MaterialSlotIndices", BaseJsonMarshaler.CastToInt);
		o.UpgradeLevel = BaseJsonMarshaler.ParseValueInt(d, "UpgradeLevel");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UpgradeEquip.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UpgradeEquip.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("MaterialSlotIndices", BaseJsonMarshaler.EncodeArray(o.MaterialSlotIndices));
		dictionary.Add("UpgradeLevel", o.UpgradeLevel.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
