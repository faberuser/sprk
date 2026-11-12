using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MissionCategoryKeyFormatter : IMessagePackFormatter<MissionCategoryKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MissionCategoryKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.MainCategoryIndex);
		writer.Write(value.SubCategoryIndex);
	}

	public MissionCategoryKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int mainCategoryIndex = 0;
		int subCategoryIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mainCategoryIndex = reader.ReadInt32();
				break;
			case 1:
				subCategoryIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		MissionCategoryKey result = new MissionCategoryKey(mainCategoryIndex, subCategoryIndex);
		reader.Depth--;
		return result;
	}
}
