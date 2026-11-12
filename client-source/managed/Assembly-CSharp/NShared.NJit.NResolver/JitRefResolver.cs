using System;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class JitRefResolver<T> : IDataResolverT<JitRef<T>>, IDataResolver where T : class, new()
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(JitRef<T>);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		return SerializeValue(obj as JitRef<T>, jitSerializer);
	}

	public byte[] SerializeValue(JitRef<T> jitRef, JitSerializer jitSerializer)
	{
		if (jitRef == null)
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		T value = jitRef.JsonLoad_GetOrNew();
		return Proxy<T>.DataResolver.SerializeReference(value, jitSerializer);
	}

	public byte[] SerializeReference(JitRef<T> jitRef, JitSerializer jitSerializer)
	{
		return jitSerializer.SerializeReference(jitRef);
	}

	public JitRef<T> Evaluate(ByteSpan span, JitPacker packer)
	{
		JitRef<T> jitRef = new JitRef<T>();
		((IJitContainer)jitRef).Bind(packer);
		return jitRef;
	}

	public void EvaluateLater(JitRef<T> target, ByteSpan span, JitPacker packer)
	{
		((IJitContainer)target).Deserialize(span);
	}

	public JitRef<T> EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return JitSerializer.EvaluateReference<JitRef<T>>(span, packer, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitSerializer.SkipEvaluateReference(span, out nextSpan);
	}
}
