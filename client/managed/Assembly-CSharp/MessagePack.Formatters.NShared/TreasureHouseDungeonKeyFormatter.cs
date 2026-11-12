using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TreasureHouseDungeonKeyFormatter : IMessagePackFormatter<TreasureHouseDungeonKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TreasureHouseDungeonKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.InfoIndex);
		writer.Write(value.ShowIndex);
	}

	public TreasureHouseDungeonKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int infoIndex = 0;
		int showIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				infoIndex = reader.ReadInt32();
				break;
			case 1:
				showIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		TreasureHouseDungeonKey result = new TreasureHouseDungeonKey(infoIndex, showIndex);
		reader.Depth--;
		return result;
	}
}
