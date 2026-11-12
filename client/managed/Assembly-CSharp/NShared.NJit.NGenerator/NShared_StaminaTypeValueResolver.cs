using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StaminaTypeValueResolver : IDataResolverT<StaminaType>, IDataResolver
{
	private Type HandleType => typeof(StaminaType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(StaminaType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((StaminaType)obj, jitSerializer);
	}

	public byte[] SerializeValue(StaminaType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(StaminaType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public StaminaType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (StaminaType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(StaminaType target, ByteSpan span, JitPacker packer)
	{
	}

	public StaminaType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (StaminaType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
