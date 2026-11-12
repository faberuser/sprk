using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildBuildingDataKeyFormatter : IMessagePackFormatter<GuildBuildingDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildBuildingDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.Level);
	}

	public GuildBuildingDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GuildBuildingDataKey result = new GuildBuildingDataKey(index, level);
		reader.Depth--;
		return result;
	}
}
