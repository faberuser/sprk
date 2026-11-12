using System;

namespace NVespa;

public class ObjVerifier : IVerifier
{
	private WeakReference _ref;

	public ObjVerifier(object obj)
	{
		_ref = new WeakReference(obj);
	}

	public bool IsValid()
	{
		if (_ref == null)
		{
			return false;
		}
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(_ref.Target))
		{
			_ref = null;
			return false;
		}
		return true;
	}
}
