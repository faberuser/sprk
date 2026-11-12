using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DungeonHeroListSetKeyFormatter : IMessagePackFormatter<DungeonHeroListSetKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DungeonHeroListSetKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.GroupIndex);
		writer.Write(value.HeroIndex);
	}

	public DungeonHeroListSetKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int groupIndex = 0;
		int heroIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				groupIndex = reader.ReadInt32();
				break;
			case 1:
				heroIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		DungeonHeroListSetKey result = new DungeonHeroListSetKey(groupIndex, heroIndex);
		reader.Depth--;
		return result;
	}
}
