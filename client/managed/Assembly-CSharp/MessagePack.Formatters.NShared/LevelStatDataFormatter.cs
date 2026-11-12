using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LevelStatDataFormatter : IMessagePackFormatter<LevelStatData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LevelStatData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(5);
		writer.Write(value.PhysicalAttack);
		writer.Write(value.MagicalAttack);
		writer.Write(value.MaxHp);
		writer.Write(value.PhysicalDefense);
		writer.Write(value.MagicalDefense);
	}

	public LevelStatData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		LevelStatData levelStatData = new LevelStatData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				levelStatData.PhysicalAttack = reader.ReadInt32();
				break;
			case 1:
				levelStatData.MagicalAttack = reader.ReadInt32();
				break;
			case 2:
				levelStatData.MaxHp = reader.ReadInt64();
				break;
			case 3:
				levelStatData.PhysicalDefense = reader.ReadInt32();
				break;
			case 4:
				levelStatData.MagicalDefense = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return levelStatData;
	}
}
