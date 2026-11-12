using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetAwakenDataFormatter : IMessagePackFormatter<PetAwakenData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetAwakenData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.PetIndex);
		writer.Write(value.Star);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode1, options);
		writer.Write(value.ItemCount1);
		writer.Write(value.ReqGold);
	}

	public PetAwakenData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetAwakenData petAwakenData = new PetAwakenData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petAwakenData.PetIndex = reader.ReadInt32();
				break;
			case 1:
				petAwakenData.Star = reader.ReadInt32();
				break;
			case 2:
				petAwakenData.ItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				petAwakenData.ItemCount1 = reader.ReadInt32();
				break;
			case 4:
				petAwakenData.ReqGold = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petAwakenData;
	}
}
