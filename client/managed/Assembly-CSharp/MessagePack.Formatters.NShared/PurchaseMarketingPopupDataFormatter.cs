using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PurchaseMarketingPopupDataFormatter : IMessagePackFormatter<PurchaseMarketingPopupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PurchaseMarketingPopupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(21);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PurchaseIndices, options);
		writer.Write(value.PopupPriority);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopupPrefabName, options);
		resolver.GetFormatterWithVerify<MarketingType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleStringKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescStringKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopupImage, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition3, options);
		resolver.GetFormatterWithVerify<OpenConditionType>().Serialize(ref writer, value.OpenCondition, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.SkipCondition1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.SkipCondition2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.SkipCondition3, options);
		writer.Write(value.RemindMinutes);
		writer.Write(value.PushID);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PushSendMinute, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.PushLocalText, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenDate, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ServerCondition, options);
	}

	public PurchaseMarketingPopupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PurchaseMarketingPopupData purchaseMarketingPopupData = new PurchaseMarketingPopupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				purchaseMarketingPopupData.Index = reader.ReadInt32();
				break;
			case 1:
				purchaseMarketingPopupData.PurchaseIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				purchaseMarketingPopupData.PopupPriority = reader.ReadInt32();
				break;
			case 3:
				purchaseMarketingPopupData.PopupPrefabName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				purchaseMarketingPopupData.Type = resolver.GetFormatterWithVerify<MarketingType>().Deserialize(ref reader, options);
				break;
			case 5:
				purchaseMarketingPopupData.TitleStringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				purchaseMarketingPopupData.DescStringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				purchaseMarketingPopupData.PopupImage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				purchaseMarketingPopupData.Condition1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				purchaseMarketingPopupData.Condition2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				purchaseMarketingPopupData.Condition3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 11:
				purchaseMarketingPopupData.OpenCondition = resolver.GetFormatterWithVerify<OpenConditionType>().Deserialize(ref reader, options);
				break;
			case 12:
				purchaseMarketingPopupData.SkipCondition1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 13:
				purchaseMarketingPopupData.SkipCondition2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				purchaseMarketingPopupData.SkipCondition3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 15:
				purchaseMarketingPopupData.RemindMinutes = reader.ReadInt32();
				break;
			case 16:
				purchaseMarketingPopupData.PushID = reader.ReadInt32();
				break;
			case 17:
				purchaseMarketingPopupData.PushSendMinute = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 18:
				purchaseMarketingPopupData.PushLocalText = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 19:
				purchaseMarketingPopupData.OpenDate = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				purchaseMarketingPopupData.ServerCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return purchaseMarketingPopupData;
	}
}
