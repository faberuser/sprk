using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StateConditionTypeValueResolver : IDataResolverT<StateConditionType>, IDataResolver
{
	private Type HandleType => typeof(StateConditionType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(StateConditionType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((StateConditionType)obj, jitSerializer);
	}

	public byte[] SerializeValue(StateConditionType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(StateConditionType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public StateConditionType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (StateConditionType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(StateConditionType target, ByteSpan span, JitPacker packer)
	{
	}

	public StateConditionType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (StateConditionType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
