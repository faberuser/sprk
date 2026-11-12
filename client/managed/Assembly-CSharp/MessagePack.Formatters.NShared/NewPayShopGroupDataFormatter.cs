using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopGroupDataFormatter : IMessagePackFormatter<NewPayShopGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PayShopCategoryType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CategoryName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ParentCategory, options);
		writer.Write(value.IsCustomize);
		writer.Write(value.Sort);
		writer.Write(value.IsEnable);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.LinkCurrency, options);
		resolver.GetFormatterWithVerify<EGroupEnableConditionType>().Serialize(ref writer, value.EnableCondition, options);
		resolver.GetFormatterWithVerify<NewPayShopCategoryType>().Serialize(ref writer, value.EPayShopCategoryType, options);
		resolver.GetFormatterWithVerify<NewPayShopCategoryType>().Serialize(ref writer, value.EParentCategory, options);
	}

	public NewPayShopGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewPayShopGroupData newPayShopGroupData = new NewPayShopGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newPayShopGroupData.PayShopCategoryType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				newPayShopGroupData.CategoryName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				newPayShopGroupData.ParentCategory = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				newPayShopGroupData.IsCustomize = reader.ReadBoolean();
				break;
			case 4:
				newPayShopGroupData.Sort = reader.ReadInt32();
				break;
			case 5:
				newPayShopGroupData.IsEnable = reader.ReadBoolean();
				break;
			case 6:
				newPayShopGroupData.LinkCurrency = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 7:
				newPayShopGroupData.EnableCondition = resolver.GetFormatterWithVerify<EGroupEnableConditionType>().Deserialize(ref reader, options);
				break;
			case 8:
				newPayShopGroupData.EPayShopCategoryType = resolver.GetFormatterWithVerify<NewPayShopCategoryType>().Deserialize(ref reader, options);
				break;
			case 9:
				newPayShopGroupData.EParentCategory = resolver.GetFormatterWithVerify<NewPayShopCategoryType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newPayShopGroupData;
	}
}
