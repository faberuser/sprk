using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignNodeData_EMenuOpenConditionFormatter : IMessagePackFormatter<CampaignNodeData.EMenuOpenCondition>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignNodeData.EMenuOpenCondition value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CampaignNodeData.EMenuOpenCondition Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CampaignNodeData.EMenuOpenCondition)reader.ReadInt32();
	}
}
