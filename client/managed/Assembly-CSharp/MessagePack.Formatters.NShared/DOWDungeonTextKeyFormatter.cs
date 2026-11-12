using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DOWDungeonTextKeyFormatter : IMessagePackFormatter<DOWDungeonTextKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DOWDungeonTextKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Diff);
	}

	public DOWDungeonTextKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int dungeonIndex = 0;
		int diff = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dungeonIndex = reader.ReadInt32();
				break;
			case 1:
				diff = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		DOWDungeonTextKey result = new DOWDungeonTextKey(dungeonIndex, diff);
		reader.Depth--;
		return result;
	}
}
