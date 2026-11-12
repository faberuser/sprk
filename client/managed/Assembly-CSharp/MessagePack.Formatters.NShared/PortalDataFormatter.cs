using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalDataFormatter : IMessagePackFormatter<PortalData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(19);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ParentNode, options);
		writer.Write(value.PositionX);
		writer.Write(value.PositionY);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ImagePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopupImagePath, options);
		writer.Write(value.HasChild);
		writer.Write(value.ExecuteNodeFunction);
		writer.Write(value.Hidden);
		writer.Write(value.ShortCutButton);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleDescKey, options);
		writer.Write(value.ContentsShortCutButton);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AdventureNoticeCode, options);
		writer.Write(value.OpenEffectPortalIndex);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.PortalGaugeCurrencyType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortalGaugePath, options);
	}

	public PortalData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PortalData portalData = new PortalData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				portalData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				portalData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				portalData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				portalData.ParentNode = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				portalData.PositionX = reader.ReadInt32();
				break;
			case 5:
				portalData.PositionY = reader.ReadInt32();
				break;
			case 6:
				portalData.ImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				portalData.PopupImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				portalData.HasChild = reader.ReadBoolean();
				break;
			case 9:
				portalData.ExecuteNodeFunction = reader.ReadBoolean();
				break;
			case 10:
				portalData.Hidden = reader.ReadBoolean();
				break;
			case 11:
				portalData.ShortCutButton = reader.ReadBoolean();
				break;
			case 12:
				portalData.Condition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 13:
				portalData.TitleDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				portalData.ContentsShortCutButton = reader.ReadBoolean();
				break;
			case 15:
				portalData.AdventureNoticeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				portalData.OpenEffectPortalIndex = reader.ReadInt32();
				break;
			case 17:
				portalData.PortalGaugeCurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 18:
				portalData.PortalGaugePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return portalData;
	}
}
