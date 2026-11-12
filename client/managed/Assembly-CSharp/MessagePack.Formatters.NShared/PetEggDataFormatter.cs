using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetEggDataFormatter : IMessagePackFormatter<PetEggData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetEggData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		writer.Write(value.ReqTime);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupCode, options);
	}

	public PetEggData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetEggData petEggData = new PetEggData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petEggData.Index = reader.ReadInt32();
				break;
			case 1:
				petEggData.ReqTime = reader.ReadInt32();
				break;
			case 2:
				petEggData.ItemGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petEggData;
	}
}
