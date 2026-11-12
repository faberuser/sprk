using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRuneItemBreakDataFormatter : IMessagePackFormatter<PunishmentRuneItemBreakData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRuneItemBreakData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.BreakIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode1, options);
		writer.Write(value.MinItemCount1);
		writer.Write(value.MaxItemCount1);
		writer.Write(value.MinItemCount2);
		writer.Write(value.MaxItemCount2);
		writer.Write(value.MinItemCount3);
		writer.Write(value.MaxItemCount3);
		writer.Write(value.MinItemCount4);
		writer.Write(value.MaxItemCount4);
		writer.Write(value.MinItemCount5);
		writer.Write(value.MaxItemCount5);
	}

	public PunishmentRuneItemBreakData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentRuneItemBreakData punishmentRuneItemBreakData = new PunishmentRuneItemBreakData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentRuneItemBreakData.BreakIndex = reader.ReadInt32();
				break;
			case 1:
				punishmentRuneItemBreakData.ItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				punishmentRuneItemBreakData.MinItemCount1 = reader.ReadInt32();
				break;
			case 3:
				punishmentRuneItemBreakData.MaxItemCount1 = reader.ReadInt32();
				break;
			case 4:
				punishmentRuneItemBreakData.MinItemCount2 = reader.ReadInt32();
				break;
			case 5:
				punishmentRuneItemBreakData.MaxItemCount2 = reader.ReadInt32();
				break;
			case 6:
				punishmentRuneItemBreakData.MinItemCount3 = reader.ReadInt32();
				break;
			case 7:
				punishmentRuneItemBreakData.MaxItemCount3 = reader.ReadInt32();
				break;
			case 8:
				punishmentRuneItemBreakData.MinItemCount4 = reader.ReadInt32();
				break;
			case 9:
				punishmentRuneItemBreakData.MaxItemCount4 = reader.ReadInt32();
				break;
			case 10:
				punishmentRuneItemBreakData.MinItemCount5 = reader.ReadInt32();
				break;
			case 11:
				punishmentRuneItemBreakData.MaxItemCount5 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentRuneItemBreakData;
	}
}
