using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetIncubatorDataFormatter : IMessagePackFormatter<PetIncubatorData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetIncubatorData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.Index);
		writer.Write(value.Grade);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescLocalKey, options);
		writer.Write(value.TimePer);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.BuyCurrencyType, options);
		writer.Write(value.BuyCurrencyValue);
		writer.Write(value.Mileage);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CreatureCode, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CreaturePosition, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CreatureRotation, options);
		writer.Write(value.CreatureScale);
	}

	public PetIncubatorData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetIncubatorData petIncubatorData = new PetIncubatorData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petIncubatorData.Index = reader.ReadInt32();
				break;
			case 1:
				petIncubatorData.Grade = reader.ReadInt32();
				break;
			case 2:
				petIncubatorData.NameLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				petIncubatorData.DescLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				petIncubatorData.TimePer = reader.ReadInt32();
				break;
			case 5:
				petIncubatorData.BuyCurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 6:
				petIncubatorData.BuyCurrencyValue = reader.ReadInt32();
				break;
			case 7:
				petIncubatorData.Mileage = reader.ReadInt32();
				break;
			case 8:
				petIncubatorData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				petIncubatorData.CreatureCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				petIncubatorData.CreaturePosition = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				petIncubatorData.CreatureRotation = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 13:
				petIncubatorData.CreatureScale = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petIncubatorData;
	}
}
