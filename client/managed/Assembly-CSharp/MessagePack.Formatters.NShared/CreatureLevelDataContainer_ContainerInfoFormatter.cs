using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureLevelDataContainer_ContainerInfoFormatter : IMessagePackFormatter<CreatureLevelDataContainer.ContainerInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureLevelDataContainer.ContainerInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.MinLevel);
		writer.Write(value.MaxLevel);
		writer.Write(value.LastTotalExp);
	}

	public CreatureLevelDataContainer.ContainerInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureLevelDataContainer.ContainerInfo containerInfo = new CreatureLevelDataContainer.ContainerInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				containerInfo.MinLevel = reader.ReadInt32();
				break;
			case 1:
				containerInfo.MaxLevel = reader.ReadInt32();
				break;
			case 2:
				containerInfo.LastTotalExp = reader.ReadInt64();
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
