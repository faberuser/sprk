using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_NewbieRewardTypeValueResolver : IDataResolverT<NewbieRewardType>, IDataResolver
{
	private Type HandleType => typeof(NewbieRewardType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(NewbieRewardType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((NewbieRewardType)obj, jitSerializer);
	}

	public byte[] SerializeValue(NewbieRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(NewbieRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public NewbieRewardType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (NewbieRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(NewbieRewardType target, ByteSpan span, JitPacker packer)
	{
	}

	public NewbieRewardType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (NewbieRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
