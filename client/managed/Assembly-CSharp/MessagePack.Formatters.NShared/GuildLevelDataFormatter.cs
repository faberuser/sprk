using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildLevelDataFormatter : IMessagePackFormatter<GuildLevelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildLevelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(5);
		writer.Write(value.Level);
		writer.Write(value.ActivityPoint);
		writer.Write(value.TotalActivityPoint);
		writer.Write(value.MaxMember);
		writer.Write(value.MaxAdmin);
	}

	public GuildLevelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GuildLevelData guildLevelData = new GuildLevelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildLevelData.Level = reader.ReadInt32();
				break;
			case 1:
				guildLevelData.ActivityPoint = reader.ReadInt32();
				break;
			case 2:
				guildLevelData.TotalActivityPoint = reader.ReadInt32();
				break;
			case 3:
				guildLevelData.MaxMember = reader.ReadInt32();
				break;
			case 4:
				guildLevelData.MaxAdmin = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildLevelData;
	}
}
