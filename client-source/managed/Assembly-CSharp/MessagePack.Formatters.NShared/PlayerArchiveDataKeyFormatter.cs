using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayerArchiveDataKeyFormatter : IMessagePackFormatter<PlayerArchiveDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayerArchiveDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.MainIndex);
		writer.Write(value.SubIndex);
	}

	public PlayerArchiveDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int mainIndex = 0;
		int subIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mainIndex = reader.ReadInt32();
				break;
			case 1:
				subIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PlayerArchiveDataKey result = new PlayerArchiveDataKey(mainIndex, subIndex);
		reader.Depth--;
		return result;
	}
}
