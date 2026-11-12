using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaRewardDataFormatter : IMessagePackFormatter<KarmaRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.RewardIndex);
		writer.Write(value.IsAccumulate);
		writer.Write(value.DropChance);
	}

	public KarmaRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		KarmaRewardData karmaRewardData = new KarmaRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				karmaRewardData.PunishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				karmaRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 2:
				karmaRewardData.IsAccumulate = reader.ReadBoolean();
				break;
			case 3:
				karmaRewardData.DropChance = reader.ReadSingle();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return karmaRewardData;
	}
}
