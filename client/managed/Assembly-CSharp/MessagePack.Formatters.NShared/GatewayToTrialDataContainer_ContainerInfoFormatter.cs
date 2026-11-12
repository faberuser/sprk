using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GatewayToTrialDataContainer_ContainerInfoFormatter : IMessagePackFormatter<GatewayToTrialDataContainer.ContainerInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GatewayToTrialDataContainer.ContainerInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.MinStep);
		writer.Write(value.MaxStep);
	}

	public GatewayToTrialDataContainer.ContainerInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GatewayToTrialDataContainer.ContainerInfo containerInfo = new GatewayToTrialDataContainer.ContainerInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				containerInfo.MinStep = reader.ReadInt32();
				break;
			case 1:
				containerInfo.MaxStep = reader.ReadInt32();
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
