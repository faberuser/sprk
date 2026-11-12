using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureGradeDataContainer_ContainerInfoFormatter : IMessagePackFormatter<CreatureGradeDataContainer.ContainerInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureGradeDataContainer.ContainerInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.MinGrade);
		writer.Write(value.MaxGrade);
	}

	public CreatureGradeDataContainer.ContainerInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureGradeDataContainer.ContainerInfo containerInfo = new CreatureGradeDataContainer.ContainerInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				containerInfo.MinGrade = reader.ReadInt32();
				break;
			case 1:
				containerInfo.MaxGrade = reader.ReadInt32();
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
