using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetTierInteractiveDataFormatter : IMessagePackFormatter<PetTierInteractiveData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetTierInteractiveData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(10);
		writer.Write(value.Grade);
		writer.Write(value.SleepCoolTimeSec);
		writer.Write(value.RewardIndex);
		writer.Write(value.PetFeedCoolTimeSec);
		writer.Write(value.PetPatCoolTimeSec);
		writer.Write(value.PetPlayCoolTimeSec);
		writer.Write(value.PetFeedAddHappinessPoint);
		writer.Write(value.PetPatAddHappinessPoint);
		writer.Write(value.PetPlayAddHappinesPoint);
		writer.Write(value.PetMaxHappinessPoint);
	}

	public PetTierInteractiveData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		PetTierInteractiveData petTierInteractiveData = new PetTierInteractiveData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petTierInteractiveData.Grade = reader.ReadInt32();
				break;
			case 1:
				petTierInteractiveData.SleepCoolTimeSec = reader.ReadInt32();
				break;
			case 2:
				petTierInteractiveData.RewardIndex = reader.ReadInt32();
				break;
			case 3:
				petTierInteractiveData.PetFeedCoolTimeSec = reader.ReadInt32();
				break;
			case 4:
				petTierInteractiveData.PetPatCoolTimeSec = reader.ReadInt32();
				break;
			case 5:
				petTierInteractiveData.PetPlayCoolTimeSec = reader.ReadInt32();
				break;
			case 6:
				petTierInteractiveData.PetFeedAddHappinessPoint = reader.ReadInt32();
				break;
			case 7:
				petTierInteractiveData.PetPatAddHappinessPoint = reader.ReadInt32();
				break;
			case 8:
				petTierInteractiveData.PetPlayAddHappinesPoint = reader.ReadInt32();
				break;
			case 9:
				petTierInteractiveData.PetMaxHappinessPoint = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petTierInteractiveData;
	}
}
