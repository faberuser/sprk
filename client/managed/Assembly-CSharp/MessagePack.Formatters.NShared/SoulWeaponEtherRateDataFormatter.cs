using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponEtherRateDataFormatter : IMessagePackFormatter<SoulWeaponEtherRateData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponEtherRateData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.ReinforceLevel);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.AddedBonusRate);
	}

	public SoulWeaponEtherRateData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponEtherRateData soulWeaponEtherRateData = new SoulWeaponEtherRateData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponEtherRateData.ReinforceLevel = reader.ReadInt32();
				break;
			case 1:
				soulWeaponEtherRateData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				soulWeaponEtherRateData.AddedBonusRate = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponEtherRateData;
	}
}
