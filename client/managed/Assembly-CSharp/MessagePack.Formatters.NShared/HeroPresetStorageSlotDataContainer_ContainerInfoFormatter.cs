using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroPresetStorageSlotDataContainer_ContainerInfoFormatter : IMessagePackFormatter<HeroPresetStorageSlotDataContainer.ContainerInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroPresetStorageSlotDataContainer.ContainerInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.FreeSlotCount);
		writer.Write(value.PaidSlotCount);
	}

	public HeroPresetStorageSlotDataContainer.ContainerInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		HeroPresetStorageSlotDataContainer.ContainerInfo containerInfo = new HeroPresetStorageSlotDataContainer.ContainerInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				containerInfo.FreeSlotCount = reader.ReadInt32();
				break;
			case 1:
				containerInfo.PaidSlotCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return containerInfo;
	}
}
