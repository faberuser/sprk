using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayGuideDataFormatter : IMessagePackFormatter<PlayGuideData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayGuideData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<PlayGuideData.GuideType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition3, options);
		writer.Write(value.TutorialIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TextKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
	}

	public PlayGuideData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PlayGuideData playGuideData = new PlayGuideData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				playGuideData.Index = reader.ReadInt32();
				break;
			case 1:
				playGuideData.Type = resolver.GetFormatterWithVerify<PlayGuideData.GuideType>().Deserialize(ref reader, options);
				break;
			case 2:
				playGuideData.Condition1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 3:
				playGuideData.Condition2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				playGuideData.Condition3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				playGuideData.TutorialIndex = reader.ReadInt32();
				break;
			case 6:
				playGuideData.TextKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				playGuideData.MoveTaskType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				playGuideData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return playGuideData;
	}
}
