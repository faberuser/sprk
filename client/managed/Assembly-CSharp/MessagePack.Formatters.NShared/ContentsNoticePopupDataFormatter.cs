using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsNoticePopupDataFormatter : IMessagePackFormatter<ContentsNoticePopupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsNoticePopupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<ContentsNoticeType>().Serialize(ref writer, value.ContentsType, options);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<ContentsNoticePopupType>().Serialize(ref writer, value.PopupType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ContentsImagePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleDescKey, options);
		writer.Write(value.UseRank);
		writer.Write(value.UseHonoraryChamber);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenCondition, options);
		writer.Write(value.TutorialIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenAction, options);
		resolver.GetFormatterWithVerify<ContentsStatusType>().Serialize(ref writer, value.ContentsStatusType, options);
		resolver.GetFormatterWithVerify<ContentsStatusSubType>().Serialize(ref writer, value.ContentsStatusSubType, options);
	}

	public ContentsNoticePopupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ContentsNoticePopupData contentsNoticePopupData = new ContentsNoticePopupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				contentsNoticePopupData.Index = reader.ReadInt32();
				break;
			case 1:
				contentsNoticePopupData.ContentsType = resolver.GetFormatterWithVerify<ContentsNoticeType>().Deserialize(ref reader, options);
				break;
			case 2:
				contentsNoticePopupData.ChapterIndex = reader.ReadInt32();
				break;
			case 3:
				contentsNoticePopupData.DungeonIndex = reader.ReadInt32();
				break;
			case 4:
				contentsNoticePopupData.PopupType = resolver.GetFormatterWithVerify<ContentsNoticePopupType>().Deserialize(ref reader, options);
				break;
			case 5:
				contentsNoticePopupData.ContentsImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				contentsNoticePopupData.TitleDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				contentsNoticePopupData.UseRank = reader.ReadBoolean();
				break;
			case 8:
				contentsNoticePopupData.UseHonoraryChamber = reader.ReadBoolean();
				break;
			case 9:
				contentsNoticePopupData.OpenCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				contentsNoticePopupData.TutorialIndex = reader.ReadInt32();
				break;
			case 11:
				contentsNoticePopupData.OpenAction = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				contentsNoticePopupData.ContentsStatusType = resolver.GetFormatterWithVerify<ContentsStatusType>().Deserialize(ref reader, options);
				break;
			case 13:
				contentsNoticePopupData.ContentsStatusSubType = resolver.GetFormatterWithVerify<ContentsStatusSubType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return contentsNoticePopupData;
	}
}
