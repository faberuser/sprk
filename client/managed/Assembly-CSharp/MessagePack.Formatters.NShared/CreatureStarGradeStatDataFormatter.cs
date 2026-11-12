using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureStarGradeStatDataFormatter : IMessagePackFormatter<CreatureStarGradeStatData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureStarGradeStatData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(5);
		writer.Write(value.CreatureIndex);
		writer.Write(value.Star);
		writer.Write(value.Transcended);
		writer.Write(value.LevelStatFactor);
		writer.Write(value.LookupTableIndex);
	}

	public CreatureStarGradeStatData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureStarGradeStatData creatureStarGradeStatData = new CreatureStarGradeStatData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureStarGradeStatData.CreatureIndex = reader.ReadInt32();
				break;
			case 1:
				creatureStarGradeStatData.Star = reader.ReadByte();
				break;
			case 2:
				creatureStarGradeStatData.Transcended = reader.ReadByte();
				break;
			case 3:
				creatureStarGradeStatData.LevelStatFactor = reader.ReadInt32();
				break;
			case 4:
				creatureStarGradeStatData.LookupTableIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureStarGradeStatData;
	}
}
