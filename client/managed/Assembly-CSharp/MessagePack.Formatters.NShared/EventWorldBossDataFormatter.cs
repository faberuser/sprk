using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventWorldBossDataFormatter : IMessagePackFormatter<EventWorldBossData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventWorldBossData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(18);
		writer.Write(value.Index);
		writer.Write(value.BossLevel);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		writer.Write(value.IsOpen);
		writer.Write(value.NextOpenMinute);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.MaxHp);
		writer.Write(value.ParticipantRewardIndex);
		writer.Write(value.KillRewardIndex);
		writer.Write(value.EndRewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewBg, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tactic, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendedHeroIndex, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossInfoImage, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ShowInfoSkill, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EventBossDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ButtonHighlightResource, options);
	}

	public EventWorldBossData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventWorldBossData eventWorldBossData = new EventWorldBossData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventWorldBossData.Index = reader.ReadInt32();
				break;
			case 1:
				eventWorldBossData.BossLevel = reader.ReadInt32();
				break;
			case 2:
				eventWorldBossData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				eventWorldBossData.IsOpen = reader.ReadBoolean();
				break;
			case 4:
				eventWorldBossData.NextOpenMinute = reader.ReadInt32();
				break;
			case 5:
				eventWorldBossData.ChapterIndex = reader.ReadInt32();
				break;
			case 6:
				eventWorldBossData.DungeonIndex = reader.ReadInt32();
				break;
			case 7:
				eventWorldBossData.MaxHp = reader.ReadInt32();
				break;
			case 8:
				eventWorldBossData.ParticipantRewardIndex = reader.ReadInt32();
				break;
			case 9:
				eventWorldBossData.KillRewardIndex = reader.ReadInt32();
				break;
			case 10:
				eventWorldBossData.EndRewardIndex = reader.ReadInt32();
				break;
			case 11:
				eventWorldBossData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				eventWorldBossData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				eventWorldBossData.RecommendedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 14:
				eventWorldBossData.BossInfoImage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				eventWorldBossData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 16:
				eventWorldBossData.EventBossDesc = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 17:
				eventWorldBossData.ButtonHighlightResource = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventWorldBossData;
	}
}
