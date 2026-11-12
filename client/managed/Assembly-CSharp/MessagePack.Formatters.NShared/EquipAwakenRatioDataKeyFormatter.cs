using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipAwakenRatioDataKeyFormatter : IMessagePackFormatter<EquipAwakenRatioDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipAwakenRatioDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Star);
		writer.Write(value.AwakenPoint);
	}

	public EquipAwakenRatioDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int star = 0;
		int awakenPoint = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				star = reader.ReadInt32();
				break;
			case 1:
				awakenPoint = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EquipAwakenRatioDataKey result = new EquipAwakenRatioDataKey(star, awakenPoint);
		reader.Depth--;
		return result;
	}
}
