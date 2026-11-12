using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AutoSeparationDataFormatter : IMessagePackFormatter<AutoSeparationData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AutoSeparationData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<BattleType[]>().Serialize(ref writer, value.BattleType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalAutoKey, options);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
	}

	public AutoSeparationData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AutoSeparationData autoSeparationData = new AutoSeparationData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				autoSeparationData.Index = reader.ReadInt32();
				break;
			case 1:
				autoSeparationData.BattleType = resolver.GetFormatterWithVerify<BattleType[]>().Deserialize(ref reader, options);
				break;
			case 2:
				autoSeparationData.LocalAutoKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				autoSeparationData.ChapterIndex = reader.ReadInt32();
				break;
			case 4:
				autoSeparationData.DungeonIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return autoSeparationData;
	}
}
