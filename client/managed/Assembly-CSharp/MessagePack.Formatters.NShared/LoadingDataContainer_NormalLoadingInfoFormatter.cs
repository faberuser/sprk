using System.Collections.Generic;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LoadingDataContainer_NormalLoadingInfoFormatter : IMessagePackFormatter<LoadingDataContainer.NormalLoadingInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LoadingDataContainer.NormalLoadingInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.TotalRate);
		resolver.GetFormatterWithVerify<List<int>>().Serialize(ref writer, value.DataKeyList, options);
	}

	public LoadingDataContainer.NormalLoadingInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LoadingDataContainer.NormalLoadingInfo normalLoadingInfo = new LoadingDataContainer.NormalLoadingInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				normalLoadingInfo.TotalRate = reader.ReadInt32();
				break;
			case 1:
				normalLoadingInfo.DataKeyList = resolver.GetFormatterWithVerify<List<int>>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return normalLoadingInfo;
	}
}
