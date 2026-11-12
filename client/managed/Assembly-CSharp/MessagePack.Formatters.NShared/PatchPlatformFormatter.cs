using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PatchPlatformFormatter : IMessagePackFormatter<PatchPlatform>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PatchPlatform value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PatchPlatform Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PatchPlatform)reader.ReadInt32();
	}
}
