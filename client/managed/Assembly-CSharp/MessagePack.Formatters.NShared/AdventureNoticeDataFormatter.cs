using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AdventureNoticeDataFormatter : IMessagePackFormatter<AdventureNoticeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AdventureNoticeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Action, options);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.StaminaType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenCondition, options);
		writer.Write(value.TutorialIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DisableText, options);
		writer.Write(value.EnableEvent);
		writer.Write(value.NodeChapterIndex);
		writer.Write(value.NodeDungeonIndex);
		resolver.GetFormatterWithVerify<BattleType>().Serialize(ref writer, value.BattleType, options);
		writer.Write(value.UseLocalTime);
	}

	public AdventureNoticeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AdventureNoticeData adventureNoticeData = new AdventureNoticeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				adventureNoticeData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				adventureNoticeData.Action = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 2:
				adventureNoticeData.StaminaType = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			case 3:
				adventureNoticeData.OpenCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				adventureNoticeData.TutorialIndex = reader.ReadInt32();
				break;
			case 5:
				adventureNoticeData.DisableText = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				adventureNoticeData.EnableEvent = reader.ReadBoolean();
				break;
			case 7:
				adventureNoticeData.NodeChapterIndex = reader.ReadInt32();
				break;
			case 8:
				adventureNoticeData.NodeDungeonIndex = reader.ReadInt32();
				break;
			case 9:
				adventureNoticeData.BattleType = resolver.GetFormatterWithVerify<BattleType>().Deserialize(ref reader, options);
				break;
			case 10:
				adventureNoticeData.UseLocalTime = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return adventureNoticeData;
	}
}
