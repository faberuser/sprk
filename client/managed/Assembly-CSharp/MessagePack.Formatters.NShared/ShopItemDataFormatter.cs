using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopItemDataFormatter : IMessagePackFormatter<ShopItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.ShopIndex);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.Count);
		writer.Write(value.Star);
		writer.Write(value.PurchasableCount);
		writer.Write(value.DailyReset);
		writer.Write(value.WeeklyReset);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.MonthlyResetDate, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Condition, options);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.BuyCostTypeForItem, options);
		writer.Write(value.TargetPrice);
	}

	public ShopItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShopItemData shopItemData = new ShopItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopItemData.ShopIndex = reader.ReadInt32();
				break;
			case 1:
				shopItemData.Index = reader.ReadInt32();
				break;
			case 2:
				shopItemData.GroupIndex = reader.ReadInt32();
				break;
			case 3:
				shopItemData.ShowIndex = reader.ReadInt32();
				break;
			case 4:
				shopItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				shopItemData.Count = reader.ReadInt32();
				break;
			case 6:
				shopItemData.Star = reader.ReadInt32();
				break;
			case 7:
				shopItemData.PurchasableCount = reader.ReadInt32();
				break;
			case 8:
				shopItemData.DailyReset = reader.ReadBoolean();
				break;
			case 9:
				shopItemData.WeeklyReset = reader.ReadBoolean();
				break;
			case 10:
				shopItemData.MonthlyResetDate = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 11:
				shopItemData.Condition = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				shopItemData.BuyCostTypeForItem = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 13:
				shopItemData.TargetPrice = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shopItemData;
	}
}
