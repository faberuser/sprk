using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TeamTypeFormatter : IMessagePackFormatter<TeamType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TeamType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public TeamType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (TeamType)reader.ReadInt32();
	}
}
