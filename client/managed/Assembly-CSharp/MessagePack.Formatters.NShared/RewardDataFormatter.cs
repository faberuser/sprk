using System.Collections.Generic;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RewardDataFormatter : IMessagePackFormatter<RewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(16);
		writer.Write(value.Index);
		writer.Write(value.GoldRate);
		writer.Write(value.GoldMin);
		writer.Write(value.GoldMax);
		writer.Write(value.GemRate);
		writer.Write(value.GemMin);
		writer.Write(value.GemMax);
		writer.Write(value.LuaPointRate);
		writer.Write(value.LuaPointMin);
		writer.Write(value.LuaPointMax);
		writer.Write(value.ShopEventPointRate);
		writer.Write(value.ShopEventPointMin);
		writer.Write(value.ShopEventPointMax);
		writer.Write(value.LimitedShopEventPointRate);
		writer.Write(value.LimitedShopEventPointValue);
		resolver.GetFormatterWithVerify<List<RewardData.SubData>>().Serialize(ref writer, value.SubDataList, options);
	}

	public RewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RewardData rewardData = new RewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				rewardData.Index = reader.ReadInt32();
				break;
			case 1:
				rewardData.GoldRate = reader.ReadInt32();
				break;
			case 2:
				rewardData.GoldMin = reader.ReadInt32();
				break;
			case 3:
				rewardData.GoldMax = reader.ReadInt32();
				break;
			case 4:
				rewardData.GemRate = reader.ReadInt32();
				break;
			case 5:
				rewardData.GemMin = reader.ReadInt32();
				break;
			case 6:
				rewardData.GemMax = reader.ReadInt32();
				break;
			case 7:
				rewardData.LuaPointRate = reader.ReadInt32();
				break;
			case 8:
				rewardData.LuaPointMin = reader.ReadInt32();
				break;
			case 9:
				rewardData.LuaPointMax = reader.ReadInt32();
				break;
			case 10:
				rewardData.ShopEventPointRate = reader.ReadInt32();
				break;
			case 11:
				rewardData.ShopEventPointMin = reader.ReadInt32();
				break;
			case 12:
				rewardData.ShopEventPointMax = reader.ReadInt32();
				break;
			case 13:
				rewardData.LimitedShopEventPointRate = reader.ReadInt32();
				break;
			case 14:
				rewardData.LimitedShopEventPointValue = reader.ReadInt32();
				break;
			case 15:
				rewardData.SubDataList = resolver.GetFormatterWithVerify<List<RewardData.SubData>>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return rewardData;
	}
}
