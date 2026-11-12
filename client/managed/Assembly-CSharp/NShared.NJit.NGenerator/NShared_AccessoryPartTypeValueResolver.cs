using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AccessoryPartTypeValueResolver : IDataResolverT<AccessoryPartType>, IDataResolver
{
	private Type HandleType => typeof(AccessoryPartType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(AccessoryPartType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((AccessoryPartType)obj, jitSerializer);
	}

	public byte[] SerializeValue(AccessoryPartType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(AccessoryPartType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public AccessoryPartType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (AccessoryPartType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(AccessoryPartType target, ByteSpan span, JitPacker packer)
	{
	}

	public AccessoryPartType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (AccessoryPartType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
