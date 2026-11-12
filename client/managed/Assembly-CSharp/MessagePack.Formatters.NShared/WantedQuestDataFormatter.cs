using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WantedQuestDataFormatter : IMessagePackFormatter<WantedQuestData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WantedQuestData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CreatureCode, options);
		writer.Write(value.BattleChapterIndex);
		writer.Write(value.BattleDungeonIndex);
		writer.Write(value.RoyalPoint);
		writer.Write(value.Stamina);
		writer.Write(value.SpeedFactor);
		writer.Write(value.PathIndex);
	}

	public WantedQuestData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WantedQuestData wantedQuestData = new WantedQuestData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				wantedQuestData.Index = reader.ReadInt32();
				break;
			case 1:
				wantedQuestData.CreatureCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				wantedQuestData.BattleChapterIndex = reader.ReadInt32();
				break;
			case 3:
				wantedQuestData.BattleDungeonIndex = reader.ReadInt32();
				break;
			case 4:
				wantedQuestData.RoyalPoint = reader.ReadInt32();
				break;
			case 5:
				wantedQuestData.Stamina = reader.ReadInt32();
				break;
			case 6:
				wantedQuestData.SpeedFactor = reader.ReadSingle();
				break;
			case 7:
				wantedQuestData.PathIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return wantedQuestData;
	}
}
