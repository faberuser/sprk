using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AdjustTokenDataFormatter : IMessagePackFormatter<AdjustTokenData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AdjustTokenData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EventName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Token, options);
	}

	public AdjustTokenData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AdjustTokenData adjustTokenData = new AdjustTokenData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				adjustTokenData.Index = reader.ReadInt32();
				break;
			case 1:
				adjustTokenData.EventName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				adjustTokenData.Token = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return adjustTokenData;
	}
}
