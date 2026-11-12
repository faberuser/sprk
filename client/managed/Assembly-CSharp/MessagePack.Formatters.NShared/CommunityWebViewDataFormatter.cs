using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CommunityWebViewDataFormatter : IMessagePackFormatter<CommunityWebViewData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CommunityWebViewData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Language, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Url, options);
		writer.Write(value.IsDefault);
	}

	public CommunityWebViewData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CommunityWebViewData communityWebViewData = new CommunityWebViewData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				communityWebViewData.Index = reader.ReadInt32();
				break;
			case 1:
				communityWebViewData.Language = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				communityWebViewData.IconName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				communityWebViewData.Url = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				communityWebViewData.IsDefault = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return communityWebViewData;
	}
}
