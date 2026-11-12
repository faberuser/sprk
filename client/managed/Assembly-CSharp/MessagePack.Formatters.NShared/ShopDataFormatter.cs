using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopDataFormatter : IMessagePackFormatter<ShopData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(19);
		writer.Write(value.Index);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.BuyCostType, options);
		resolver.GetFormatterWithVerify<ShopCostType[]>().Serialize(ref writer, value.BuyCostTypeUI, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BuyCostTypeIconSprite, options);
		writer.Write(value.MaxStock);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Timeline, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Buyline, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GroupTitleKey, options);
		writer.Write(value.BeforeBuyConfirm);
		writer.Write(value.RestockSecond);
		writer.Write(value.RestockGem);
		writer.Write(value.EventOnly);
		writer.Write(value.MiniGuideIndex);
		writer.Write(value.UseGroupIndexForLevel);
		writer.Write(value.MultiplePurchaseShop);
		resolver.GetFormatterWithVerify<EShopItemFilterGroupType>().Serialize(ref writer, value.ShopFilterGroupType, options);
	}

	public ShopData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShopData shopData = new ShopData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopData.Index = reader.ReadInt32();
				break;
			case 2:
				shopData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				shopData.HeroIndex = reader.ReadInt32();
				break;
			case 4:
				shopData.BuyCostType = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 5:
				shopData.BuyCostTypeUI = resolver.GetFormatterWithVerify<ShopCostType[]>().Deserialize(ref reader, options);
				break;
			case 6:
				shopData.BuyCostTypeIconSprite = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				shopData.MaxStock = reader.ReadInt32();
				break;
			case 8:
				shopData.Timeline = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				shopData.Buyline = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				shopData.GroupTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				shopData.BeforeBuyConfirm = reader.ReadBoolean();
				break;
			case 12:
				shopData.RestockSecond = reader.ReadInt32();
				break;
			case 13:
				shopData.RestockGem = reader.ReadInt32();
				break;
			case 14:
				shopData.EventOnly = reader.ReadByte();
				break;
			case 15:
				shopData.MiniGuideIndex = reader.ReadInt32();
				break;
			case 16:
				shopData.UseGroupIndexForLevel = reader.ReadBoolean();
				break;
			case 17:
				shopData.MultiplePurchaseShop = reader.ReadBoolean();
				break;
			case 18:
				shopData.ShopFilterGroupType = resolver.GetFormatterWithVerify<EShopItemFilterGroupType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shopData;
	}
}
