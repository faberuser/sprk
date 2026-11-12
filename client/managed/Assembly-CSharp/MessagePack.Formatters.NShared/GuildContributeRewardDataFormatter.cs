using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildContributeRewardDataFormatter : IMessagePackFormatter<GuildContributeRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildContributeRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.ContributedGem);
		writer.Write(value.RewardGuildActivityPoint);
	}

	public GuildContributeRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GuildContributeRewardData guildContributeRewardData = new GuildContributeRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildContributeRewardData.ContributedGem = reader.ReadInt32();
				break;
			case 1:
				guildContributeRewardData.RewardGuildActivityPoint = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildContributeRewardData;
	}
}
