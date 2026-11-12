using System;
using MessagePack;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class MessagePackClassResolver<T> : IDataResolverT<T>, IDataResolver where T : class
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(T);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || !(obj is T))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue(obj as T, jitSerializer);
	}

	public byte[] SerializeValue(T value, JitSerializer jitSerializer)
	{
		return MessagePackSerializer.Serialize(value, jitSerializer.MessagePackSerializerOptions);
	}

	public byte[] SerializeReference(T value, JitSerializer jitSerializer)
	{
		return jitSerializer.SerializeReference(value);
	}

	public T Evaluate(ByteSpan span, JitPacker packer)
	{
		return MessagePackSerializer.Deserialize<T>(span.SliceSegment(0), packer.MessagePackSerializerOptions);
	}

	public void EvaluateLater(T target, ByteSpan span, JitPacker packer)
	{
	}

	public T EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return JitSerializer.EvaluateReference<T>(span, packer, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitSerializer.SkipEvaluateReference(span, out nextSpan);
	}
}
