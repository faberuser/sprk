using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClearMissionProductDataFormatter : IMessagePackFormatter<ClearMissionProductData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClearMissionProductData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MissionName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MissionInfo, options);
		resolver.GetFormatterWithVerify<ClearMissionType>().Serialize(ref writer, value.MissionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MissionOpenCondition, options);
		writer.Write(value.PayShopProductIndex);
		resolver.GetFormatterWithVerify<ClearMissionPurchaseType>().Serialize(ref writer, value.PurchaseType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ProductImagePath, options);
		writer.Write(value.ProductGroupIndex);
		writer.Write(value.ShowIndex);
	}

	public ClearMissionProductData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ClearMissionProductData clearMissionProductData = new ClearMissionProductData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				clearMissionProductData.Index = reader.ReadInt32();
				break;
			case 1:
				clearMissionProductData.MissionName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				clearMissionProductData.MissionInfo = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				clearMissionProductData.MissionType = resolver.GetFormatterWithVerify<ClearMissionType>().Deserialize(ref reader, options);
				break;
			case 4:
				clearMissionProductData.MissionOpenCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				clearMissionProductData.PayShopProductIndex = reader.ReadInt32();
				break;
			case 6:
				clearMissionProductData.PurchaseType = resolver.GetFormatterWithVerify<ClearMissionPurchaseType>().Deserialize(ref reader, options);
				break;
			case 7:
				clearMissionProductData.ProductImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				clearMissionProductData.ProductGroupIndex = reader.ReadInt32();
				break;
			case 9:
				clearMissionProductData.ShowIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return clearMissionProductData;
	}
}
