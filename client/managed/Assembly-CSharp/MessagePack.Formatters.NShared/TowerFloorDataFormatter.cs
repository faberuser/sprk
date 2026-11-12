using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TowerFloorDataFormatter : IMessagePackFormatter<TowerFloorData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TowerFloorData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(18);
		writer.Write(value.TowerIndex);
		writer.Write(value.Floor);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenTime, options);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<FloorDungeonSelectType>().Serialize(ref writer, value.DungeonSelectType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DungeonIndices, options);
		writer.Write(value.ReqTeamLevel);
		writer.Write(value.ReqChapterIndex);
		writer.Write(value.ReqDungeonIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqClearedChapterIndex, options);
		writer.Write(value.ReqClearedChapterDifficulty);
		writer.Write(value.ReqClass);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.BattleCostType, options);
		writer.Write(value.BattleStartCost);
		writer.Write(value.BattleEndCost);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RewardIndex, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RepeatRewardIndex, options);
	}

	public TowerFloorData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TowerFloorData towerFloorData = new TowerFloorData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				towerFloorData.TowerIndex = reader.ReadInt32();
				break;
			case 1:
				towerFloorData.Floor = reader.ReadInt32();
				break;
			case 2:
				towerFloorData.OpenTime = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				towerFloorData.ChapterIndex = reader.ReadInt32();
				break;
			case 4:
				towerFloorData.DungeonIndex = reader.ReadInt32();
				break;
			case 5:
				towerFloorData.DungeonSelectType = resolver.GetFormatterWithVerify<FloorDungeonSelectType>().Deserialize(ref reader, options);
				break;
			case 6:
				towerFloorData.DungeonIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 7:
				towerFloorData.ReqTeamLevel = reader.ReadInt32();
				break;
			case 8:
				towerFloorData.ReqChapterIndex = reader.ReadInt32();
				break;
			case 9:
				towerFloorData.ReqDungeonIndex = reader.ReadInt32();
				break;
			case 10:
				towerFloorData.ReqClearedChapterIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 11:
				towerFloorData.ReqClearedChapterDifficulty = reader.ReadInt32();
				break;
			case 12:
				towerFloorData.ReqClass = reader.ReadInt32();
				break;
			case 13:
				towerFloorData.BattleCostType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 14:
				towerFloorData.BattleStartCost = reader.ReadInt32();
				break;
			case 15:
				towerFloorData.BattleEndCost = reader.ReadInt32();
				break;
			case 16:
				towerFloorData.RewardIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 17:
				towerFloorData.RepeatRewardIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return towerFloorData;
	}
}
