using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PhaseStepReqTypeFormatter : IMessagePackFormatter<PhaseStepReqType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PhaseStepReqType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PhaseStepReqType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PhaseStepReqType)reader.ReadInt32();
	}
}
