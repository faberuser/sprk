using System;
using System.Collections;
using System.IO.MemoryMappedFiles;
using System.Text;
using NGame2.NLogin;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Networking;

namespace NGame2.NAccount;

public class MasangLoginManager : GlobalAutoBehaviour<MasangLoginManager>
{
	public enum LoginSuccessState
	{
		None,
		Success,
		Failed,
		Cancel
	}

	public delegate void LoginResultCallback(bool issuccess, LoginSuccData data);

	public delegate void ConnectedPlatformCallback(bool issuccess, ConnectedData data);

	public delegate void LinkedCallback(LoginSuccessState loginstate, string msg = "");

	public delegate void SignoutCallback(bool issucess);

	private LoginSuccessState _loginSuccessState;

	private string _errormsg = string.Empty;

	public string webClientId = "289464588431-ndfsrmrul8ouooa4jrfchslks9ias02s.apps.googleusercontent.com";

	private LoginResultCallback _callback;

	private ConnectedPlatformCallback _connectcallbak;

	private LinkedCallback _linkedCallback;

	private SignoutCallback _signoutCallback;

	private int count;

	private string _deviceid;

	private GetTokenData tokendata = new GetTokenData();

	private MemoryMappedFile mmf;

	private MemoryMappedViewAccessor accessor;

	protected override void Awake()
	{
		SprkAccounts.Initialize();
		base.Awake();
		DeviceIDManager.deviceIDHandler += delegate(string deviceid)
		{
			if (!string.IsNullOrEmpty(deviceid))
			{
				_deviceid = deviceid;
			}
		};
		DeviceIDManager.GetDeviceID();
	}

	public void SetListener()
	{
		count = 0;
		mmf = MemoryMappedFile.CreateOrOpen("MySharedMemory", 4096L);
		accessor = mmf.CreateViewAccessor(0L, 4096L);
		InvokeRepeating("ReadMessage", 1f, 1f);
	}

