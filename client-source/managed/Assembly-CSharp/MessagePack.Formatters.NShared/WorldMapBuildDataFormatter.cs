using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldMapBuildDataFormatter : IMessagePackFormatter<WorldMapBuildData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldMapBuildData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.ChapterIndex);
		writer.Write(value.BuildingIndex);
		writer.Write(value.PositionX);
		writer.Write(value.PositionY);
		writer.Write(value.Scale);
		writer.Write(value.Angle);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ModelName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectorName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuildingType, options);
	}

	public WorldMapBuildData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WorldMapBuildData worldMapBuildData = new WorldMapBuildData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				worldMapBuildData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				worldMapBuildData.BuildingIndex = reader.ReadInt32();
				break;
			case 2:
				worldMapBuildData.PositionX = reader.ReadInt32();
				break;
			case 3:
				worldMapBuildData.PositionY = reader.ReadInt32();
				break;
			case 4:
				worldMapBuildData.Scale = reader.ReadInt32();
				break;
			case 5:
				worldMapBuildData.Angle = reader.ReadInt32();
				break;
			case 6:
				worldMapBuildData.ModelName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				worldMapBuildData.SelectorName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				worldMapBuildData.BuildingType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return worldMapBuildData;
	}
}
