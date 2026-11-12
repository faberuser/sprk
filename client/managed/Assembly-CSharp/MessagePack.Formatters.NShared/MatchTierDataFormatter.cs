using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchTierDataFormatter : IMessagePackFormatter<MatchTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchTierData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<MatchTierType>().Serialize(ref writer, value.Tier, options);
		writer.Write(value.Level);
		writer.Write(value.MinScore);
		writer.Write(value.MaxScore);
		writer.Write(value.MinPercent);
		writer.Write(value.MaxPercent);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
	}

	public MatchTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchTierData matchTierData = new MatchTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchTierData.Index = reader.ReadInt32();
				break;
			case 1:
				matchTierData.Tier = resolver.GetFormatterWithVerify<MatchTierType>().Deserialize(ref reader, options);
				break;
			case 2:
				matchTierData.Level = reader.ReadInt32();
				break;
			case 3:
				matchTierData.MinScore = reader.ReadInt32();
				break;
			case 4:
				matchTierData.MaxScore = reader.ReadInt32();
				break;
			case 5:
				matchTierData.MinPercent = reader.ReadSingle();
				break;
			case 6:
				matchTierData.MaxPercent = reader.ReadSingle();
				break;
			case 7:
				matchTierData.MinRank = reader.ReadInt32();
				break;
			case 8:
				matchTierData.MaxRank = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchTierData;
	}
}
