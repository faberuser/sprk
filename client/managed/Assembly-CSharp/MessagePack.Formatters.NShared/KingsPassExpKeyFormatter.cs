using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassExpKeyFormatter : IMessagePackFormatter<KingsPassExpKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassExpKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.SeasonIndex);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.Type, options);
	}

	public KingsPassExpKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int seasonIndex = 0;
		StaminaType type = StaminaType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				seasonIndex = reader.ReadInt32();
				break;
			case 1:
				type = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KingsPassExpKey result = new KingsPassExpKey(seasonIndex, type);
		reader.Depth--;
		return result;
	}
}
