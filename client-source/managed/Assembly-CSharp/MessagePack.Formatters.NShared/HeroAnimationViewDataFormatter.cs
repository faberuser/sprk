using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroAnimationViewDataFormatter : IMessagePackFormatter<HeroAnimationViewData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroAnimationViewData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Animation, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AnimationText, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.VoiceCode, options);
		writer.Write(value.SkipLegendCostume);
	}

	public HeroAnimationViewData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroAnimationViewData heroAnimationViewData = new HeroAnimationViewData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroAnimationViewData.Index = reader.ReadInt32();
				break;
			case 1:
				heroAnimationViewData.HeroIndex = reader.ReadInt32();
				break;
			case 2:
				heroAnimationViewData.Animation = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				heroAnimationViewData.AnimationText = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				heroAnimationViewData.IconType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				heroAnimationViewData.VoiceCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				heroAnimationViewData.SkipLegendCostume = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroAnimationViewData;
	}
}
