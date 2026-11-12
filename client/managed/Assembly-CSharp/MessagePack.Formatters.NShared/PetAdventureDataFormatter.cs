using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetAdventureDataFormatter : IMessagePackFormatter<PetAdventureData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetAdventureData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EPetAdventureType>().Serialize(ref writer, value.AdventureType, options);
		writer.Write(value.MaxPetCount);
		writer.Write(value.IsShow);
		writer.Write(value.Order);
		writer.Write(value.ReqFood);
		writer.Write(value.MaxDeckCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ImagePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ThumbnailImagePath, options);
	}

	public PetAdventureData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetAdventureData petAdventureData = new PetAdventureData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petAdventureData.Index = reader.ReadInt32();
				break;
			case 1:
				petAdventureData.AdventureType = resolver.GetFormatterWithVerify<EPetAdventureType>().Deserialize(ref reader, options);
				break;
			case 2:
				petAdventureData.MaxPetCount = reader.ReadInt32();
				break;
			case 3:
				petAdventureData.IsShow = reader.ReadBoolean();
				break;
			case 4:
				petAdventureData.Order = reader.ReadInt32();
				break;
			case 5:
				petAdventureData.ReqFood = reader.ReadInt32();
				break;
			case 6:
				petAdventureData.MaxDeckCount = reader.ReadInt32();
				break;
			case 7:
				petAdventureData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				petAdventureData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				petAdventureData.ImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				petAdventureData.ThumbnailImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petAdventureData;
	}
}
