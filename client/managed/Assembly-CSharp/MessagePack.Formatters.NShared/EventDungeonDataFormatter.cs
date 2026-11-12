using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventDungeonDataFormatter : IMessagePackFormatter<EventDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(39);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DecorationSelectorName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PrefabName, options);
		resolver.GetFormatterWithVerify<EventDungeonPropType[]>().Serialize(ref writer, value.PropTypes, options);
		resolver.GetFormatterWithVerify<EEventDungeonCloseType>().Serialize(ref writer, value.CloseType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenType1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenTypeValues1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenType2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenTypeValues2, options);
		writer.Write(value.EventDungeonPointRate_Easy);
		writer.Write(value.EventDungeonPointAmount_Easy);
		writer.Write(value.EventDungeonPointRate_Normal);
		writer.Write(value.EventDungeonPointAmount_Normal);
		writer.Write(value.EventDungeonPointRate_Hard);
		writer.Write(value.EventDungeonPointAmount_Hard);
		writer.Write(value.EventDungeonPointRate_Hell);
		writer.Write(value.EventDungeonPointAmount_Hell);
		writer.Write(value.EventDungeonPoint2Rate_Easy);
		writer.Write(value.EventDungeonPoint2Amount_Easy);
		writer.Write(value.EventDungeonPoint2Rate_Normal);
		writer.Write(value.EventDungeonPoint2Amount_Normal);
		writer.Write(value.EventDungeonPoint2Rate_Hard);
		writer.Write(value.EventDungeonPoint2Amount_Hard);
		writer.Write(value.EventDungeonPoint2Rate_Hell);
		writer.Write(value.EventDungeonPoint2Amount_Hell);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonName, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ResetAmount, options);
		writer.Write(value.MaxResetCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ResetCurrencyType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.HelpTexts, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenedDungeonDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ClosedDungeonDesc, options);
		writer.Write(value.BanIndex);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonCardPrefix, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonCardName, options);
	}

	public EventDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventDungeonData eventDungeonData = new EventDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				eventDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				eventDungeonData.ShowIndex = reader.ReadInt32();
				break;
			case 3:
				eventDungeonData.DecorationSelectorName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				eventDungeonData.PrefabName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				eventDungeonData.PropTypes = resolver.GetFormatterWithVerify<EventDungeonPropType[]>().Deserialize(ref reader, options);
				break;
			case 6:
				eventDungeonData.CloseType = resolver.GetFormatterWithVerify<EEventDungeonCloseType>().Deserialize(ref reader, options);
				break;
			case 7:
				eventDungeonData.OpenType1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				eventDungeonData.OpenTypeValues1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				eventDungeonData.OpenType2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				eventDungeonData.OpenTypeValues2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 11:
				eventDungeonData.EventDungeonPointRate_Easy = reader.ReadInt32();
				break;
			case 12:
				eventDungeonData.EventDungeonPointAmount_Easy = reader.ReadInt32();
				break;
			case 13:
				eventDungeonData.EventDungeonPointRate_Normal = reader.ReadInt32();
				break;
			case 14:
				eventDungeonData.EventDungeonPointAmount_Normal = reader.ReadInt32();
				break;
			case 15:
				eventDungeonData.EventDungeonPointRate_Hard = reader.ReadInt32();
				break;
			case 16:
				eventDungeonData.EventDungeonPointAmount_Hard = reader.ReadInt32();
				break;
			case 17:
				eventDungeonData.EventDungeonPointRate_Hell = reader.ReadInt32();
				break;
			case 18:
				eventDungeonData.EventDungeonPointAmount_Hell = reader.ReadInt32();
				break;
			case 19:
				eventDungeonData.EventDungeonPoint2Rate_Easy = reader.ReadInt32();
				break;
			case 20:
				eventDungeonData.EventDungeonPoint2Amount_Easy = reader.ReadInt32();
				break;
			case 21:
				eventDungeonData.EventDungeonPoint2Rate_Normal = reader.ReadInt32();
				break;
			case 22:
				eventDungeonData.EventDungeonPoint2Amount_Normal = reader.ReadInt32();
				break;
			case 23:
				eventDungeonData.EventDungeonPoint2Rate_Hard = reader.ReadInt32();
				break;
			case 24:
				eventDungeonData.EventDungeonPoint2Amount_Hard = reader.ReadInt32();
				break;
			case 25:
				eventDungeonData.EventDungeonPoint2Rate_Hell = reader.ReadInt32();
				break;
			case 26:
				eventDungeonData.EventDungeonPoint2Amount_Hell = reader.ReadInt32();
				break;
			case 27:
				eventDungeonData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 28:
				eventDungeonData.DungeonName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 29:
				eventDungeonData.ResetAmount = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 30:
				eventDungeonData.MaxResetCount = reader.ReadInt32();
				break;
			case 31:
				eventDungeonData.ResetCurrencyType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 32:
				eventDungeonData.HelpTexts = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 33:
				eventDungeonData.OpenedDungeonDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 34:
				eventDungeonData.ClosedDungeonDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 35:
				eventDungeonData.BanIndex = reader.ReadInt32();
				break;
			case 36:
				eventDungeonData.RewardIndex = reader.ReadInt32();
				break;
			case 37:
				eventDungeonData.DungeonCardPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 38:
				eventDungeonData.DungeonCardName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventDungeonData;
	}
}
