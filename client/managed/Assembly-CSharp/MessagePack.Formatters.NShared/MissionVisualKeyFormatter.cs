using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MissionVisualKeyFormatter : IMessagePackFormatter<MissionVisualKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MissionVisualKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(4);
		writer.Write(value.MainCategoryIndex);
		writer.Write(value.SubCategoryIndex);
		writer.Write(value.MainStep);
		writer.Write(value.SubStep);
	}

	public MissionVisualKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int mainCategory = 0;
		int subCategory = 0;
		int mainStep = 0;
		int subStep = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mainCategory = reader.ReadInt32();
				break;
			case 1:
				subCategory = reader.ReadInt32();
				break;
			case 2:
				mainStep = reader.ReadInt32();
				break;
			case 3:
				subStep = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		MissionVisualKey result = new MissionVisualKey(mainCategory, subCategory, mainStep, subStep);
		reader.Depth--;
		return result;
	}
}
