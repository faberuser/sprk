using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventWorldBossDataKeyFormatter : IMessagePackFormatter<EventWorldBossDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventWorldBossDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.BossLevel);
	}

	public EventWorldBossDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int bossLevel = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				bossLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EventWorldBossDataKey result = new EventWorldBossDataKey(index, bossLevel);
		reader.Depth--;
		return result;
	}
}
