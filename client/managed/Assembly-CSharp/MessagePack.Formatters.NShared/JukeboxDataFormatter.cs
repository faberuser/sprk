using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class JukeboxDataFormatter : IMessagePackFormatter<JukeboxData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, JukeboxData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Title, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubTitle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EffectCode, options);
	}

	public JukeboxData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		JukeboxData jukeboxData = new JukeboxData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				jukeboxData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				jukeboxData.Index = reader.ReadInt32();
				break;
			case 2:
				jukeboxData.Title = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				jukeboxData.SubTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				jukeboxData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				jukeboxData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				jukeboxData.EffectCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return jukeboxData;
	}
}
