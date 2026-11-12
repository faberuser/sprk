using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TowerDataFormatter : IMessagePackFormatter<TowerData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TowerData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		writer.Write(value.TowerIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<TowerType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.FloorCountPerPage);
		writer.Write(value.DailyReset);
		writer.Write(value.WeeklyReset);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.MonthlyResetDate, options);
		writer.Write(value.RepeatableFloor);
		writer.Write(value.UseNpc);
		writer.Write(value.MaxResetCount);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.ResetCostType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ResetCost, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CompleteRewardIndex, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TowerImage, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkyImage, options);
	}

	public TowerData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TowerData towerData = new TowerData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				towerData.TowerIndex = reader.ReadInt32();
				break;
			case 1:
				towerData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				towerData.Type = resolver.GetFormatterWithVerify<TowerType>().Deserialize(ref reader, options);
				break;
			case 3:
				towerData.FloorCountPerPage = reader.ReadInt32();
				break;
			case 4:
				towerData.DailyReset = reader.ReadBoolean();
				break;
			case 5:
				towerData.WeeklyReset = reader.ReadBoolean();
				break;
			case 6:
				towerData.MonthlyResetDate = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 7:
				towerData.RepeatableFloor = reader.ReadBoolean();
				break;
			case 8:
				towerData.UseNpc = reader.ReadBoolean();
				break;
			case 9:
				towerData.MaxResetCount = reader.ReadInt32();
				break;
			case 10:
				towerData.ResetCostType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 11:
				towerData.ResetCost = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				towerData.CompleteRewardIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 13:
				towerData.TowerImage = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				towerData.SkyImage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return towerData;
	}
}
