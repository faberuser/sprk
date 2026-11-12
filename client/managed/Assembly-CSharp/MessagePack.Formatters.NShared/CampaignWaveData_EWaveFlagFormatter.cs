using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignWaveData_EWaveFlagFormatter : IMessagePackFormatter<CampaignWaveData.EWaveFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignWaveData.EWaveFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CampaignWaveData.EWaveFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CampaignWaveData.EWaveFlag)reader.ReadInt32();
	}
}
