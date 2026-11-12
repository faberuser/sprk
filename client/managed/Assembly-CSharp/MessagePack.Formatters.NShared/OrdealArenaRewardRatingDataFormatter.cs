using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OrdealArenaRewardRatingDataFormatter : IMessagePackFormatter<OrdealArenaRewardRatingData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OrdealArenaRewardRatingData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<MatchTierType>().Serialize(ref writer, value.TierType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RatingArrange, options);
		writer.Write(value.RewardIndex);
	}

	public OrdealArenaRewardRatingData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OrdealArenaRewardRatingData ordealArenaRewardRatingData = new OrdealArenaRewardRatingData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				ordealArenaRewardRatingData.Index = reader.ReadInt32();
				break;
			case 1:
				ordealArenaRewardRatingData.TierType = resolver.GetFormatterWithVerify<MatchTierType>().Deserialize(ref reader, options);
				break;
			case 2:
				ordealArenaRewardRatingData.RatingArrange = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				ordealArenaRewardRatingData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return ordealArenaRewardRatingData;
	}
}
