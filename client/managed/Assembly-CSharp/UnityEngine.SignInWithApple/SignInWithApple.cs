using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace UnityEngine.SignInWithApple;

public class SignInWithApple : MonoBehaviour
{
	public struct CallbackArgs
	{
		public UserCredentialState credentialState;

		public UserInfo userInfo;

		public string error;
	}

	public delegate void Callback(CallbackArgs args);

	private delegate void LoginCompleted(int result, UserInfo info);

	private delegate void GetCredentialStateCompleted(UserCredentialState state);

	private static Callback s_LoginCompletedCallback;

	private static Callback s_CredentialStateCallback;

	private static readonly Queue<Action> s_EventQueue = new Queue<Action>();

	[Header("Event fired when login is complete.")]
	public SignInWithAppleEvent onLogin;

	[Header("Event fired when the users credential state has been retrieved.")]
	public SignInWithAppleEvent onCredentialState;

	[Header("Event fired when there is an error.")]
	public SignInWithAppleEvent onError;

	[MonoPInvokeCallback(typeof(LoginCompleted))]
	private static void LoginCompletedCallback(int result, [MarshalAs(UnmanagedType.Struct)] UserInfo info)
	{
		CallbackArgs args = default(CallbackArgs);
		if (result != 0)
		{
			args.userInfo = new UserInfo
			{
				idToken = info.idToken,
				displayName = info.displayName,
				email = info.email,
				userId = info.userId,
				userDetectionStatus = info.userDetectionStatus
			};
		}
		else
		{
			args.error = info.error;
		}
		s_LoginCompletedCallback(args);
		s_LoginCompletedCallback = null;
	}

	[MonoPInvokeCallback(typeof(GetCredentialStateCompleted))]
	private static void GetCredentialStateCallback([MarshalAs(UnmanagedType.SysInt)] UserCredentialState state)
	{
		CallbackArgs args = new CallbackArgs
		{
			credentialState = state
		};
		s_CredentialStateCallback(args);
		s_CredentialStateCallback = null;
	}

	public void GetCredentialState(string userID)
	{
		GetCredentialState(userID, TriggerCredentialStateEvent);
	}

	public void GetCredentialState(string userID, Callback callback)
	{
		if (s_CredentialStateCallback != null)
		{
			throw new InvalidOperationException("Credential state fetch called while another request is in progress");
		}
		s_CredentialStateCallback = callback;
		GetCredentialState(userID);
	}

	private void GetCredentialStateInternal(string userID)
	{
	}

	public void Login()
	{
		Login(TriggerOnLoginEvent);
	}

	public void Login(Callback callback)
	{
		if (s_LoginCompletedCallback != null)
		{
			throw new InvalidOperationException("Login called while another login is in progress");
		}
		s_LoginCompletedCallback = callback;
		LoginInternal();
	}

	private void LoginInternal()
	{
	}

	private void TriggerOnLoginEvent(CallbackArgs args)
	{
		if (args.error != null)
		{
			TriggerOnErrorEvent(args);
			return;
		}
		lock (s_EventQueue)
		{
			s_EventQueue.Enqueue(delegate
			{
				if (onLogin != null)
				{
					onLogin.Invoke(args);
				}
			});
		}
	}

	private void TriggerCredentialStateEvent(CallbackArgs args)
	{
		if (args.error != null)
		{
			TriggerOnErrorEvent(args);
			return;
		}
		lock (s_EventQueue)
		{
			s_EventQueue.Enqueue(delegate
			{
				if (onCredentialState != null)
				{
					onCredentialState.Invoke(args);
				}
			});
		}
	}

	private void TriggerOnErrorEvent(CallbackArgs args)
	{
		s_EventQueue.Enqueue(delegate
		{
			if (onError != null)
			{
				onError.Invoke(args);
			}
		});
	}

	public void Update()
	{
		lock (s_EventQueue)
		{
			while (s_EventQueue.Count > 0)
			{
				s_EventQueue.Dequeue()();
			}
		}
	}
}
