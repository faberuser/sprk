using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalRenewalDataFormatter : IMessagePackFormatter<PortalRenewalData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalRenewalData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(25);
		resolver.GetFormatterWithVerify<PortalRenewalCategory>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<PortalRenewalSubCategory>().Serialize(ref writer, value.SubCategory, options);
		writer.Write(value.MenuShowIndex);
		writer.Write(value.ContentsShowIndex);
		writer.Write(value.Pined);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MenuButtonLocalkey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleLocalKey, options);
		resolver.GetFormatterWithVerify<PortalRenewalCompType>().Serialize(ref writer, value.SubComponentShape, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MainImagePath, options);
		resolver.GetFormatterWithVerify<CurrencyShowType>().Serialize(ref writer, value.CurrencyShowType, options);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.CurrencyParam, options);
		writer.Write(value.ShopIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ShopParam, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescLocalKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenCondition, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ShowCondition, options);
		writer.Write(value.TutorialIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenConditionLocalKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EventBuffCondition, options);
		resolver.GetFormatterWithVerify<ClientActivityType>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
		resolver.GetFormatterWithVerify<ContentsStateIcon[]>().Serialize(ref writer, value.ContentsStateIcon, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AdventureNoticeMove, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.VisualChapterIndex, options);
	}

	public PortalRenewalData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PortalRenewalData portalRenewalData = new PortalRenewalData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				portalRenewalData.Category = resolver.GetFormatterWithVerify<PortalRenewalCategory>().Deserialize(ref reader, options);
				break;
			case 1:
				portalRenewalData.SubCategory = resolver.GetFormatterWithVerify<PortalRenewalSubCategory>().Deserialize(ref reader, options);
				break;
			case 2:
				portalRenewalData.MenuShowIndex = reader.ReadInt32();
				break;
			case 3:
				portalRenewalData.ContentsShowIndex = reader.ReadInt32();
				break;
			case 4:
				portalRenewalData.Pined = reader.ReadBoolean();
				break;
			case 5:
				portalRenewalData.MenuButtonLocalkey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				portalRenewalData.TitleLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				portalRenewalData.SubComponentShape = resolver.GetFormatterWithVerify<PortalRenewalCompType>().Deserialize(ref reader, options);
				break;
			case 8:
				portalRenewalData.MainImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				portalRenewalData.CurrencyShowType = resolver.GetFormatterWithVerify<CurrencyShowType>().Deserialize(ref reader, options);
				break;
			case 10:
				portalRenewalData.CurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 11:
				portalRenewalData.CurrencyParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				portalRenewalData.ShopIndex = reader.ReadInt32();
				break;
			case 13:
				portalRenewalData.ShopParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				portalRenewalData.DescLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				portalRenewalData.OpenCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 16:
				portalRenewalData.ShowCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 17:
				portalRenewalData.TutorialIndex = reader.ReadInt32();
				break;
			case 18:
				portalRenewalData.OpenConditionLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				portalRenewalData.EventBuffCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 20:
				portalRenewalData.MoveTaskType = resolver.GetFormatterWithVerify<ClientActivityType>().Deserialize(ref reader, options);
				break;
			case 21:
				portalRenewalData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 22:
				portalRenewalData.ContentsStateIcon = resolver.GetFormatterWithVerify<ContentsStateIcon[]>().Deserialize(ref reader, options);
				break;
			case 23:
				portalRenewalData.AdventureNoticeMove = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 24:
				portalRenewalData.VisualChapterIndex = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return portalRenewalData;
	}
}
