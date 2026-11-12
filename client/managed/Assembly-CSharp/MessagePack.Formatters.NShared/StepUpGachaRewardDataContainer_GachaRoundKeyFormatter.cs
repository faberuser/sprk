using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StepUpGachaRewardDataContainer_GachaRoundKeyFormatter : IMessagePackFormatter<StepUpGachaRewardDataContainer.GachaRoundKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StepUpGachaRewardDataContainer.GachaRoundKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.Round);
	}

	public StepUpGachaRewardDataContainer.GachaRoundKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int round = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				round = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		StepUpGachaRewardDataContainer.GachaRoundKey result = new StepUpGachaRewardDataContainer.GachaRoundKey(index, round);
		reader.Depth--;
		return result;
	}
}
