using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassLevelKeyFormatter : IMessagePackFormatter<KingsPassLevelKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassLevelKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.LevelIndex);
		writer.Write(value.Level);
	}

	public KingsPassLevelKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int levelIndex = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				levelIndex = reader.ReadInt32();
				break;
			case 1:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KingsPassLevelKey result = new KingsPassLevelKey(levelIndex, level);
		reader.Depth--;
		return result;
	}
}
