using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateDataFormatter : IMessagePackFormatter<StateData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(32);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Group, options);
		writer.Write(value.MaxStack);
		resolver.GetFormatterWithVerify<StateOverflowRuleType>().Serialize(ref writer, value.OverflowRule, options);
		resolver.GetFormatterWithVerify<StateStackDurationRuleType>().Serialize(ref writer, value.StackDurationRuleType, options);
		writer.Write(value.CanMove);
		writer.Write(value.CanPhysicalSkill);
		writer.Write(value.CanMagicalSkill);
		writer.Write(value.CanBaseAttack);
		writer.Write(value.CanBeTargetedByAlly);
		writer.Write(value.CanBeTargetedByEnemy);
		writer.Write(value.CanBePhysicalCc);
		writer.Write(value.CanBeMagicalCc);
		writer.Write(value.CanBePhysicalDamaged);
		writer.Write(value.CanBeMagicalDamaged);
		writer.Write(value.CanHitPhysicalAttack);
		writer.Write(value.CanHitMagicalAttack);
		writer.Write(value.CanPhysicalBlockAll);
		writer.Write(value.CanMagicalBlockAll);
		writer.Write(value.CanPhysicalDodgeAll);
		writer.Write(value.CanMagicalDodgeAll);
		writer.Write(value.IsPositive);
		writer.Write(value.IsNegative);
		writer.Write(value.IsSticky);
		writer.Write(value.CanUpdateCooltime);
		writer.Write(value.RejectReservedSkill);
		writer.Write(value.IsCc);
		writer.Write(value.CanPassiveSkill);
		resolver.GetFormatterWithVerify<CcMotionType>().Serialize(ref writer, value.CcMotionType, options);
		writer.Write(value.CcPower);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		resolver.GetFormatterWithVerify<StateFlag>().Serialize(ref writer, value.StateFlag, options);
	}

	public StateData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StateData stateData = new StateData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				stateData.StateType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				stateData.Group = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				stateData.MaxStack = reader.ReadInt32();
				break;
			case 3:
				stateData.OverflowRule = resolver.GetFormatterWithVerify<StateOverflowRuleType>().Deserialize(ref reader, options);
				break;
			case 4:
				stateData.StackDurationRuleType = resolver.GetFormatterWithVerify<StateStackDurationRuleType>().Deserialize(ref reader, options);
				break;
			case 5:
				stateData.CanMove = reader.ReadBoolean();
				break;
			case 6:
				stateData.CanPhysicalSkill = reader.ReadBoolean();
				break;
			case 7:
				stateData.CanMagicalSkill = reader.ReadBoolean();
				break;
			case 8:
				stateData.CanBaseAttack = reader.ReadBoolean();
				break;
			case 9:
				stateData.CanBeTargetedByAlly = reader.ReadBoolean();
				break;
			case 10:
				stateData.CanBeTargetedByEnemy = reader.ReadBoolean();
				break;
			case 11:
				stateData.CanBePhysicalCc = reader.ReadBoolean();
				break;
			case 12:
				stateData.CanBeMagicalCc = reader.ReadBoolean();
				break;
			case 13:
				stateData.CanBePhysicalDamaged = reader.ReadBoolean();
				break;
			case 14:
				stateData.CanBeMagicalDamaged = reader.ReadBoolean();
				break;
			case 15:
				stateData.CanHitPhysicalAttack = reader.ReadBoolean();
				break;
			case 16:
				stateData.CanHitMagicalAttack = reader.ReadBoolean();
				break;
			case 17:
				stateData.CanPhysicalBlockAll = reader.ReadBoolean();
				break;
			case 18:
				stateData.CanMagicalBlockAll = reader.ReadBoolean();
				break;
			case 19:
				stateData.CanPhysicalDodgeAll = reader.ReadBoolean();
				break;
			case 20:
				stateData.CanMagicalDodgeAll = reader.ReadBoolean();
				break;
			case 21:
				stateData.IsPositive = reader.ReadBoolean();
				break;
			case 22:
				stateData.IsNegative = reader.ReadBoolean();
				break;
			case 23:
				stateData.IsSticky = reader.ReadBoolean();
				break;
			case 24:
				stateData.CanUpdateCooltime = reader.ReadBoolean();
				break;
			case 25:
				stateData.RejectReservedSkill = reader.ReadBoolean();
				break;
			case 26:
				stateData.IsCc = reader.ReadBoolean();
				break;
			case 27:
				stateData.CanPassiveSkill = reader.ReadBoolean();
				break;
			case 28:
				stateData.CcMotionType = resolver.GetFormatterWithVerify<CcMotionType>().Deserialize(ref reader, options);
				break;
			case 29:
				stateData.CcPower = reader.ReadInt32();
				break;
			case 30:
				stateData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 31:
				stateData.StateFlag = resolver.GetFormatterWithVerify<StateFlag>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return stateData;
	}
}
