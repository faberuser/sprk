using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LuckyArenaRewardDataKeyFormatter : IMessagePackFormatter<LuckyArenaRewardDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LuckyArenaRewardDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<LuckyMatchRewardType>().Serialize(ref writer, value.TierIndex, options);
		writer.Write(value.ShowIndex);
	}

	public LuckyArenaRewardDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LuckyMatchRewardType tierIndex = LuckyMatchRewardType.None;
		int showIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tierIndex = resolver.GetFormatterWithVerify<LuckyMatchRewardType>().Deserialize(ref reader, options);
				break;
			case 1:
				showIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		LuckyArenaRewardDataKey result = new LuckyArenaRewardDataKey(tierIndex, showIndex);
		reader.Depth--;
		return result;
	}
}
