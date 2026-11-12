using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CinemaWindowDataFormatter : IMessagePackFormatter<CinemaWindowData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CinemaWindowData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		writer.Write(value.IsOpenPlayOnce);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenNodeTreePack, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenNodeTreeName, options);
		writer.Write(value.IsClosePlayOnce);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CloseNodeTreePack, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CloseNodeTreeName, options);
		writer.Write(value.IsOwnerWindow);
	}

	public CinemaWindowData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CinemaWindowData cinemaWindowData = new CinemaWindowData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				cinemaWindowData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				cinemaWindowData.IsOpenPlayOnce = reader.ReadBoolean();
				break;
			case 2:
				cinemaWindowData.OpenNodeTreePack = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				cinemaWindowData.OpenNodeTreeName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				cinemaWindowData.IsClosePlayOnce = reader.ReadBoolean();
				break;
			case 5:
				cinemaWindowData.CloseNodeTreePack = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				cinemaWindowData.CloseNodeTreeName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				cinemaWindowData.IsOwnerWindow = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return cinemaWindowData;
	}
}
