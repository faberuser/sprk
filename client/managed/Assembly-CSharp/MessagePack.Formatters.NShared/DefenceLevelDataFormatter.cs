using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DefenceLevelDataFormatter : IMessagePackFormatter<DefenceLevelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DefenceLevelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.Level);
		writer.Write(value.MaxDefence);
	}

	public DefenceLevelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		DefenceLevelData defenceLevelData = new DefenceLevelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				defenceLevelData.Level = reader.ReadInt32();
				break;
			case 1:
				defenceLevelData.MaxDefence = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return defenceLevelData;
	}
}
