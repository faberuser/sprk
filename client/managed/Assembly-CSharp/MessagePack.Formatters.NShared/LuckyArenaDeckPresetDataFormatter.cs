using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LuckyArenaDeckPresetDataFormatter : IMessagePackFormatter<LuckyArenaDeckPresetData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LuckyArenaDeckPresetData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DeckName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RouletteDeckName, options);
		writer.Write(value.Hero1);
		writer.Write(value.Hero2);
		writer.Write(value.Hero3);
		writer.Write(value.Hero4);
	}

	public LuckyArenaDeckPresetData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LuckyArenaDeckPresetData luckyArenaDeckPresetData = new LuckyArenaDeckPresetData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				luckyArenaDeckPresetData.Index = reader.ReadInt32();
				break;
			case 1:
				luckyArenaDeckPresetData.DeckName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				luckyArenaDeckPresetData.RouletteDeckName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				luckyArenaDeckPresetData.Hero1 = reader.ReadInt32();
				break;
			case 4:
				luckyArenaDeckPresetData.Hero2 = reader.ReadInt32();
				break;
			case 5:
				luckyArenaDeckPresetData.Hero3 = reader.ReadInt32();
				break;
			case 6:
				luckyArenaDeckPresetData.Hero4 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return luckyArenaDeckPresetData;
	}
}
