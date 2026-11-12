using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildPenaltyDataFormatter : IMessagePackFormatter<GuildPenaltyData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildPenaltyData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.WithdrawCount);
		writer.Write(value.RejoinTimeMin);
		writer.Write(value.RestrictTimeMin);
		writer.Write(value.ShowMinitue);
	}

	public GuildPenaltyData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GuildPenaltyData guildPenaltyData = new GuildPenaltyData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildPenaltyData.WithdrawCount = reader.ReadInt32();
				break;
			case 1:
				guildPenaltyData.RejoinTimeMin = reader.ReadInt32();
				break;
			case 2:
				guildPenaltyData.RestrictTimeMin = reader.ReadInt32();
				break;
			case 3:
				guildPenaltyData.ShowMinitue = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildPenaltyData;
	}
}
