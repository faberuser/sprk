using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InventoryExtendDataFormatter : IMessagePackFormatter<InventoryExtendData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InventoryExtendData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		writer.Write(value.EquipItemExtendCount);
		writer.Write(value.EquipItemExtendPrice);
		writer.Write(value.EquipItemExtendSize);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ProductIndex, options);
		resolver.GetFormatterWithVerify<InventoryType>().Serialize(ref writer, value.InventoryType, options);
	}

	public InventoryExtendData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		InventoryExtendData inventoryExtendData = new InventoryExtendData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				inventoryExtendData.Index = reader.ReadInt32();
				break;
			case 1:
				inventoryExtendData.EquipItemExtendCount = reader.ReadInt32();
				break;
			case 2:
				inventoryExtendData.EquipItemExtendPrice = reader.ReadInt32();
				break;
			case 3:
				inventoryExtendData.EquipItemExtendSize = reader.ReadInt32();
				break;
			case 4:
				inventoryExtendData.ProductIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				inventoryExtendData.InventoryType = resolver.GetFormatterWithVerify<InventoryType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return inventoryExtendData;
	}
}
