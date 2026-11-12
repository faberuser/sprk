using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassSeasonDataFormatter : IMessagePackFormatter<KingsPassSeasonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassSeasonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.SeasonIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BeginTime, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EndTime, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey, options);
		writer.Write(value.ExpIndex);
		writer.Write(value.LevelIndex);
		writer.Write(value.ProductIndex);
	}

	public KingsPassSeasonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		KingsPassSeasonData kingsPassSeasonData = new KingsPassSeasonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				kingsPassSeasonData.SeasonIndex = reader.ReadInt32();
				break;
			case 1:
				kingsPassSeasonData.BeginTime = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				kingsPassSeasonData.EndTime = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				kingsPassSeasonData.TitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				kingsPassSeasonData.ExpIndex = reader.ReadInt32();
				break;
			case 5:
				kingsPassSeasonData.LevelIndex = reader.ReadInt32();
				break;
			case 6:
				kingsPassSeasonData.ProductIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return kingsPassSeasonData;
	}
}
