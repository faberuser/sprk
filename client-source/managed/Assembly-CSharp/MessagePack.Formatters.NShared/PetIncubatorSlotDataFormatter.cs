using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetIncubatorSlotDataFormatter : IMessagePackFormatter<PetIncubatorSlotData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetIncubatorSlotData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.BuyCurrencyType, options);
		writer.Write(value.BuyCurrencyValue);
		writer.Write(value.OpenNum);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.Position, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.Rotation, options);
		writer.Write(value.DefaultIndex);
		writer.Write(value.Mileage);
	}

	public PetIncubatorSlotData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetIncubatorSlotData petIncubatorSlotData = new PetIncubatorSlotData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petIncubatorSlotData.Index = reader.ReadInt32();
				break;
			case 1:
				petIncubatorSlotData.BuyCurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 2:
				petIncubatorSlotData.BuyCurrencyValue = reader.ReadInt32();
				break;
			case 3:
				petIncubatorSlotData.OpenNum = reader.ReadInt32();
				break;
			case 4:
				petIncubatorSlotData.Position = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 5:
				petIncubatorSlotData.Rotation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 6:
				petIncubatorSlotData.DefaultIndex = reader.ReadInt32();
				break;
			case 7:
				petIncubatorSlotData.Mileage = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petIncubatorSlotData;
	}
}
