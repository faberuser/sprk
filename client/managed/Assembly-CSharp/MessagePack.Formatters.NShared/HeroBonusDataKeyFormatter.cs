using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroBonusDataKeyFormatter : IMessagePackFormatter<HeroBonusDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroBonusDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.HeroIndex);
		writer.Write(value.HeroStar);
		writer.Write(value.HeroTranscended);
	}

	public HeroBonusDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int heroIndex = 0;
		int heroStar = 0;
		int heroTranscended = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroIndex = reader.ReadInt32();
				break;
			case 1:
				heroStar = reader.ReadInt32();
				break;
			case 2:
				heroTranscended = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		HeroBonusDataKey result = new HeroBonusDataKey(heroIndex, heroStar, heroTranscended);
		reader.Depth--;
		return result;
	}
}
