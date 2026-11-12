using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_HighGachaCategoryTypeValueResolver : IDataResolverT<HighGachaCategoryType>, IDataResolver
{
	private Type HandleType => typeof(HighGachaCategoryType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(HighGachaCategoryType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((HighGachaCategoryType)obj, jitSerializer);
	}

	public byte[] SerializeValue(HighGachaCategoryType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(HighGachaCategoryType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public HighGachaCategoryType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (HighGachaCategoryType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(HighGachaCategoryType target, ByteSpan span, JitPacker packer)
	{
	}

	public HighGachaCategoryType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (HighGachaCategoryType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
