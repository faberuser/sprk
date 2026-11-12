using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EMissionTopViewButtonGroupFormatter : IMessagePackFormatter<EMissionTopViewButtonGroup>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EMissionTopViewButtonGroup value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EMissionTopViewButtonGroup Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EMissionTopViewButtonGroup)reader.ReadInt32();
	}
}
