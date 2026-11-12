using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaDungeonDataKeyFormatter : IMessagePackFormatter<KarmaDungeonDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaDungeonDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.StageLevel);
	}

	public KarmaDungeonDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int punishmentGroupIndex = 0;
		int stageLevel = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				stageLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KarmaDungeonDataKey result = new KarmaDungeonDataKey(punishmentGroupIndex, stageLevel);
		reader.Depth--;
		return result;
	}
}
