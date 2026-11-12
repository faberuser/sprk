using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TeamLevelGradeFormatter : IMessagePackFormatter<TeamLevelGrade>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TeamLevelGrade value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public TeamLevelGrade Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (TeamLevelGrade)reader.ReadInt32();
	}
}
