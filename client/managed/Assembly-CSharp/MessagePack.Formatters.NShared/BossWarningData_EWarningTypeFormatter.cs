using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BossWarningData_EWarningTypeFormatter : IMessagePackFormatter<BossWarningData.EWarningType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BossWarningData.EWarningType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public BossWarningData.EWarningType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (BossWarningData.EWarningType)reader.ReadInt32();
	}
}
