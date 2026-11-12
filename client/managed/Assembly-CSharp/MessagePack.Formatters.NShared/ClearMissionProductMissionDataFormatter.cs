using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClearMissionProductMissionDataFormatter : IMessagePackFormatter<ClearMissionProductMissionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClearMissionProductMissionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		writer.Write(value.ProductGroupIndex);
		resolver.GetFormatterWithVerify<ClearMissionProvideType>().Serialize(ref writer, value.ProvideType, options);
		resolver.GetFormatterWithVerify<QuestActionType>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.StateValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MissionDesc, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MissionDescStateValue, options);
		writer.Write(value.ReqProgress);
		writer.Write(value.RewardIndex);
	}

	public ClearMissionProductMissionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ClearMissionProductMissionData clearMissionProductMissionData = new ClearMissionProductMissionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				clearMissionProductMissionData.Index = reader.ReadInt32();
				break;
			case 1:
				clearMissionProductMissionData.ProductGroupIndex = reader.ReadInt32();
				break;
			case 2:
				clearMissionProductMissionData.ProvideType = resolver.GetFormatterWithVerify<ClearMissionProvideType>().Deserialize(ref reader, options);
				break;
			case 3:
				clearMissionProductMissionData.StateType = resolver.GetFormatterWithVerify<QuestActionType>().Deserialize(ref reader, options);
				break;
			case 4:
				clearMissionProductMissionData.StateValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				clearMissionProductMissionData.MissionDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				clearMissionProductMissionData.MissionDescStateValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				clearMissionProductMissionData.ReqProgress = reader.ReadInt64();
				break;
			case 8:
				clearMissionProductMissionData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return clearMissionProductMissionData;
	}
}
