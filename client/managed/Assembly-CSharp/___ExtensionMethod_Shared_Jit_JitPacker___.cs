using System.Diagnostics;
using NShared.NJit;

public static class ___ExtensionMethod_Shared_Jit_JitPacker___
{
	[Conditional("UNITY_EDITOR")]
	public static void VerifyPacker(this JitPacker This)
	{
		if (This == null)
		{
			JitOption.JitLogError("Packer.RegisterAndBind 호출이 필요");
		}
	}
}
