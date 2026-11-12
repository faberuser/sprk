using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CinemaItemDataFormatter : IMessagePackFormatter<CinemaItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CinemaItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PackCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AppearName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReactionName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReturnName, options);
	}

	public CinemaItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CinemaItemData cinemaItemData = new CinemaItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				cinemaItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				cinemaItemData.PackCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				cinemaItemData.AppearName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				cinemaItemData.ReactionName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				cinemaItemData.ReturnName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return cinemaItemData;
	}
}
