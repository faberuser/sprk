using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassSeasonMissionDataFormatter : IMessagePackFormatter<KingsPassSeasonMissionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassSeasonMissionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.SeasonIndex);
		writer.Write(value.MissionIndex);
		writer.Write(value.Step);
		resolver.GetFormatterWithVerify<KingsPassType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.Level);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AdditionalRewardPriceType, options);
		writer.Write(value.AdditionalRewardPrice);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.StateValue, options);
		writer.Write(value.ReqProgress);
		writer.Write(value.AutoComplete);
	}

	public KingsPassSeasonMissionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		KingsPassSeasonMissionData kingsPassSeasonMissionData = new KingsPassSeasonMissionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				kingsPassSeasonMissionData.SeasonIndex = reader.ReadInt32();
				break;
			case 1:
				kingsPassSeasonMissionData.MissionIndex = reader.ReadInt32();
				break;
			case 2:
				kingsPassSeasonMissionData.Step = reader.ReadInt32();
				break;
			case 3:
				kingsPassSeasonMissionData.Type = resolver.GetFormatterWithVerify<KingsPassType>().Deserialize(ref reader, options);
				break;
			case 4:
				kingsPassSeasonMissionData.Level = reader.ReadInt32();
				break;
			case 5:
				kingsPassSeasonMissionData.AdditionalRewardPriceType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				kingsPassSeasonMissionData.AdditionalRewardPrice = reader.ReadInt32();
				break;
			case 7:
				kingsPassSeasonMissionData.RewardIndex = reader.ReadInt32();
				break;
			case 8:
				kingsPassSeasonMissionData.StateType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				kingsPassSeasonMissionData.StateValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				kingsPassSeasonMissionData.ReqProgress = reader.ReadInt32();
				break;
			case 11:
				kingsPassSeasonMissionData.AutoComplete = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return kingsPassSeasonMissionData;
	}
}
