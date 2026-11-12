using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NewEquipGachaData
{
	public static NewEquipGachaData Parse(string s)
	{
		return Parse(s, new NewEquipGachaData());
	}

	public static NewEquipGachaData Parse(string s, NewEquipGachaData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NewEquipGachaData Parse(IDictionary d)
	{
		return Parse(d, new NewEquipGachaData());
	}

	public static NewEquipGachaData Parse(IDictionary d, NewEquipGachaData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Type = BaseJsonMarshaler.ParseEnum<EquipGachaType>(d, "Type");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Desc = BaseJsonMarshaler.ParseValueString(d, "Desc");
		o.CardType = BaseJsonMarshaler.ParseValueInt(d, "CardType");
		o.Category = BaseJsonMarshaler.ParseValueString(d, "Category");
		o.Durational = BaseJsonMarshaler.ParseValueInt(d, "Durational");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.FreeCoolTime = BaseJsonMarshaler.ParseValueInt(d, "FreeCoolTime");
		o.Bid = BaseJsonMarshaler.ParseValueByte(d, "Bid");
		o.BonusBid = BaseJsonMarshaler.ParseValueByte(d, "BonusBid");
		o.OpenGold = BaseJsonMarshaler.ParseValueInt(d, "OpenGold");
		o.OpenGem = BaseJsonMarshaler.ParseValueInt(d, "OpenGem");
		o.Mileage = BaseJsonMarshaler.ParseValueInt(d, "Mileage");
		o.DailyMaxFree = BaseJsonMarshaler.ParseValueInt(d, "DailyMaxFree");
		o.ShowProbability = BaseJsonMarshaler.ParseValueBool(d, "ShowProbability");
		o.PickupHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "PickupHeroIndices", BaseJsonMarshaler.CastToInt);
		o.PickupHeroCount = BaseJsonMarshaler.ParseValueInt(d, "PickupHeroCount");
		o.GachaSubType = BaseJsonMarshaler.ParseEnum<EquipGachaSubType>(d, "GachaSubType");
		o.GachaCategorySelectTitle = BaseJsonMarshaler.ParseValueString(d, "GachaCategorySelectTitle");
		o.GachaSceneDesc = BaseJsonMarshaler.ParseValueString(d, "GachaSceneDesc");
		o.GachaShowIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaShowIndex");
		o.PickupGachaExpiredMessage = BaseJsonMarshaler.ParseValueString(d, "PickupGachaExpiredMessage");
		o.GachaCategoryGroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaCategoryGroupIndex");
		o.UniqueAwake = BaseJsonMarshaler.ParseValueBool(d, "UniqueAwake");
		o.DailyMaxSale = BaseJsonMarshaler.ParseValueInt(d, "DailyMaxSale");
		o.StepUpHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "StepUpHeroIndices", BaseJsonMarshaler.CastToInt);
		o.StepUpHeroCostumeIndices = BaseJsonMarshaler.ParseArrayCast(d, "StepUpHeroCostumeIndices", BaseJsonMarshaler.CastToInt);
		o.IconPath = BaseJsonMarshaler.ParseValueString(d, "IconPath");
		o.TicketOnly = BaseJsonMarshaler.ParseValueBool(d, "TicketOnly");
		o.StarterMileage = BaseJsonMarshaler.ParseValueInt(d, "StarterMileage");
		o.RewardIndices = BaseJsonMarshaler.ParseArrayCast(d, "RewardIndices", BaseJsonMarshaler.CastToInt);
		o.AppearNodeTree = BaseJsonMarshaler.ParseValueString(d, "AppearNodeTree");
		o.ReactionNodeTree = BaseJsonMarshaler.ParseValueString(d, "ReactionNodeTree");
		o.NodeTree = BaseJsonMarshaler.ParseArrayCast(d, "NodeTree", BaseJsonMarshaler.CastToString);
		o.OpenDate = BaseJsonMarshaler.ParseValueInt(d, "OpenDate");
		o.OpenCondition = BaseJsonMarshaler.ParseArrayCast(d, "OpenCondition", BaseJsonMarshaler.CastToString);
		o.MileageAmountForReward = BaseJsonMarshaler.ParseValueInt(d, "MileageAmountForReward");
		o.StepUpAccessoryIndex = BaseJsonMarshaler.ParseValueInt(d, "StepUpAccessoryIndex");
		o.StepUpAccessoryOffset = BaseJsonMarshaler.ParseArrayCast(d, "StepUpAccessoryOffset", BaseJsonMarshaler.CastToFloat);
		o.StepUpAccessoryRotation = BaseJsonMarshaler.ParseArrayCast(d, "StepUpAccessoryRotation", BaseJsonMarshaler.CastToFloat);
		o.StepUpAccessoryScale = BaseJsonMarshaler.ParseValueFloat(d, "StepUpAccessoryScale");
		o.AllowExpiredTicket = BaseJsonMarshaler.ParseValueBool(d, "AllowExpiredTicket");
		o.PurchaseRestrictProductIndex = BaseJsonMarshaler.ParseValueInt(d, "PurchaseRestrictProductIndex");
		return o;
	}

	public static string EncodeJsonString(NewEquipGachaData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NewEquipGachaData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Type", o.Type.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.Desc != null)
		{
			dictionary.Add("Desc", o.Desc);
		}
		dictionary.Add("CardType", o.CardType.ToString());
		if (o.Category != null)
		{
			dictionary.Add("Category", o.Category);
		}
		dictionary.Add("Durational", o.Durational.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("FreeCoolTime", o.FreeCoolTime.ToString());
		dictionary.Add("Bid", o.Bid.ToString());
		dictionary.Add("BonusBid", o.BonusBid.ToString());
		dictionary.Add("OpenGold", o.OpenGold.ToString());
		dictionary.Add("OpenGem", o.OpenGem.ToString());
		dictionary.Add("Mileage", o.Mileage.ToString());
		dictionary.Add("DailyMaxFree", o.DailyMaxFree.ToString());
		dictionary.Add("ShowProbability", o.ShowProbability.ToString());
		dictionary.Add("PickupHeroIndices", BaseJsonMarshaler.EncodeArray(o.PickupHeroIndices));
		dictionary.Add("PickupHeroCount", o.PickupHeroCount.ToString());
		dictionary.Add("GachaSubType", o.GachaSubType.ToString());
		if (o.GachaCategorySelectTitle != null)
		{
			dictionary.Add("GachaCategorySelectTitle", o.GachaCategorySelectTitle);
		}
		if (o.GachaSceneDesc != null)
		{
			dictionary.Add("GachaSceneDesc", o.GachaSceneDesc);
		}
		dictionary.Add("GachaShowIndex", o.GachaShowIndex.ToString());
		if (o.PickupGachaExpiredMessage != null)
		{
			dictionary.Add("PickupGachaExpiredMessage", o.PickupGachaExpiredMessage);
		}
		dictionary.Add("GachaCategoryGroupIndex", o.GachaCategoryGroupIndex.ToString());
		dictionary.Add("UniqueAwake", o.UniqueAwake.ToString());
		dictionary.Add("DailyMaxSale", o.DailyMaxSale.ToString());
		dictionary.Add("StepUpHeroIndices", BaseJsonMarshaler.EncodeArray(o.StepUpHeroIndices));
		dictionary.Add("StepUpHeroCostumeIndices", BaseJsonMarshaler.EncodeArray(o.StepUpHeroCostumeIndices));
		if (o.IconPath != null)
		{
			dictionary.Add("IconPath", o.IconPath);
		}
		dictionary.Add("TicketOnly", o.TicketOnly.ToString());
		dictionary.Add("StarterMileage", o.StarterMileage.ToString());
		dictionary.Add("RewardIndices", BaseJsonMarshaler.EncodeArray(o.RewardIndices));
		if (o.AppearNodeTree != null)
		{
			dictionary.Add("AppearNodeTree", o.AppearNodeTree);
		}
		if (o.ReactionNodeTree != null)
		{
			dictionary.Add("ReactionNodeTree", o.ReactionNodeTree);
		}
		dictionary.Add("NodeTree", BaseJsonMarshaler.EncodeArray(o.NodeTree));
		dictionary.Add("OpenDate", o.OpenDate.ToString());
		dictionary.Add("OpenCondition", BaseJsonMarshaler.EncodeArray(o.OpenCondition));
		dictionary.Add("MileageAmountForReward", o.MileageAmountForReward.ToString());
		dictionary.Add("StepUpAccessoryIndex", o.StepUpAccessoryIndex.ToString());
		dictionary.Add("StepUpAccessoryOffset", BaseJsonMarshaler.EncodeArray(o.StepUpAccessoryOffset));
		dictionary.Add("StepUpAccessoryRotation", BaseJsonMarshaler.EncodeArray(o.StepUpAccessoryRotation));
		dictionary.Add("StepUpAccessoryScale", o.StepUpAccessoryScale.ToString());
		dictionary.Add("AllowExpiredTicket", o.AllowExpiredTicket.ToString());
		dictionary.Add("PurchaseRestrictProductIndex", o.PurchaseRestrictProductIndex.ToString());
		return dictionary;
	}
}
