using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemLevelDataContainer_ContainerInfoFormatter : IMessagePackFormatter<EquipItemLevelDataContainer.ContainerInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemLevelDataContainer.ContainerInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(1);
		writer.Write(value.MaxLevel);
	}

	public EquipItemLevelDataContainer.ContainerInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipItemLevelDataContainer.ContainerInfo containerInfo = new EquipItemLevelDataContainer.ContainerInfo();
		for (int i = 0; i < num; i++)
		{
			if (i == 0)
			{
				containerInfo.MaxLevel = reader.ReadInt32();
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Depth--;
		return containerInfo;
	}
}
