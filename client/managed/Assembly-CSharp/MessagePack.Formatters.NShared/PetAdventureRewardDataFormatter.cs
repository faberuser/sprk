using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetAdventureRewardDataFormatter : IMessagePackFormatter<PetAdventureRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetAdventureRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		writer.Write(value.RewardItemIndex);
		writer.Write(value.Time);
		writer.Write(value.PenaltyTime);
	}

	public PetAdventureRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		PetAdventureRewardData petAdventureRewardData = new PetAdventureRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petAdventureRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				petAdventureRewardData.RewardItemIndex = reader.ReadInt32();
				break;
			case 2:
				petAdventureRewardData.Time = reader.ReadInt32();
				break;
			case 3:
				petAdventureRewardData.PenaltyTime = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petAdventureRewardData;
	}
}
