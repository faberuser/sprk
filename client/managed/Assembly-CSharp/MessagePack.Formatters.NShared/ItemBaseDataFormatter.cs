using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemBaseDataFormatter : IMessagePackFormatter<ItemBaseData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemBaseData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(50);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemStory, options);
		writer.Write(value.IsArchive);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item3DPath, options);
		writer.Write(value.Item3DScale);
		writer.Write(value.Item3DFlip);
		resolver.GetFormatterWithVerify<ItemType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Value, options);
		writer.Write(value.Grade);
		writer.Write(value.Tutorial);
		writer.Write(value.BuyGold);
		writer.Write(value.BuyGem);
		writer.Write(value.BuyPvpCoin);
		writer.Write(value.BuyGuildPoint);
		writer.Write(value.BuyGuildArenaPoint);
		writer.Write(value.BuyRoyalPoint);
		writer.Write(value.BuyFriendshipPoint);
		writer.Write(value.BuyRaidPoint);
		writer.Write(value.BuyWorldBossPoint);
		writer.Write(value.BuyEventDungeonPoint);
		writer.Write(value.BuyEventDungeonPoint2);
		writer.Write(value.BuyEventDungeonPoint3);
		writer.Write(value.BuyEventGiftPoint);
		writer.Write(value.BuyGloryPoint);
		writer.Write(value.BuyRankingPoint);
		writer.Write(value.SellGold);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AssetGroup, options);
		writer.Write(value.NotForSale);
		writer.Write(value.Breakable);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Item3dModelNodeTree, options);
		writer.Write(value.OpenWithdrawal);
		writer.Write(value.GlobalNotify);
		writer.Write(value.Tier);
		writer.Write(value.UseLog);
		resolver.GetFormatterWithVerify<ItemData.GetEffectType>().Serialize(ref writer, value.GetEffect, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DropItemNodeTreeCode, options);
		writer.Write(value.BoxDivision);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceNameKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NameParam, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceDesc2Key, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Desc2Param, options);
		resolver.GetFormatterWithVerify<ClientActivityType>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
		writer.Write(value.AwakenStar);
		writer.Write(value.BuyOrdealArenaPoint);
		writer.Write(value.BuyEventOrvelPoint);
		writer.Write(value.BuyChallengeRaidPoint);
		writer.Write(value.ItemMaxCap);
	}

	public ItemBaseData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ItemBaseData itemBaseData = new ItemBaseData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemBaseData.Index = reader.ReadInt32();
				break;
			case 1:
				itemBaseData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				itemBaseData.ItemStory = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				itemBaseData.IsArchive = reader.ReadBoolean();
				break;
			case 5:
				itemBaseData.Item3DPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				itemBaseData.Item3DScale = reader.ReadSingle();
				break;
			case 7:
				itemBaseData.Item3DFlip = reader.ReadBoolean();
				break;
			case 8:
				itemBaseData.Type = resolver.GetFormatterWithVerify<ItemType>().Deserialize(ref reader, options);
				break;
			case 9:
				itemBaseData.Value = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				itemBaseData.Grade = reader.ReadByte();
				break;
			case 11:
				itemBaseData.Tutorial = reader.ReadBoolean();
				break;
			case 12:
				itemBaseData.BuyGold = reader.ReadInt32();
				break;
			case 13:
				itemBaseData.BuyGem = reader.ReadInt32();
				break;
			case 14:
				itemBaseData.BuyPvpCoin = reader.ReadInt32();
				break;
			case 15:
				itemBaseData.BuyGuildPoint = reader.ReadInt32();
				break;
			case 16:
				itemBaseData.BuyGuildArenaPoint = reader.ReadInt32();
				break;
			case 17:
				itemBaseData.BuyRoyalPoint = reader.ReadInt32();
				break;
			case 18:
				itemBaseData.BuyFriendshipPoint = reader.ReadInt32();
				break;
			case 19:
				itemBaseData.BuyRaidPoint = reader.ReadInt32();
				break;
			case 20:
				itemBaseData.BuyWorldBossPoint = reader.ReadInt32();
				break;
			case 21:
				itemBaseData.BuyEventDungeonPoint = reader.ReadInt32();
				break;
			case 22:
				itemBaseData.BuyEventDungeonPoint2 = reader.ReadInt32();
				break;
			case 23:
				itemBaseData.BuyEventDungeonPoint3 = reader.ReadInt32();
				break;
			case 24:
				itemBaseData.BuyEventGiftPoint = reader.ReadInt32();
				break;
			case 25:
				itemBaseData.BuyGloryPoint = reader.ReadInt32();
				break;
			case 26:
				itemBaseData.BuyRankingPoint = reader.ReadInt32();
				break;
			case 27:
				itemBaseData.SellGold = reader.ReadInt32();
				break;
			case 28:
				itemBaseData.AssetGroup = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 29:
				itemBaseData.NotForSale = reader.ReadBoolean();
				break;
			case 30:
				itemBaseData.Breakable = reader.ReadBoolean();
				break;
			case 31:
				itemBaseData.Item3dModelNodeTree = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 32:
				itemBaseData.OpenWithdrawal = reader.ReadByte();
				break;
			case 33:
				itemBaseData.GlobalNotify = reader.ReadBoolean();
				break;
			case 34:
				itemBaseData.Tier = reader.ReadInt16();
				break;
			case 35:
				itemBaseData.UseLog = reader.ReadBoolean();
				break;
			case 36:
				itemBaseData.GetEffect = resolver.GetFormatterWithVerify<ItemData.GetEffectType>().Deserialize(ref reader, options);
				break;
			case 37:
				itemBaseData.DropItemNodeTreeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 38:
				itemBaseData.BoxDivision = reader.ReadInt32();
				break;
			case 39:
				itemBaseData.ReplaceNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 40:
				itemBaseData.NameParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 41:
				itemBaseData.ReplaceDesc2Key = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 42:
				itemBaseData.Desc2Param = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 43:
				itemBaseData.MoveTaskType = resolver.GetFormatterWithVerify<ClientActivityType>().Deserialize(ref reader, options);
				break;
			case 44:
				itemBaseData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 45:
				itemBaseData.AwakenStar = reader.ReadInt32();
				break;
			case 46:
				itemBaseData.BuyOrdealArenaPoint = reader.ReadInt32();
				break;
			case 47:
				itemBaseData.BuyEventOrvelPoint = reader.ReadInt32();
				break;
			case 48:
				itemBaseData.BuyChallengeRaidPoint = reader.ReadInt32();
				break;
			case 49:
				itemBaseData.ItemMaxCap = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return itemBaseData;
	}
}
