using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopAndIndexKeyFormatter : IMessagePackFormatter<ShopAndIndexKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopAndIndexKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ShopIndex);
		writer.Write(value.Index);
	}

	public ShopAndIndexKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int shopIndex = 0;
		int index = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopIndex = reader.ReadInt32();
				break;
			case 1:
				index = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ShopAndIndexKey result = new ShopAndIndexKey(shopIndex, index);
		reader.Depth--;
		return result;
	}
}
