using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GodkingTrialDungeonGateTypeFormatter : IMessagePackFormatter<GodkingTrialDungeonGateType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GodkingTrialDungeonGateType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GodkingTrialDungeonGateType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GodkingTrialDungeonGateType)reader.ReadInt32();
	}
}
