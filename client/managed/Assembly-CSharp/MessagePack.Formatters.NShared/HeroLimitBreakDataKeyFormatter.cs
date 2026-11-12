using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroLimitBreakDataKeyFormatter : IMessagePackFormatter<HeroLimitBreakDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroLimitBreakDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.HeroIndex);
		writer.Write(value.LimitBreakLevel);
	}

	public HeroLimitBreakDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int heroIndex = 0;
		int limitBreakLevel = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroIndex = reader.ReadInt32();
				break;
			case 1:
				limitBreakLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		HeroLimitBreakDataKey result = new HeroLimitBreakDataKey(heroIndex, limitBreakLevel);
		reader.Depth--;
		return result;
	}
}
