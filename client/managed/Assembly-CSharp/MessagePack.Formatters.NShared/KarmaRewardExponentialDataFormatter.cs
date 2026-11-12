using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaRewardExponentialDataFormatter : IMessagePackFormatter<KarmaRewardExponentialData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaRewardExponentialData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.Level);
		writer.Write(value.RewardIndex);
		writer.Write(value.ExponentialValue);
	}

	public KarmaRewardExponentialData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		KarmaRewardExponentialData karmaRewardExponentialData = new KarmaRewardExponentialData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				karmaRewardExponentialData.PunishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				karmaRewardExponentialData.Level = reader.ReadInt32();
				break;
			case 2:
				karmaRewardExponentialData.RewardIndex = reader.ReadInt32();
				break;
			case 3:
				karmaRewardExponentialData.ExponentialValue = reader.ReadSingle();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return karmaRewardExponentialData;
	}
}
