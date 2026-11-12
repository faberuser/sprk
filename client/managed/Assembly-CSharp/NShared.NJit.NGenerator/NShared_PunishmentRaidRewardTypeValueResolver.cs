using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PunishmentRaidRewardTypeValueResolver : IDataResolverT<PunishmentRaidRewardType>, IDataResolver
{
	private Type HandleType => typeof(PunishmentRaidRewardType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(PunishmentRaidRewardType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((PunishmentRaidRewardType)obj, jitSerializer);
	}

	public byte[] SerializeValue(PunishmentRaidRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(PunishmentRaidRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public PunishmentRaidRewardType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (PunishmentRaidRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(PunishmentRaidRewardType target, ByteSpan span, JitPacker packer)
	{
	}

	public PunishmentRaidRewardType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (PunishmentRaidRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
