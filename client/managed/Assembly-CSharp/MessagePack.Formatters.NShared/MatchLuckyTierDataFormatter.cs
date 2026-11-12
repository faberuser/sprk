using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchLuckyTierDataFormatter : IMessagePackFormatter<MatchLuckyTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchLuckyTierData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<LuckyMatchTierType>().Serialize(ref writer, value.Tier, options);
		writer.Write(value.Level);
		writer.Write(value.MinScore);
		writer.Write(value.MaxScore);
		writer.Write(value.MinPercent);
		writer.Write(value.MaxPercent);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
	}

	public MatchLuckyTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchLuckyTierData matchLuckyTierData = new MatchLuckyTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchLuckyTierData.Index = reader.ReadInt32();
				break;
			case 1:
				matchLuckyTierData.Tier = resolver.GetFormatterWithVerify<LuckyMatchTierType>().Deserialize(ref reader, options);
				break;
			case 2:
				matchLuckyTierData.Level = reader.ReadInt32();
				break;
			case 3:
				matchLuckyTierData.MinScore = reader.ReadInt32();
				break;
			case 4:
				matchLuckyTierData.MaxScore = reader.ReadInt32();
				break;
			case 5:
				matchLuckyTierData.MinPercent = reader.ReadSingle();
				break;
			case 6:
				matchLuckyTierData.MaxPercent = reader.ReadSingle();
				break;
			case 7:
				matchLuckyTierData.MinRank = reader.ReadInt32();
				break;
			case 8:
				matchLuckyTierData.MaxRank = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchLuckyTierData;
	}
}
