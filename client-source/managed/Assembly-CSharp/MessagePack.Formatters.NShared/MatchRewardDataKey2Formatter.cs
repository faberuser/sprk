using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchRewardDataKey2Formatter : IMessagePackFormatter<MatchRewardDataKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchRewardDataKey2 value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.TierIndex);
		writer.Write(value.MinRank);
	}

	public MatchRewardDataKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int tierIndex = 0;
		int minRank = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tierIndex = reader.ReadInt32();
				break;
			case 1:
				minRank = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		MatchRewardDataKey2 result = new MatchRewardDataKey2(tierIndex, minRank);
		reader.Depth--;
		return result;
	}
}
