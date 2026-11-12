using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OrdealArenaTierDataFormatter : IMessagePackFormatter<OrdealArenaTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OrdealArenaTierData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<MatchTierType>().Serialize(ref writer, value.MatchTierType, options);
		writer.Write(value.StartRating);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		writer.Write(value.SortingOrder);
		writer.Write(value.MoveChapterIndex);
	}

	public OrdealArenaTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OrdealArenaTierData ordealArenaTierData = new OrdealArenaTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				ordealArenaTierData.MatchTierType = resolver.GetFormatterWithVerify<MatchTierType>().Deserialize(ref reader, options);
				break;
			case 1:
				ordealArenaTierData.StartRating = reader.ReadInt32();
				break;
			case 2:
				ordealArenaTierData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				ordealArenaTierData.SortingOrder = reader.ReadInt32();
				break;
			case 4:
				ordealArenaTierData.MoveChapterIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return ordealArenaTierData;
	}
}
