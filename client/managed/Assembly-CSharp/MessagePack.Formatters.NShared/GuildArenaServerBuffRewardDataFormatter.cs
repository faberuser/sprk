using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildArenaServerBuffRewardDataFormatter : IMessagePackFormatter<GuildArenaServerBuffRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildArenaServerBuffRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.ItemIndex);
	}

	public GuildArenaServerBuffRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GuildArenaServerBuffRewardData guildArenaServerBuffRewardData = new GuildArenaServerBuffRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildArenaServerBuffRewardData.MinRank = reader.ReadInt32();
				break;
			case 1:
				guildArenaServerBuffRewardData.MaxRank = reader.ReadInt32();
				break;
			case 2:
				guildArenaServerBuffRewardData.ItemIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildArenaServerBuffRewardData;
	}
}
