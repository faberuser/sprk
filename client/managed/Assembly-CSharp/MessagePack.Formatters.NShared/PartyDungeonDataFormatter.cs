using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PartyDungeonDataFormatter : IMessagePackFormatter<PartyDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PartyDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<PartyDungeonType>().Serialize(ref writer, value.DungeonType, options);
		resolver.GetFormatterWithVerify<PartyDungeonChapter>().Serialize(ref writer, value.Chapter, options);
		writer.Write(value.Easy);
		writer.Write(value.Normal);
		writer.Write(value.Hard);
		writer.Write(value.Hell);
		writer.Write(value.PlayerCount);
		writer.Write(value.DeckCount);
		writer.Write(value.DeckCountPerPlayer);
		writer.Write(value.IncreaseRatio);
		writer.Write(value.DecreaseRatio);
		writer.Write(value.AddtionalReward);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
	}

	public PartyDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PartyDungeonData partyDungeonData = new PartyDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				partyDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				partyDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				partyDungeonData.DungeonType = resolver.GetFormatterWithVerify<PartyDungeonType>().Deserialize(ref reader, options);
				break;
			case 3:
				partyDungeonData.Chapter = resolver.GetFormatterWithVerify<PartyDungeonChapter>().Deserialize(ref reader, options);
				break;
			case 4:
				partyDungeonData.Easy = reader.ReadBoolean();
				break;
			case 5:
				partyDungeonData.Normal = reader.ReadBoolean();
				break;
			case 6:
				partyDungeonData.Hard = reader.ReadBoolean();
				break;
			case 7:
				partyDungeonData.Hell = reader.ReadBoolean();
				break;
			case 8:
				partyDungeonData.PlayerCount = reader.ReadInt32();
				break;
			case 9:
				partyDungeonData.DeckCount = reader.ReadInt32();
				break;
			case 10:
				partyDungeonData.DeckCountPerPlayer = reader.ReadInt32();
				break;
			case 11:
				partyDungeonData.IncreaseRatio = reader.ReadInt32();
				break;
			case 12:
				partyDungeonData.DecreaseRatio = reader.ReadInt32();
				break;
			case 13:
				partyDungeonData.AddtionalReward = reader.ReadInt32();
				break;
			case 14:
				partyDungeonData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return partyDungeonData;
	}
}