	private void ReadMessage()
	{
		try
		{
			int num = accessor.ReadInt32(0L);
			byte[] array = new byte[num];
			accessor.ReadArray(4L, array, 0, num);
			string text = Encoding.UTF8.GetString(array);
			if (text != "")
			{
				if (!text.StartsWith("kingsraid://"))
				{
					return;
				}
				string[] array2 = new Uri(text).Query.Split('&');
				bool flag = false;
				string[] array3 = array2;
				foreach (string text2 in array3)
				{
					string text3 = text2;
					int num2 = text3.IndexOf('?');
					if (num2 >= 0)
					{
						string[] array4 = text3.Substring(num2 + 1).Split('=');
						if (array4.Length == 2)
						{
							string text4 = array4[0];
							string text5 = array4[1];
							if (text4 == "id")
							{
								tokendata.id = text5;
							}
							else if (text4 == "status" && text5 == "error")
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						string[] array5 = text2.Split('=');
						if (array5.Length != 2)
						{
							continue;
						}
						string text6 = array5[0];
						string text7 = array5[1];
						if (text6 == "code")
						{
							switch (text7)
							{
							case "409":
								CancelInvoke("ReadMessage");
								accessor.Dispose();
								mmf.Dispose();
								_errormsg = "Platform Already Linked";
								_loginSuccessState = LoginSuccessState.Failed;
								return;
							case "400":
								CancelInvoke("ReadMessage");
								accessor.Dispose();
								mmf.Dispose();
								_errormsg = "Bad Request";
								_loginSuccessState = LoginSuccessState.Failed;
								return;
							case "404":
								CancelInvoke("ReadMessage");
								accessor.Dispose();
								mmf.Dispose();
								_errormsg = "Not Found";
								_loginSuccessState = LoginSuccessState.Failed;
								return;
							case "500":
								CancelInvoke("ReadMessage");
								accessor.Dispose();
								mmf.Dispose();
								_errormsg = "Internal Server Error";
								_loginSuccessState = LoginSuccessState.Failed;
								return;
							}
						}
						continue;
					}
					string[] array6 = text2.Split('=');
					if (array6.Length == 2)
					{
						string text8 = array6[0];
						string text9 = array6[1];
						if (text8 == "access_token")
						{
							tokendata.access_token = text9;
						}
						if (text8 == "expires_in")
						{
							tokendata.expires_in = text9;
						}
						if (text8 == "refresh_token")
						{
							tokendata.refresh_token = text9;
						}
					}
				}
				CancelInvoke("ReadMessage");
				accessor.Dispose();
				mmf.Dispose();
				_loginSuccessState = LoginSuccessState.Success;
			}
			else
			{
				count++;
				if (count == 60)
				{
					CancelInvoke("ReadMessage");
					accessor.Dispose();
					mmf.Dispose();
					_errormsg = "Time Out";
					_loginSuccessState = LoginSuccessState.Failed;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void GoogleBtn()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (accessor != null)
		{
			accessor.Dispose();
		}
		if (mmf != null)
		{
			mmf.Dispose();
		}
	}

	public void OnGoolgSignOut()
	{
	}

	public void SteamLogin(LoginResultCallback callback)
	{
		SprkAccounts.Reject(callback);
	}

	public void GoogleLogin(LoginResultCallback callback)
	{
		SprkAccounts.Reject(callback);
	}

	public void AppleLogin(LoginResultCallback callback)
	{
		SprkAccounts.Reject(callback);
	}

	public void GuestLogin(LoginResultCallback callback)
	{
		SprkAccounts.Complete(callback);
	}

	private IEnumerator StartGuestLogin()
	{
		string text = JsonUtility.ToJson(new GuestData
		{
			product = "kr",
			id = NGame2.NLogin.Helper.GetDeviceUid()
		});
		UnityWebRequest request = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/login/guest?device=" + _deviceid, text);
		byte[] bytes = new UTF8Encoding().GetBytes(text);
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.error == null)
		{
			WebRequestTokenData webRequestTokenData = JsonUtility.FromJson<WebRequestTokenData>(request.downloadHandler.text);
			tokendata.access_token = webRequestTokenData.data.access_token;
			tokendata.refresh_token = webRequestTokenData.data.refresh_token;
			tokendata.expires_in = webRequestTokenData.data.expires_in;
			LoginData.LastAccessToken = tokendata.access_token;
			LoginData.LastRefreshToken = tokendata.refresh_token;
			string text2 = JsonUtility.ToJson(new VerifyData
			{
				product = "kr"
			});
			UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/token/verify", text2);
			byte[] bytes2 = new UTF8Encoding().GetBytes(text2);
			request2.uploadHandler = new UploadHandlerRaw(bytes2);
			request2.downloadHandler = new DownloadHandlerBuffer();
			request2.SetRequestHeader("Authorization", "Bearer " + tokendata.access_token);
			request2.SetRequestHeader("Content-Type", "application/json");
			yield return request2.SendWebRequest();
			if (request2.error == null)
			{
				WebRequestSessionData webRequestSessionData = JsonUtility.FromJson<WebRequestSessionData>(request2.downloadHandler.text);
				LoginSuccData loginSuccData = new LoginSuccData();
				loginSuccData.id = webRequestSessionData.data.id;
				loginSuccData.sessionid = webRequestSessionData.data.session;
				_callback(issuccess: true, loginSuccData);
			}
			else
			{
				_callback(issuccess: false, null);
			}
		}
		else
		{
			_callback(issuccess: false, null);
		}
	}

	private void CustomTabFinish()
	{
		if (_loginSuccessState != LoginSuccessState.Success)
		{
			_loginSuccessState = LoginSuccessState.Cancel;
		}
	}

	private void HandleDeepLink(string url)
	{
		if (!url.Contains("login"))
		{
			return;
		}
		string[] array = GetParameterFromUrl(url, "token").Split('&');
		bool flag = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text;
			int num = text2.IndexOf('?');
			if (num >= 0)
			{
				string[] array3 = text2.Substring(num + 1).Split('=');
				if (array3.Length == 2)
				{
					string text3 = array3[0];
					string text4 = array3[1];
					if (text3 == "id")
					{
						tokendata.id = text4;
					}
					else if (text3 == "status" && text4 == "error")
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				string[] array4 = text.Split('=');
				if (array4.Length != 2)
				{
					continue;
				}
				string text5 = array4[0];
				string text6 = array4[1];
				if (text5 == "code")
				{
					switch (text6)
					{
					case "409":
						_errormsg = "Platform Already Linked";
						_loginSuccessState = LoginSuccessState.Failed;
						return;
					case "400":
						_errormsg = "Bad Request";
						_loginSuccessState = LoginSuccessState.Failed;
						return;
					case "404":
						_errormsg = "Not Found";
						_loginSuccessState = LoginSuccessState.Failed;
						return;
					case "500":
						_errormsg = "Internal Server Error";
						_loginSuccessState = LoginSuccessState.Failed;
						return;
					}
				}
				continue;
			}
			string[] array5 = text.Split('=');
			if (array5.Length == 2)
			{
				string text7 = array5[0];
				string text8 = array5[1];
				if (text7 == "access_token")
				{
					tokendata.access_token = text8;
				}
				if (text7 == "expires_in")
				{
					tokendata.expires_in = text8;
				}
				if (text7 == "refresh_token")
				{
					tokendata.refresh_token = text8;
				}
			}
		}
		_loginSuccessState = LoginSuccessState.Success;
	}

	private string GetParameterFromUrl(string url, string parameterName)
	{
		return new Uri(url).Query;
	}

	public void AccessTokenLogin(LoginResultCallback callback)
	{
		_callback = callback;
		StartCoroutine(StartAccessTokenLogin());
	}

	private IEnumerator StartAccessTokenLogin()
	{
		bool isStop = false;
		int count = 0;
		while (!isStop)
		{
			string text = JsonUtility.ToJson(new VerifyData
			{
				product = "kr"
			});
			UnityWebRequest request = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/token/verify", text);
			byte[] bytes = new UTF8Encoding().GetBytes(text);
			request.uploadHandler = new UploadHandlerRaw(bytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Authorization", "Bearer " + LoginData.LastAccessToken);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				if (!request.downloadHandler.text.Contains("error"))
				{
					if (request.downloadHandler.text.Contains("deleted_dt"))
					{
						WebRequestDeleteIdData webRequestDeleteIdData = JsonUtility.FromJson<WebRequestDeleteIdData>(request.downloadHandler.text);
						string deleted_dt = webRequestDeleteIdData.data.deleted_dt;
						LoginSuccData loginSuccData = new LoginSuccData();
						loginSuccData.id = webRequestDeleteIdData.data.id;
						loginSuccData.sessionid = deleted_dt;
						_callback(issuccess: false, loginSuccData);
					}
					else
					{
						WebRequestSessionData webRequestSessionData = JsonUtility.FromJson<WebRequestSessionData>(request.downloadHandler.text);
						LoginSuccData loginSuccData2 = new LoginSuccData();
						loginSuccData2.id = webRequestSessionData.data.id;
						loginSuccData2.sessionid = webRequestSessionData.data.session;
						_callback(issuccess: true, loginSuccData2);
					}
					break;
				}
				if (request.downloadHandler.text.Contains("404"))
				{
					string text2 = JsonUtility.ToJson(new RefreshData
					{
						grant_type = "refresh_token",
						refresh_token = LoginData.LastRefreshToken
					});
					UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text2);
					byte[] bytes2 = new UTF8Encoding().GetBytes(text2);
					request2.uploadHandler = new UploadHandlerRaw(bytes2);
					request2.downloadHandler = new DownloadHandlerBuffer();
					request2.SetRequestHeader("Content-Type", "application/json");
					yield return request2.SendWebRequest();
					if (request2.error != null)
					{
						LoginData.LastAccessToken = "";
						LoginData.LastRefreshToken = "";
						_callback(issuccess: false, null);
						break;
					}
					if (request2.downloadHandler.text.Contains("error"))
					{
						_callback(issuccess: false, null);
						break;
					}
					WebRequestRefreshData webRequestRefreshData = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
					LoginData.LastAccessToken = webRequestRefreshData.data.access_token;
					LoginData.LastRefreshToken = webRequestRefreshData.data.refresh_token;
				}
			}
			else
			{
				string text3 = JsonUtility.ToJson(new RefreshData
				{
					grant_type = "refresh_token",
					refresh_token = LoginData.LastRefreshToken
				});
				UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text3);
				byte[] bytes3 = new UTF8Encoding().GetBytes(text3);
				request2.uploadHandler = new UploadHandlerRaw(bytes3);
				request2.downloadHandler = new DownloadHandlerBuffer();
				request2.SetRequestHeader("Content-Type", "application/json");
				yield return request2.SendWebRequest();
				if (request2.error != null)
				{
					LoginData.LastAccessToken = "";
					LoginData.LastRefreshToken = "";
					_callback(issuccess: false, null);
					break;
				}
				if (request2.downloadHandler.text.Contains("error"))
				{
					_callback(issuccess: false, null);
					break;
				}
				WebRequestRefreshData webRequestRefreshData2 = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
				LoginData.LastAccessToken = webRequestRefreshData2.data.access_token;
				LoginData.LastRefreshToken = webRequestRefreshData2.data.refresh_token;
			}
			count++;
			if (count == 10)
			{
				_callback(issuccess: false, null);
				break;
			}
		}
	}

	public void CheckLinkedPlatform(ConnectedPlatformCallback callback)
	{
		_connectcallbak = callback;
		StartCoroutine(StartCheckLinedPlatform());
	}

	private IEnumerator StartCheckLinedPlatform()
	{
		bool isStop = false;
		int count = 0;
		while (!isStop)
		{
			UnityWebRequest request = UnityWebRequest.Get(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/game-account/?product=kr");
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Authorization", "Bearer " + LoginData.LastAccessToken);
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				if (!request.downloadHandler.text.Contains("error"))
				{
					WebRequestCheckLinked webRequestCheckLinked = JsonUtility.FromJson<WebRequestCheckLinked>(request.downloadHandler.text);
					ConnectedData connectedData = new ConnectedData();
					for (int i = 0; i < webRequestCheckLinked.data.Length; i++)
					{
						if (webRequestCheckLinked.data[i].platform == "google")
						{
							connectedData.isGoogle = true;
						}
						else if (webRequestCheckLinked.data[i].platform == "apple")
						{
							connectedData.isApple = true;
						}
						else if (webRequestCheckLinked.data[i].platform == "steam")
						{
							connectedData.isSteam = true;
						}
					}
					_connectcallbak(issuccess: true, connectedData);
					break;
				}
				if (request.downloadHandler.text.Contains("404"))
				{
					string text = JsonUtility.ToJson(new RefreshData
					{
						grant_type = "refresh_token",
						refresh_token = LoginData.LastRefreshToken
					});
					UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text);
					byte[] bytes = new UTF8Encoding().GetBytes(text);
					request2.uploadHandler = new UploadHandlerRaw(bytes);
					request2.downloadHandler = new DownloadHandlerBuffer();
					request2.SetRequestHeader("Content-Type", "application/json");
					yield return request2.SendWebRequest();
					if (request2.error != null)
					{
						LoginData.LastAccessToken = "";
						LoginData.LastRefreshToken = "";
						_connectcallbak(issuccess: false, null);
						break;
					}
					if (request2.downloadHandler.text.Contains("error"))
					{
						_connectcallbak(issuccess: false, null);
						break;
					}
					WebRequestRefreshData webRequestRefreshData = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
					LoginData.LastAccessToken = webRequestRefreshData.data.access_token;
					LoginData.LastRefreshToken = webRequestRefreshData.data.refresh_token;
				}
			}
			else
			{
				string text2 = JsonUtility.ToJson(new RefreshData
				{
					grant_type = "refresh_token",
					refresh_token = LoginData.LastRefreshToken
				});
				UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text2);
				byte[] bytes2 = new UTF8Encoding().GetBytes(text2);
				request2.uploadHandler = new UploadHandlerRaw(bytes2);
				request2.downloadHandler = new DownloadHandlerBuffer();
				request2.SetRequestHeader("Content-Type", "application/json");
				yield return request2.SendWebRequest();
				if (request2.error != null)
				{
					LoginData.LastAccessToken = "";
					LoginData.LastRefreshToken = "";
					_connectcallbak(issuccess: false, null);
					break;
				}
				if (request2.downloadHandler.text.Contains("error"))
				{
					_connectcallbak(issuccess: false, null);
					break;
				}
				WebRequestRefreshData webRequestRefreshData2 = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
				LoginData.LastAccessToken = webRequestRefreshData2.data.access_token;
				LoginData.LastRefreshToken = webRequestRefreshData2.data.refresh_token;
			}
			count++;
			if (count == 10)
			{
				_connectcallbak(issuccess: false, null);
				break;
			}
		}
	}

	public void LinkedApple(LinkedCallback linkcallback)
	{
		linkcallback?.Invoke(LoginSuccessState.Failed, "Sign in with your SPRK username and password.");
	}

	public void LinkedSteam(LinkedCallback linkcallback)
	{
		linkcallback?.Invoke(LoginSuccessState.Failed, "Sign in with your SPRK username and password.");
	}

	public void LinkedGoogle(LinkedCallback linkcallback)
	{
		linkcallback?.Invoke(LoginSuccessState.Failed, "Sign in with your SPRK username and password.");
	}

	public void AccountSignOut(SignoutCallback sigoutCallback)
	{
		_signoutCallback = sigoutCallback;
		StartCoroutine(StartAccountSignOut());
	}

	private IEnumerator StartAccountSignOut()
	{
		do
		{
			string s = JsonUtility.ToJson(new VerifyData
			{
				product = "kr"
			});
			string url = SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/game-account/withdraw";
			UnityWebRequest request = new UnityWebRequest(url, "PATCH");
			byte[] bytes = new UTF8Encoding().GetBytes(s);
			request.uploadHandler = new UploadHandlerRaw(bytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Authorization", "Bearer " + LoginData.LastAccessToken);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				if (!request.downloadHandler.text.Contains("error"))
				{
					_signoutCallback(issucess: true);
					yield break;
				}
				if (request.downloadHandler.text.Contains("404"))
				{
					string text = JsonUtility.ToJson(new RefreshData
					{
						grant_type = "refresh_token",
						refresh_token = LoginData.LastRefreshToken
					});
					UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text);
					byte[] bytes2 = new UTF8Encoding().GetBytes(text);
					request2.uploadHandler = new UploadHandlerRaw(bytes2);
					request2.downloadHandler = new DownloadHandlerBuffer();
					request2.SetRequestHeader("Content-Type", "application/json");
					yield return request2.SendWebRequest();
					if (request2.error != null)
					{
						_signoutCallback(issucess: false);
						yield break;
					}
					if (request2.downloadHandler.text.Contains("error"))
					{
						_signoutCallback(issucess: false);
						yield break;
					}
					WebRequestRefreshData webRequestRefreshData = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
					LoginData.LastAccessToken = webRequestRefreshData.data.access_token;
					LoginData.LastRefreshToken = webRequestRefreshData.data.refresh_token;
				}
			}
			else
			{
				string text2 = JsonUtility.ToJson(new RefreshData
				{
					grant_type = "refresh_token",
					refresh_token = LoginData.LastRefreshToken
				});
				UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text2);
				byte[] bytes3 = new UTF8Encoding().GetBytes(text2);
				request2.uploadHandler = new UploadHandlerRaw(bytes3);
				request2.downloadHandler = new DownloadHandlerBuffer();
				request2.SetRequestHeader("Content-Type", "application/json");
				yield return request2.SendWebRequest();
				if (request2.error != null)
				{
					_signoutCallback(issucess: false);
					yield break;
				}
				if (request2.downloadHandler.text.Contains("error"))
				{
					_signoutCallback(issucess: false);
					yield break;
				}
				WebRequestRefreshData webRequestRefreshData2 = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
				LoginData.LastAccessToken = webRequestRefreshData2.data.access_token;
				LoginData.LastRefreshToken = webRequestRefreshData2.data.refresh_token;
			}
			count++;
		}
		while (count < 10);
		_signoutCallback(issucess: false);
	}

	public void AccountSignOutCancel(SignoutCallback singoutCallback)
	{
		_signoutCallback = singoutCallback;
		StartCoroutine(StartAccountSignOutCancel());
	}

	private IEnumerator StartAccountSignOutCancel()
	{
		do
		{
			string s = JsonUtility.ToJson(new VerifyData
			{
				product = "kr"
			});
			string url = SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/game-account/cancel";
			UnityWebRequest request = new UnityWebRequest(url, "PATCH");
			byte[] bytes = new UTF8Encoding().GetBytes(s);
			request.uploadHandler = new UploadHandlerRaw(bytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Authorization", "Bearer " + LoginData.LastAccessToken);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				if (!request.downloadHandler.text.Contains("error"))
				{
					_signoutCallback(issucess: true);
					yield break;
				}
				if (request.downloadHandler.text.Contains("404"))
				{
					string text = JsonUtility.ToJson(new RefreshData
					{
						grant_type = "refresh_token",
						refresh_token = LoginData.LastRefreshToken
					});
					UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text);
					byte[] bytes2 = new UTF8Encoding().GetBytes(text);
					request2.uploadHandler = new UploadHandlerRaw(bytes2);
					request2.downloadHandler = new DownloadHandlerBuffer();
					request2.SetRequestHeader("Content-Type", "application/json");
					yield return request2.SendWebRequest();
					if (request2.error != null)
					{
						_signoutCallback(issucess: false);
						yield break;
					}
					if (request2.downloadHandler.text.Contains("error"))
					{
						_signoutCallback(issucess: false);
						yield break;
					}
					WebRequestRefreshData webRequestRefreshData = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
					LoginData.LastAccessToken = webRequestRefreshData.data.access_token;
					LoginData.LastRefreshToken = webRequestRefreshData.data.refresh_token;
				}
			}
			else
			{
				string text2 = JsonUtility.ToJson(new RefreshData
				{
					grant_type = "refresh_token",
					refresh_token = LoginData.LastRefreshToken
				});
				UnityWebRequest request2 = UnityWebRequest.PostWwwForm(SingletonUpdater<LoginManager>.instance.ServerInfo.LoginServer + "api/auth/refresh-token?device=" + _deviceid, text2);
				byte[] bytes3 = new UTF8Encoding().GetBytes(text2);
				request2.uploadHandler = new UploadHandlerRaw(bytes3);
				request2.downloadHandler = new DownloadHandlerBuffer();
				request2.SetRequestHeader("Content-Type", "application/json");
				yield return request2.SendWebRequest();
				if (request2.error != null)
				{
					_signoutCallback(issucess: false);
					yield break;
				}
				if (request2.downloadHandler.text.Contains("error"))
				{
					_signoutCallback(issucess: false);
					yield break;
				}
				WebRequestRefreshData webRequestRefreshData2 = JsonUtility.FromJson<WebRequestRefreshData>(request2.downloadHandler.text);
				LoginData.LastAccessToken = webRequestRefreshData2.data.access_token;
				LoginData.LastRefreshToken = webRequestRefreshData2.data.refresh_token;
			}
			count++;
		}
		while (count != 10);
		_signoutCallback(issucess: false);
	}
}
