using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchBanPickTierDataFormatter : IMessagePackFormatter<MatchBanPickTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchBanPickTierData value, MessagePackSerializerOptions options)
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

	public MatchBanPickTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchBanPickTierData matchBanPickTierData = new MatchBanPickTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchBanPickTierData.Index = reader.ReadInt32();
				break;
			case 1:
				matchBanPickTierData.Tier = resolver.GetFormatterWithVerify<MatchTierType>().Deserialize(ref reader, options);
				break;
			case 2:
				matchBanPickTierData.Level = reader.ReadInt32();
				break;
			case 3:
				matchBanPickTierData.MinScore = reader.ReadInt32();
				break;
			case 4:
				matchBanPickTierData.MaxScore = reader.ReadInt32();
				break;
			case 5:
				matchBanPickTierData.MinPercent = reader.ReadSingle();
				break;
			case 6:
				matchBanPickTierData.MaxPercent = reader.ReadSingle();
				break;
			case 7:
				matchBanPickTierData.MinRank = reader.ReadInt32();
				break;
			case 8:
				matchBanPickTierData.MaxRank = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchBanPickTierData;
	}
}
