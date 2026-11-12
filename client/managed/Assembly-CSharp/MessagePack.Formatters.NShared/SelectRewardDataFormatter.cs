using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SelectRewardDataFormatter : IMessagePackFormatter<SelectRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SelectRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(87);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.SelectCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item1Code, options);
		writer.Write(value.Item1Count);
		writer.Write(value.Item1Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item2Code, options);
		writer.Write(value.Item2Count);
		writer.Write(value.Item2Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item3Code, options);
		writer.Write(value.Item3Count);
		writer.Write(value.Item3Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item4Code, options);
		writer.Write(value.Item4Count);
		writer.Write(value.Item4Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item5Code, options);
		writer.Write(value.Item5Count);
		writer.Write(value.Item5Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item6Code, options);
		writer.Write(value.Item6Count);
		writer.Write(value.Item6Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item7Code, options);
		writer.Write(value.Item7Count);
		writer.Write(value.Item7Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item8Code, options);
		writer.Write(value.Item8Count);
		writer.Write(value.Item8Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item9Code, options);
		writer.Write(value.Item9Count);
		writer.Write(value.Item9Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item10Code, options);
		writer.Write(value.Item10Count);
		writer.Write(value.Item10Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item11Code, options);
		writer.Write(value.Item11Count);
		writer.Write(value.Item11Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item12Code, options);
		writer.Write(value.Item12Count);
		writer.Write(value.Item12Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item13Code, options);
		writer.Write(value.Item13Count);
		writer.Write(value.Item13Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item14Code, options);
		writer.Write(value.Item14Count);
		writer.Write(value.Item14Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item15Code, options);
		writer.Write(value.Item15Count);
		writer.Write(value.Item15Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item16Code, options);
		writer.Write(value.Item16Count);
		writer.Write(value.Item16Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item17Code, options);
		writer.Write(value.Item17Count);
		writer.Write(value.Item17Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item18Code, options);
		writer.Write(value.Item18Count);
		writer.Write(value.Item18Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item19Code, options);
		writer.Write(value.Item19Count);
		writer.Write(value.Item19Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item20Code, options);
		writer.Write(value.Item20Count);
		writer.Write(value.Item20Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item21Code, options);
		writer.Write(value.Item21Count);
		writer.Write(value.Item21Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item22Code, options);
		writer.Write(value.Item22Count);
		writer.Write(value.Item22Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item23Code, options);
		writer.Write(value.Item23Count);
		writer.Write(value.Item23Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item24Code, options);
		writer.Write(value.Item24Count);
		writer.Write(value.Item24Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item25Code, options);
		writer.Write(value.Item25Count);
		writer.Write(value.Item25Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item26Code, options);
		writer.Write(value.Item26Count);
		writer.Write(value.Item26Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item27Code, options);
		writer.Write(value.Item27Count);
		writer.Write(value.Item27Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Item28Code, options);
		writer.Write(value.Item28Count);
		writer.Write(value.Item28Star);
	}

	public SelectRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SelectRewardData selectRewardData = new SelectRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				selectRewardData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				selectRewardData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				selectRewardData.SelectCount = reader.ReadInt32();
				break;
			case 3:
				selectRewardData.Item1Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				selectRewardData.Item1Count = reader.ReadInt32();
				break;
			case 5:
				selectRewardData.Item1Star = reader.ReadInt32();
				break;
			case 6:
				selectRewardData.Item2Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				selectRewardData.Item2Count = reader.ReadInt32();
				break;
			case 8:
				selectRewardData.Item2Star = reader.ReadInt32();
				break;
			case 9:
				selectRewardData.Item3Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				selectRewardData.Item3Count = reader.ReadInt32();
				break;
			case 11:
				selectRewardData.Item3Star = reader.ReadInt32();
				break;
			case 12:
				selectRewardData.Item4Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				selectRewardData.Item4Count = reader.ReadInt32();
				break;
			case 14:
				selectRewardData.Item4Star = reader.ReadInt32();
				break;
			case 15:
				selectRewardData.Item5Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				selectRewardData.Item5Count = reader.ReadInt32();
				break;
			case 17:
				selectRewardData.Item5Star = reader.ReadInt32();
				break;
			case 18:
				selectRewardData.Item6Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				selectRewardData.Item6Count = reader.ReadInt32();
				break;
			case 20:
				selectRewardData.Item6Star = reader.ReadInt32();
				break;
			case 21:
				selectRewardData.Item7Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				selectRewardData.Item7Count = reader.ReadInt32();
				break;
			case 23:
				selectRewardData.Item7Star = reader.ReadInt32();
				break;
			case 24:
				selectRewardData.Item8Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 25:
				selectRewardData.Item8Count = reader.ReadInt32();
				break;
			case 26:
				selectRewardData.Item8Star = reader.ReadInt32();
				break;
			case 27:
				selectRewardData.Item9Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 28:
				selectRewardData.Item9Count = reader.ReadInt32();
				break;
			case 29:
				selectRewardData.Item9Star = reader.ReadInt32();
				break;
			case 30:
				selectRewardData.Item10Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 31:
				selectRewardData.Item10Count = reader.ReadInt32();
				break;
			case 32:
				selectRewardData.Item10Star = reader.ReadInt32();
				break;
			case 33:
				selectRewardData.Item11Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 34:
				selectRewardData.Item11Count = reader.ReadInt32();
				break;
			case 35:
				selectRewardData.Item11Star = reader.ReadInt32();
				break;
			case 36:
				selectRewardData.Item12Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 37:
				selectRewardData.Item12Count = reader.ReadInt32();
				break;
			case 38:
				selectRewardData.Item12Star = reader.ReadInt32();
				break;
			case 39:
				selectRewardData.Item13Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 40:
				selectRewardData.Item13Count = reader.ReadInt32();
				break;
			case 41:
				selectRewardData.Item13Star = reader.ReadInt32();
				break;
			case 42:
				selectRewardData.Item14Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 43:
				selectRewardData.Item14Count = reader.ReadInt32();
				break;
			case 44:
				selectRewardData.Item14Star = reader.ReadInt32();
				break;
			case 45:
				selectRewardData.Item15Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 46:
				selectRewardData.Item15Count = reader.ReadInt32();
				break;
			case 47:
				selectRewardData.Item15Star = reader.ReadInt32();
				break;
			case 48:
				selectRewardData.Item16Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 49:
				selectRewardData.Item16Count = reader.ReadInt32();
				break;
			case 50:
				selectRewardData.Item16Star = reader.ReadInt32();
				break;
			case 51:
				selectRewardData.Item17Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 52:
				selectRewardData.Item17Count = reader.ReadInt32();
				break;
			case 53:
				selectRewardData.Item17Star = reader.ReadInt32();
				break;
			case 54:
				selectRewardData.Item18Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 55:
				selectRewardData.Item18Count = reader.ReadInt32();
				break;
			case 56:
				selectRewardData.Item18Star = reader.ReadInt32();
				break;
			case 57:
				selectRewardData.Item19Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 58:
				selectRewardData.Item19Count = reader.ReadInt32();
				break;
			case 59:
				selectRewardData.Item19Star = reader.ReadInt32();
				break;
			case 60:
				selectRewardData.Item20Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 61:
				selectRewardData.Item20Count = reader.ReadInt32();
				break;
			case 62:
				selectRewardData.Item20Star = reader.ReadInt32();
				break;
			case 63:
				selectRewardData.Item21Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 64:
				selectRewardData.Item21Count = reader.ReadInt32();
				break;
			case 65:
				selectRewardData.Item21Star = reader.ReadInt32();
				break;
			case 66:
				selectRewardData.Item22Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 67:
				selectRewardData.Item22Count = reader.ReadInt32();
				break;
			case 68:
				selectRewardData.Item22Star = reader.ReadInt32();
				break;
			case 69:
				selectRewardData.Item23Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 70:
				selectRewardData.Item23Count = reader.ReadInt32();
				break;
			case 71:
				selectRewardData.Item23Star = reader.ReadInt32();
				break;
			case 72:
				selectRewardData.Item24Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 73:
				selectRewardData.Item24Count = reader.ReadInt32();
				break;
			case 74:
				selectRewardData.Item24Star = reader.ReadInt32();
				break;
			case 75:
				selectRewardData.Item25Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 76:
				selectRewardData.Item25Count = reader.ReadInt32();
				break;
			case 77:
				selectRewardData.Item25Star = reader.ReadInt32();
				break;
			case 78:
				selectRewardData.Item26Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 79:
				selectRewardData.Item26Count = reader.ReadInt32();
				break;
			case 80:
				selectRewardData.Item26Star = reader.ReadInt32();
				break;
			case 81:
				selectRewardData.Item27Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 82:
				selectRewardData.Item27Count = reader.ReadInt32();
				break;
			case 83:
				selectRewardData.Item27Star = reader.ReadInt32();
				break;
			case 84:
				selectRewardData.Item28Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 85:
				selectRewardData.Item28Count = reader.ReadInt32();
				break;
			case 86:
				selectRewardData.Item28Star = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return selectRewardData;
	}
}
