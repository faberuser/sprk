using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TowerFloorKeyFormatter : IMessagePackFormatter<TowerFloorKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TowerFloorKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.Floor);
	}

	public TowerFloorKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int floor = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				floor = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		TowerFloorKey result = new TowerFloorKey(index, floor);
		reader.Depth--;
		return result;
	}
}
