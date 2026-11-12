using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemBreakDataKeyFormatter : IMessagePackFormatter<EquipItemBreakDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemBreakDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.Star);
		writer.Write(value.SetIndex);
	}

	public EquipItemBreakDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int tier = 0;
		int detailIndex = 0;
		string itemCode = null;
		int star = 0;
		int setIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tier = reader.ReadInt32();
				break;
			case 1:
				detailIndex = reader.ReadInt32();
				break;
			case 2:
				itemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				star = reader.ReadInt32();
				break;
			case 4:
				setIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EquipItemBreakDataKey result = new EquipItemBreakDataKey(tier, detailIndex, itemCode, star, setIndex);
		reader.Depth--;
		return result;
	}
}
