using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PhaseStepRewardTypeValueResolver : IDataResolverT<PhaseStepRewardType>, IDataResolver
{
	private Type HandleType => typeof(PhaseStepRewardType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(PhaseStepRewardType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((PhaseStepRewardType)obj, jitSerializer);
	}

	public byte[] SerializeValue(PhaseStepRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(PhaseStepRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public PhaseStepRewardType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (PhaseStepRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(PhaseStepRewardType target, ByteSpan span, JitPacker packer)
	{
	}

	public PhaseStepRewardType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (PhaseStepRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
