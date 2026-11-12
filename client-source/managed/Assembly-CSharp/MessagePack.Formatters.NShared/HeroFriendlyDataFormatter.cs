using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroFriendlyDataFormatter : IMessagePackFormatter<HeroFriendlyData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroFriendlyData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(34);
		writer.Write(value.HeroIndex);
		writer.Write(value.NeedFriendlyPoint);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action1, options);
		writer.Write(value.GetMinPoint1);
		writer.Write(value.GetMaxPoint1);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.ActionCostType1, options);
		writer.Write(value.ActionCostValue1);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action2, options);
		writer.Write(value.GetMinPoint2);
		writer.Write(value.GetMaxPoint2);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.ActionCostType2, options);
		writer.Write(value.ActionCostValue2);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action3, options);
		writer.Write(value.GetMinPoint3);
		writer.Write(value.GetMaxPoint3);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.ActionCostType3, options);
		writer.Write(value.ActionCostValue3);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action4, options);
		writer.Write(value.GetMinPoint4);
		writer.Write(value.GetMaxPoint4);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.ActionCostType4, options);
		writer.Write(value.ActionCostValue4);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action5, options);
		writer.Write(value.GetMinPoint5);
		writer.Write(value.GetMaxPoint5);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.ActionCostType5, options);
		writer.Write(value.ActionCostValue5);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action6, options);
		writer.Write(value.GetMinPoint6);
		writer.Write(value.GetMaxPoint6);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.ActionCostType6, options);
		writer.Write(value.ActionCostValue6);
		writer.Write(value.HeroSelectChance);
		writer.Write(value.NPC_Check);
	}

	public HeroFriendlyData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroFriendlyData heroFriendlyData = new HeroFriendlyData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroFriendlyData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				heroFriendlyData.NeedFriendlyPoint = reader.ReadInt32();
				break;
			case 2:
				heroFriendlyData.Action1 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 3:
				heroFriendlyData.GetMinPoint1 = reader.ReadInt32();
				break;
			case 4:
				heroFriendlyData.GetMaxPoint1 = reader.ReadInt32();
				break;
			case 5:
				heroFriendlyData.ActionCostType1 = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 6:
				heroFriendlyData.ActionCostValue1 = reader.ReadInt32();
				break;
			case 7:
				heroFriendlyData.Action2 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 8:
				heroFriendlyData.GetMinPoint2 = reader.ReadInt32();
				break;
			case 9:
				heroFriendlyData.GetMaxPoint2 = reader.ReadInt32();
				break;
			case 10:
				heroFriendlyData.ActionCostType2 = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 11:
				heroFriendlyData.ActionCostValue2 = reader.ReadInt32();
				break;
			case 12:
				heroFriendlyData.Action3 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 13:
				heroFriendlyData.GetMinPoint3 = reader.ReadInt32();
				break;
			case 14:
				heroFriendlyData.GetMaxPoint3 = reader.ReadInt32();
				break;
			case 15:
				heroFriendlyData.ActionCostType3 = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 16:
				heroFriendlyData.ActionCostValue3 = reader.ReadInt32();
				break;
			case 17:
				heroFriendlyData.Action4 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 18:
				heroFriendlyData.GetMinPoint4 = reader.ReadInt32();
				break;
			case 19:
				heroFriendlyData.GetMaxPoint4 = reader.ReadInt32();
				break;
			case 20:
				heroFriendlyData.ActionCostType4 = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 21:
				heroFriendlyData.ActionCostValue4 = reader.ReadInt32();
				break;
			case 22:
				heroFriendlyData.Action5 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 23:
				heroFriendlyData.GetMinPoint5 = reader.ReadInt32();
				break;
			case 24:
				heroFriendlyData.GetMaxPoint5 = reader.ReadInt32();
				break;
			case 25:
				heroFriendlyData.ActionCostType5 = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 26:
				heroFriendlyData.ActionCostValue5 = reader.ReadInt32();
				break;
			case 27:
				heroFriendlyData.Action6 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 28:
				heroFriendlyData.GetMinPoint6 = reader.ReadInt32();
				break;
			case 29:
				heroFriendlyData.GetMaxPoint6 = reader.ReadInt32();
				break;
			case 30:
				heroFriendlyData.ActionCostType6 = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 31:
				heroFriendlyData.ActionCostValue6 = reader.ReadInt32();
				break;
			case 32:
				heroFriendlyData.HeroSelectChance = reader.ReadInt32();
				break;
			case 33:
				heroFriendlyData.NPC_Check = reader.ReadByte();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroFriendlyData;
	}
}
