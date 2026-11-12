using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ShakmehBossTypeValueResolver : IDataResolverT<ShakmehBossType>, IDataResolver
{
	private Type HandleType => typeof(ShakmehBossType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ShakmehBossType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ShakmehBossType)obj, jitSerializer);
	}

	public byte[] SerializeValue(ShakmehBossType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ShakmehBossType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ShakmehBossType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ShakmehBossType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ShakmehBossType target, ByteSpan span, JitPacker packer)
	{
	}

	public ShakmehBossType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ShakmehBossType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
