using NVespa;
using NVespa.NSingleton;
using UnityEngine.SignInWithApple;

namespace NGame2.NAccount;

public class AppleManager : GlobalAutoBehaviour<AppleManager>
{
	private SignInWithApple _signInWithApple;

	private SignInWithApple.Callback _callback;

	private static int _cachedEnableIOSVersion = -1;

	protected override void Awake()
	{
		base.Awake();
		ActivateApp();
	}

	private void ActivateApp()
	{
		_signInWithApple = base.gameObject.AddComponent<SignInWithApple>();
	}

	public void Login(SignInWithApple.Callback callback)
	{
		if (_signInWithApple == null)
		{
			ActivateApp();
		}
		_callback = callback;
		_signInWithApple.Login(LoginCompletedCallback);
	}

	private void LoginCompletedCallback(SignInWithApple.CallbackArgs callback)
	{
		if (!String_Extension_Method.IsNullOrEmpty(callback.error))
		{
			OnError(callback);
		}
		if (_callback != null)
		{
			_callback(callback);
		}
	}

	private void OnError(SignInWithApple.CallbackArgs callback)
	{
		VespaDebug.LogError(callback.error);
	}

	public static bool IsEnableIOSVersion()
	{
		return false;
	}
}
