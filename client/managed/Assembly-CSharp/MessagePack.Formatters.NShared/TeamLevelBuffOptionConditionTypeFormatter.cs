using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TeamLevelBuffOptionConditionTypeFormatter : IMessagePackFormatter<TeamLevelBuffOptionConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TeamLevelBuffOptionConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public TeamLevelBuffOptionConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (TeamLevelBuffOptionConditionType)reader.ReadInt32();
	}
}
