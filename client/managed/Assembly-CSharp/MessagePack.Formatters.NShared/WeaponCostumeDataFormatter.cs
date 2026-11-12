using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WeaponCostumeDataFormatter : IMessagePackFormatter<WeaponCostumeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WeaponCostumeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(19);
		writer.Write(value.Index);
		writer.Write(value.HeroIndex);
		writer.Write(value.Star);
		writer.WriteNil();
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WeaponPrefix, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.NeedCostume, options);
		writer.Write(value.IsOpen);
		writer.Write(value.IsDefault);
		writer.Write(value.PreviewableWhenNeedCostumeOwned);
		writer.Write(value.PreviewableWhenNeedWeaponOwned);
		writer.Write(value.IsNew);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Group, options);
		writer.Write(value.ReqBuyGem);
		writer.Write(value.Mileage);
		writer.Write(value.IsShow);
	}

	public WeaponCostumeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WeaponCostumeData weaponCostumeData = new WeaponCostumeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				weaponCostumeData.Index = reader.ReadInt32();
				break;
			case 1:
				weaponCostumeData.HeroIndex = reader.ReadInt32();
				break;
			case 2:
				weaponCostumeData.Star = reader.ReadInt32();
				break;
			case 5:
				weaponCostumeData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				weaponCostumeData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				weaponCostumeData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				weaponCostumeData.WeaponPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				weaponCostumeData.NeedCostume = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				weaponCostumeData.IsOpen = reader.ReadBoolean();
				break;
			case 11:
				weaponCostumeData.IsDefault = reader.ReadBoolean();
				break;
			case 12:
				weaponCostumeData.PreviewableWhenNeedCostumeOwned = reader.ReadBoolean();
				break;
			case 13:
				weaponCostumeData.PreviewableWhenNeedWeaponOwned = reader.ReadBoolean();
				break;
			case 14:
				weaponCostumeData.IsNew = reader.ReadBoolean();
				break;
			case 15:
				weaponCostumeData.Group = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				weaponCostumeData.ReqBuyGem = reader.ReadInt32();
				break;
			case 17:
				weaponCostumeData.Mileage = reader.ReadInt32();
				break;
			case 18:
				weaponCostumeData.IsShow = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return weaponCostumeData;
	}
}
