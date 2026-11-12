using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildAttendanceRewardDataFormatter : IMessagePackFormatter<GuildAttendanceRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildAttendanceRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		writer.Write(value.Day);
		writer.Write(value.Step);
		writer.Write(value.RewardIndex);
	}

	public GuildAttendanceRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GuildAttendanceRewardData guildAttendanceRewardData = new GuildAttendanceRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildAttendanceRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				guildAttendanceRewardData.Day = reader.ReadInt32();
				break;
			case 2:
				guildAttendanceRewardData.Step = reader.ReadInt32();
				break;
			case 3:
				guildAttendanceRewardData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildAttendanceRewardData;
	}
}
