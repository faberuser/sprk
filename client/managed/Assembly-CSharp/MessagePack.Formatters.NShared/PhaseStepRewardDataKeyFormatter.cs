using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PhaseStepRewardDataKeyFormatter : IMessagePackFormatter<PhaseStepRewardDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PhaseStepRewardDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<PhaseStepRewardType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.Step);
	}

	public PhaseStepRewardDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int dungeonIndex = 0;
		PhaseStepRewardType type = PhaseStepRewardType.None;
		int step = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				dungeonIndex = reader.ReadInt32();
				break;
			case 2:
				type = resolver.GetFormatterWithVerify<PhaseStepRewardType>().Deserialize(ref reader, options);
				break;
			case 3:
				step = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PhaseStepRewardDataKey result = new PhaseStepRewardDataKey(chapterIndex, dungeonIndex, type, step);
		reader.Depth--;
		return result;
	}
}
