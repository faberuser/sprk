using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PhaseStepRewardTypeFormatter : IMessagePackFormatter<PhaseStepRewardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PhaseStepRewardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PhaseStepRewardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PhaseStepRewardType)reader.ReadInt32();
	}
}
