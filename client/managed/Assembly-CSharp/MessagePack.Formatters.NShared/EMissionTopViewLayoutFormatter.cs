using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EMissionTopViewLayoutFormatter : IMessagePackFormatter<EMissionTopViewLayout>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EMissionTopViewLayout value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EMissionTopViewLayout Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EMissionTopViewLayout)reader.ReadInt32();
	}
}
