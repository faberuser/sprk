using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OrdealArenaRewardAreaDataFormatter : IMessagePackFormatter<OrdealArenaRewardAreaData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OrdealArenaRewardAreaData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<MatchTierType>().Serialize(ref writer, value.TierType, options);
		writer.Write(value.RewardIndex);
	}

	public OrdealArenaRewardAreaData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OrdealArenaRewardAreaData ordealArenaRewardAreaData = new OrdealArenaRewardAreaData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				ordealArenaRewardAreaData.Index = reader.ReadInt32();
				break;
			case 1:
				ordealArenaRewardAreaData.TierType = resolver.GetFormatterWithVerify<MatchTierType>().Deserialize(ref reader, options);
				break;
			case 2:
				ordealArenaRewardAreaData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return ordealArenaRewardAreaData;
	}
}
