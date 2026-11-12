using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AttackAttrTypeFormatter : IMessagePackFormatter<AttackAttrType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AttackAttrType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AttackAttrType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AttackAttrType)reader.ReadInt32();
	}
}
