using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShakmehDungeonKeyFormatter : IMessagePackFormatter<ShakmehDungeonKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShakmehDungeonKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ShakmehIndex);
		writer.Write(value.Level);
	}

	public ShakmehDungeonKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int shakmehIndex = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shakmehIndex = reader.ReadInt32();
				break;
			case 1:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ShakmehDungeonKey result = new ShakmehDungeonKey(shakmehIndex, level);
		reader.Depth--;
		return result;
	}
}
