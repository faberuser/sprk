using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopItemGroupKeyFormatter : IMessagePackFormatter<ShopItemGroupKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopItemGroupKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ShopIndex);
		writer.Write(value.GroupIndex);
	}

	public ShopItemGroupKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int shopIndex = 0;
		int groupIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopIndex = reader.ReadInt32();
				break;
			case 1:
				groupIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ShopItemGroupKey result = new ShopItemGroupKey(shopIndex, groupIndex);
		reader.Depth--;
		return result;
	}
}
