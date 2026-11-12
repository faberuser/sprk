using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponNodeTreeDataFormatter : IMessagePackFormatter<SoulWeaponNodeTreeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponNodeTreeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(22);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.SoulStoneItemIndex);
		writer.Write(value.WeaponCreatureIndex);
		writer.Write(value.SoulStoneCreatureIndex);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.ManageWeaponPosition, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.ManageWeaponRotation, options);
		writer.Write(value.ManageWeaponScale);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.UpgradeWeaponPosition, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.UpgradeWeaponRotation, options);
		writer.Write(value.UpgradeWeaponScale);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.UpgradeResultWeaponPosition, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.UpgradeResultWeaponRotation, options);
		writer.Write(value.UpgradeResultWeaponScale);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.LiberationBackGround, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.LiberationWait, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.LiberationAction, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Liberation, options);
		resolver.GetFormatterWithVerify<ESoulweaponLiberationResultSide>().Serialize(ref writer, value.LiberationResultSide, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ModelView, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ReinforceSuccess, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ReinforceFail, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Restore, options);
	}

	public SoulWeaponNodeTreeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponNodeTreeData soulWeaponNodeTreeData = new SoulWeaponNodeTreeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponNodeTreeData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				soulWeaponNodeTreeData.SoulStoneItemIndex = reader.ReadInt32();
				break;
			case 2:
				soulWeaponNodeTreeData.WeaponCreatureIndex = reader.ReadInt32();
				break;
			case 3:
				soulWeaponNodeTreeData.SoulStoneCreatureIndex = reader.ReadInt32();
				break;
			case 4:
				soulWeaponNodeTreeData.ManageWeaponPosition = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 5:
				soulWeaponNodeTreeData.ManageWeaponRotation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 6:
				soulWeaponNodeTreeData.ManageWeaponScale = reader.ReadSingle();
				break;
			case 7:
				soulWeaponNodeTreeData.UpgradeWeaponPosition = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 8:
				soulWeaponNodeTreeData.UpgradeWeaponRotation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 9:
				soulWeaponNodeTreeData.UpgradeWeaponScale = reader.ReadSingle();
				break;
			case 10:
				soulWeaponNodeTreeData.UpgradeResultWeaponPosition = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 11:
				soulWeaponNodeTreeData.UpgradeResultWeaponRotation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 12:
				soulWeaponNodeTreeData.UpgradeResultWeaponScale = reader.ReadSingle();
				break;
			case 13:
				soulWeaponNodeTreeData.LiberationBackGround = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				soulWeaponNodeTreeData.LiberationWait = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 15:
				soulWeaponNodeTreeData.LiberationAction = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 16:
				soulWeaponNodeTreeData.Liberation = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 17:
				soulWeaponNodeTreeData.LiberationResultSide = resolver.GetFormatterWithVerify<ESoulweaponLiberationResultSide>().Deserialize(ref reader, options);
				break;
			case 18:
				soulWeaponNodeTreeData.ModelView = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 19:
				soulWeaponNodeTreeData.ReinforceSuccess = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 20:
				soulWeaponNodeTreeData.ReinforceFail = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 21:
				soulWeaponNodeTreeData.Restore = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponNodeTreeData;
	}
}
