using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BossWarningData_ETypeFormatter : IMessagePackFormatter<BossWarningData.EType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BossWarningData.EType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public BossWarningData.EType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (BossWarningData.EType)reader.ReadInt32();
	}
}
