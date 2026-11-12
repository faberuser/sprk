using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetSelectItemDataFormatter : IMessagePackFormatter<PetSelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetSelectItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.PetStar);
		writer.Write(value.PetLevel);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PetIndices, options);
	}

	public PetSelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetSelectItemData petSelectItemData = new PetSelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petSelectItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				petSelectItemData.PetStar = reader.ReadInt32();
				break;
			case 2:
				petSelectItemData.PetLevel = reader.ReadInt32();
				break;
			case 3:
				petSelectItemData.PetIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petSelectItemData;
	}
}
