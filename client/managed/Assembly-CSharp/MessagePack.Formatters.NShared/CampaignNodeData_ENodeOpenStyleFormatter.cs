using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignNodeData_ENodeOpenStyleFormatter : IMessagePackFormatter<CampaignNodeData.ENodeOpenStyle>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignNodeData.ENodeOpenStyle value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CampaignNodeData.ENodeOpenStyle Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CampaignNodeData.ENodeOpenStyle)reader.ReadInt32();
	}
}
