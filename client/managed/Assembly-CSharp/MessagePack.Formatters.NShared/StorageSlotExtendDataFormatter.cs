using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StorageSlotExtendDataFormatter : IMessagePackFormatter<StorageSlotExtendData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StorageSlotExtendData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		writer.Write(value.SlotCount);
		writer.Write(value.ExtendPrice);
		writer.Write(value.IsDefaultOpen);
	}

	public StorageSlotExtendData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		StorageSlotExtendData storageSlotExtendData = new StorageSlotExtendData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				storageSlotExtendData.Index = reader.ReadInt32();
				break;
			case 1:
				storageSlotExtendData.SlotCount = reader.ReadInt32();
				break;
			case 2:
				storageSlotExtendData.ExtendPrice = reader.ReadInt32();
				break;
			case 3:
				storageSlotExtendData.IsDefaultOpen = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return storageSlotExtendData;
	}
}
