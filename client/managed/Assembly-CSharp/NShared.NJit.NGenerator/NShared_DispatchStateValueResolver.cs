using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DispatchStateValueResolver : IDataResolverT<DispatchState>, IDataResolver
{
	private Type HandleType => typeof(DispatchState);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(DispatchState))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((DispatchState)obj, jitSerializer);
	}

	public byte[] SerializeValue(DispatchState value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(DispatchState value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public DispatchState Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (DispatchState)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(DispatchState target, ByteSpan span, JitPacker packer)
	{
	}

	public DispatchState EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (DispatchState)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
