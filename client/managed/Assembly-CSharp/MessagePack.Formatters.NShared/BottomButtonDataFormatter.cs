using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BottomButtonDataFormatter : IMessagePackFormatter<BottomButtonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BottomButtonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<ERightMenuButton>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubType, options);
		writer.Write(value.TutorialIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LabelKey, options);
		writer.Write(value.BgScale);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ContentsOpenCode, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ForceOpenDungeon, options);
	}

	public BottomButtonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		BottomButtonData bottomButtonData = new BottomButtonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				bottomButtonData.Type = resolver.GetFormatterWithVerify<ERightMenuButton>().Deserialize(ref reader, options);
				break;
			case 1:
				bottomButtonData.SubType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				bottomButtonData.TutorialIndex = reader.ReadInt32();
				break;
			case 3:
				bottomButtonData.LabelKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				bottomButtonData.BgScale = reader.ReadSingle();
				break;
			case 5:
				bottomButtonData.ContentsOpenCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				bottomButtonData.ForceOpenDungeon = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return bottomButtonData;
	}
}
