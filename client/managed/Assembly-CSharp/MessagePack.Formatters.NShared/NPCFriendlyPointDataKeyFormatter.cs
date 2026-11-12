using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NPCFriendlyPointDataKeyFormatter : IMessagePackFormatter<NPCFriendlyPointDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NPCFriendlyPointDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.HeroIndex);
		writer.Write(value.Step);
	}

	public NPCFriendlyPointDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int heroIndex = 0;
		int step = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroIndex = reader.ReadInt32();
				break;
			case 1:
				step = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		NPCFriendlyPointDataKey result = new NPCFriendlyPointDataKey(heroIndex, step);
		reader.Depth--;
		return result;
	}
}
