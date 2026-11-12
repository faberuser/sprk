using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EBattleMenuFormatter : IMessagePackFormatter<EBattleMenu>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EBattleMenu value, MessagePackSerializerOptions options)
	{
		writer.Write((ulong)value);
	}

	public EBattleMenu Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EBattleMenu)reader.ReadUInt64();
	}
}
