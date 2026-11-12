using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CostumeDataContainer_HeroAndStarKeyFormatter : IMessagePackFormatter<CostumeDataContainer.HeroAndStarKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CostumeDataContainer.HeroAndStarKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.Star);
	}

	public CostumeDataContainer.HeroAndStarKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int star = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				star = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		CostumeDataContainer.HeroAndStarKey result = new CostumeDataContainer.HeroAndStarKey(index, star);
		reader.Depth--;
		return result;
	}
}
