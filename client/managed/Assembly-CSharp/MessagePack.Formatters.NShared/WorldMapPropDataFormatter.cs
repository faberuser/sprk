using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldMapPropDataFormatter : IMessagePackFormatter<WorldMapPropData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldMapPropData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.ChapterIndex);
		writer.Write(value.PositionX);
		writer.Write(value.PositionY);
		resolver.GetFormatterWithVerify<WorldMapPropType>().Serialize(ref writer, value.PropType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PropName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.PropAction, options);
		writer.Write(value.Scale);
		writer.Write(value.Angle);
		resolver.GetFormatterWithVerify<WorldMapPropConditionType>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionArgs, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LayerName, options);
	}

	public WorldMapPropData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WorldMapPropData worldMapPropData = new WorldMapPropData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				worldMapPropData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				worldMapPropData.PositionX = reader.ReadInt32();
				break;
			case 2:
				worldMapPropData.PositionY = reader.ReadInt32();
				break;
			case 3:
				worldMapPropData.PropType = resolver.GetFormatterWithVerify<WorldMapPropType>().Deserialize(ref reader, options);
				break;
			case 4:
				worldMapPropData.PropName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				worldMapPropData.PropAction = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				worldMapPropData.Scale = reader.ReadInt32();
				break;
			case 7:
				worldMapPropData.Angle = reader.ReadInt32();
				break;
			case 8:
				worldMapPropData.ConditionType = resolver.GetFormatterWithVerify<WorldMapPropConditionType>().Deserialize(ref reader, options);
				break;
			case 9:
				worldMapPropData.ConditionArgs = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				worldMapPropData.LayerName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return worldMapPropData;
	}
}
