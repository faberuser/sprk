using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalRenewalDataKeyFormatter : IMessagePackFormatter<PortalRenewalDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalRenewalDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<PortalRenewalCategory>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<PortalRenewalSubCategory>().Serialize(ref writer, value.SubCategory, options);
	}

	public PortalRenewalDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PortalRenewalCategory category = PortalRenewalCategory.NONE;
		PortalRenewalSubCategory subCategory = PortalRenewalSubCategory.NONE;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				category = resolver.GetFormatterWithVerify<PortalRenewalCategory>().Deserialize(ref reader, options);
				break;
			case 1:
				subCategory = resolver.GetFormatterWithVerify<PortalRenewalSubCategory>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PortalRenewalDataKey result = new PortalRenewalDataKey(category, subCategory);
		reader.Depth--;
		return result;
	}
}
