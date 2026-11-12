using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemBreakDataFormatter : IMessagePackFormatter<EquipItemBreakData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemBreakData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(40);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode1, options);
		writer.Write(value.MinItemCount1);
		writer.Write(value.MaxItemCount1);
		writer.Write(value.ItemCount1_1);
		writer.Write(value.ItemRate1_1);
		writer.Write(value.ItemCount1_2);
		writer.Write(value.ItemRate1_2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode2, options);
		writer.Write(value.MinItemCount2);
		writer.Write(value.MaxItemCount2);
		writer.Write(value.ItemCount2_1);
		writer.Write(value.ItemRate2_1);
		writer.Write(value.ItemCount2_2);
		writer.Write(value.ItemRate2_2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode3, options);
		writer.Write(value.MinItemCount3);
		writer.Write(value.MaxItemCount3);
		writer.Write(value.ItemCount3_1);
		writer.Write(value.ItemRate3_1);
		writer.Write(value.ItemCount3_2);
		writer.Write(value.ItemRate3_2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode4, options);
		writer.Write(value.MinItemCount4);
		writer.Write(value.MaxItemCount4);
		writer.Write(value.ItemCount4_1);
		writer.Write(value.ItemRate4_1);
		writer.Write(value.ItemCount4_2);
		writer.Write(value.ItemRate4_2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode5, options);
		writer.Write(value.MinItemCount5);
		writer.Write(value.MaxItemCount5);
		writer.Write(value.ItemCount5_1);
		writer.Write(value.ItemRate5_1);
		writer.Write(value.ItemCount5_2);
		writer.Write(value.ItemRate5_2);
		writer.Write(value.SetIndex);
	}

	public EquipItemBreakData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipItemBreakData equipItemBreakData = new EquipItemBreakData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipItemBreakData.Tier = reader.ReadInt32();
				break;
			case 1:
				equipItemBreakData.DetailIndex = reader.ReadInt32();
				break;
			case 2:
				equipItemBreakData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				equipItemBreakData.Star = reader.ReadInt32();
				break;
			case 4:
				equipItemBreakData.ItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				equipItemBreakData.MinItemCount1 = reader.ReadInt32();
				break;
			case 6:
				equipItemBreakData.MaxItemCount1 = reader.ReadInt32();
				break;
			case 7:
				equipItemBreakData.ItemCount1_1 = reader.ReadInt32();
				break;
			case 8:
				equipItemBreakData.ItemRate1_1 = reader.ReadInt32();
				break;
			case 9:
				equipItemBreakData.ItemCount1_2 = reader.ReadInt32();
				break;
			case 10:
				equipItemBreakData.ItemRate1_2 = reader.ReadInt32();
				break;
			case 11:
				equipItemBreakData.ItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				equipItemBreakData.MinItemCount2 = reader.ReadInt32();
				break;
			case 13:
				equipItemBreakData.MaxItemCount2 = reader.ReadInt32();
				break;
			case 14:
				equipItemBreakData.ItemCount2_1 = reader.ReadInt32();
				break;
			case 15:
				equipItemBreakData.ItemRate2_1 = reader.ReadInt32();
				break;
			case 16:
				equipItemBreakData.ItemCount2_2 = reader.ReadInt32();
				break;
			case 17:
				equipItemBreakData.ItemRate2_2 = reader.ReadInt32();
				break;
			case 18:
				equipItemBreakData.ItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				equipItemBreakData.MinItemCount3 = reader.ReadInt32();
				break;
			case 20:
				equipItemBreakData.MaxItemCount3 = reader.ReadInt32();
				break;
			case 21:
				equipItemBreakData.ItemCount3_1 = reader.ReadInt32();
				break;
			case 22:
				equipItemBreakData.ItemRate3_1 = reader.ReadInt32();
				break;
			case 23:
				equipItemBreakData.ItemCount3_2 = reader.ReadInt32();
				break;
			case 24:
				equipItemBreakData.ItemRate3_2 = reader.ReadInt32();
				break;
			case 25:
				equipItemBreakData.ItemCode4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 26:
				equipItemBreakData.MinItemCount4 = reader.ReadInt32();
				break;
			case 27:
				equipItemBreakData.MaxItemCount4 = reader.ReadInt32();
				break;
			case 28:
				equipItemBreakData.ItemCount4_1 = reader.ReadInt32();
				break;
			case 29:
				equipItemBreakData.ItemRate4_1 = reader.ReadInt32();
				break;
			case 30:
				equipItemBreakData.ItemCount4_2 = reader.ReadInt32();
				break;
			case 31:
				equipItemBreakData.ItemRate4_2 = reader.ReadInt32();
				break;
			case 32:
				equipItemBreakData.ItemCode5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 33:
				equipItemBreakData.MinItemCount5 = reader.ReadInt32();
				break;
			case 34:
				equipItemBreakData.MaxItemCount5 = reader.ReadInt32();
				break;
			case 35:
				equipItemBreakData.ItemCount5_1 = reader.ReadInt32();
				break;
			case 36:
				equipItemBreakData.ItemRate5_1 = reader.ReadInt32();
				break;
			case 37:
				equipItemBreakData.ItemCount5_2 = reader.ReadInt32();
				break;
			case 38:
				equipItemBreakData.ItemRate5_2 = reader.ReadInt32();
				break;
			case 39:
				equipItemBreakData.SetIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipItemBreakData;
	}
}
