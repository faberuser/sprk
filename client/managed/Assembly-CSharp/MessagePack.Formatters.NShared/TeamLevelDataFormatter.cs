using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TeamLevelDataFormatter : IMessagePackFormatter<TeamLevelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TeamLevelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(11);
		writer.Write(value.Level);
		writer.Write(value.LocalExp);
		writer.Write(value.MaxStamina);
		writer.Write(value.RechargeStaminaSec);
		writer.Write(value.DailyBonusStamina);
		writer.Write(value.ExpBonusPercentage);
		writer.Write(value.GoldBonusPercentage);
		writer.Write(value.DailyGold);
		writer.Write(value.TeamLevelBuffOptionIndex);
		writer.Write(value.TeamLevelBuffBonusOptionIndex);
		writer.Write(value.TotalExp);
	}

	public TeamLevelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		TeamLevelData teamLevelData = new TeamLevelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				teamLevelData.Level = reader.ReadInt32();
				break;
			case 1:
				teamLevelData.LocalExp = reader.ReadInt64();
				break;
			case 2:
				teamLevelData.MaxStamina = reader.ReadInt32();
				break;
			case 3:
				teamLevelData.RechargeStaminaSec = reader.ReadInt32();
				break;
			case 4:
				teamLevelData.DailyBonusStamina = reader.ReadInt32();
				break;
			case 5:
				teamLevelData.ExpBonusPercentage = reader.ReadInt32();
				break;
			case 6:
				teamLevelData.GoldBonusPercentage = reader.ReadInt32();
				break;
			case 7:
				teamLevelData.DailyGold = reader.ReadInt32();
				break;
			case 8:
				teamLevelData.TeamLevelBuffOptionIndex = reader.ReadInt32();
				break;
			case 9:
				teamLevelData.TeamLevelBuffBonusOptionIndex = reader.ReadInt32();
				break;
			case 10:
				teamLevelData.TotalExp = reader.ReadInt64();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return teamLevelData;
	}
}
