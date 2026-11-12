using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DungeonHeroListSetDataFormatter : IMessagePackFormatter<DungeonHeroListSetData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DungeonHeroListSetData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.GroupIndex);
		writer.Write(value.HeroIndex);
		writer.Write(value.CostumeIndex);
	}

	public DungeonHeroListSetData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		DungeonHeroListSetData dungeonHeroListSetData = new DungeonHeroListSetData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dungeonHeroListSetData.GroupIndex = reader.ReadInt32();
				break;
			case 1:
				dungeonHeroListSetData.HeroIndex = reader.ReadInt32();
				break;
			case 2:
				dungeonHeroListSetData.CostumeIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return dungeonHeroListSetData;
	}
}
