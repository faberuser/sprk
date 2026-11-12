using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ESoulWeaponAbilityTicketTypeValueResolver : IDataResolverT<ESoulWeaponAbilityTicketType>, IDataResolver
{
	private Type HandleType => typeof(ESoulWeaponAbilityTicketType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ESoulWeaponAbilityTicketType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ESoulWeaponAbilityTicketType)obj, jitSerializer);
	}

	public byte[] SerializeValue(ESoulWeaponAbilityTicketType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ESoulWeaponAbilityTicketType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ESoulWeaponAbilityTicketType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ESoulWeaponAbilityTicketType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ESoulWeaponAbilityTicketType target, ByteSpan span, JitPacker packer)
	{
	}

	public ESoulWeaponAbilityTicketType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ESoulWeaponAbilityTicketType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
