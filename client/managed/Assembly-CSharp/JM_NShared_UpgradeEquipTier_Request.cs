using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UpgradeEquipTier;

public static class JM_NShared_UpgradeEquipTier_Request
{
	public static NShared.UpgradeEquipTier.Request Parse(string s)
	{
		return Parse(s, new NShared.UpgradeEquipTier.Request());
	}

	public static NShared.UpgradeEquipTier.Request Parse(string s, NShared.UpgradeEquipTier.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UpgradeEquipTier.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UpgradeEquipTier.Request());
	}

	public static NShared.UpgradeEquipTier.Request Parse(IDictionary d, NShared.UpgradeEquipTier.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UpgradeEquipTier.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UpgradeEquipTier.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
