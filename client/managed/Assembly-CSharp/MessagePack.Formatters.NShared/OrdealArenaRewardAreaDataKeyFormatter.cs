using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OrdealArenaRewardAreaDataKeyFormatter : IMessagePackFormatter<OrdealArenaRewardAreaDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OrdealArenaRewardAreaDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<MatchTierType>().Serialize(ref writer, value.MatchTierType, options);
	}

	public OrdealArenaRewardAreaDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int index = 0;
		MatchTierType matchTierType = MatchTierType.Bronze;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				matchTierType = resolver.GetFormatterWithVerify<MatchTierType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		OrdealArenaRewardAreaDataKey result = new OrdealArenaRewardAreaDataKey(index, matchTierType);
		reader.Depth--;
		return result;
	}
}
