using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PatchDownloadSpecDataFormatter : IMessagePackFormatter<PatchDownloadSpecData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PatchDownloadSpecData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<PatchSpecType>().Serialize(ref writer, value.Spec, options);
		resolver.GetFormatterWithVerify<PatchPlatform>().Serialize(ref writer, value.Platform, options);
		writer.Write(value.RamMin);
		writer.Write(value.RamMax);
		resolver.GetFormatterWithVerify<PatchSpecType>().Serialize(ref writer, value.StandardSpec, options);
		resolver.GetFormatterWithVerify<PatchSpecType[]>().Serialize(ref writer, value.SelectSpecInPatchDownload, options);
		resolver.GetFormatterWithVerify<PatchSpecType[]>().Serialize(ref writer, value.SelectSpecInGameOption, options);
	}

	public PatchDownloadSpecData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PatchDownloadSpecData patchDownloadSpecData = new PatchDownloadSpecData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				patchDownloadSpecData.Spec = resolver.GetFormatterWithVerify<PatchSpecType>().Deserialize(ref reader, options);
				break;
			case 1:
				patchDownloadSpecData.Platform = resolver.GetFormatterWithVerify<PatchPlatform>().Deserialize(ref reader, options);
				break;
			case 2:
				patchDownloadSpecData.RamMin = reader.ReadInt32();
				break;
			case 3:
				patchDownloadSpecData.RamMax = reader.ReadInt32();
				break;
			case 4:
				patchDownloadSpecData.StandardSpec = resolver.GetFormatterWithVerify<PatchSpecType>().Deserialize(ref reader, options);
				break;
			case 5:
				patchDownloadSpecData.SelectSpecInPatchDownload = resolver.GetFormatterWithVerify<PatchSpecType[]>().Deserialize(ref reader, options);
				break;
			case 6:
				patchDownloadSpecData.SelectSpecInGameOption = resolver.GetFormatterWithVerify<PatchSpecType[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return patchDownloadSpecData;
	}
}
