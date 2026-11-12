using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NGame2;
using NGame2.IAP;
using NGame2.NConfig;
using NGame2.NContents;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NUtil;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NWeb;

public class WebServiceRequester : SingletonUpdater<WebServiceRequester>
{
	public const int MaxRetryCount = 2;

	public const WebRequestFlag DefaultWebRequestFlag = WebRequestFlag.ShowLoadingGauge;

	public Action<Request, Response, string> Listen;

	private List<WebServiceRequest> _serviceRequestList;

	private Aes128 _aes = new Aes128();

	private List<IServiceDisabler> _serviceDisablerList;

	private Dictionary<WebServiceRequest, object> waiterList = new Dictionary<WebServiceRequest, object>();

	public string Host { get; private set; }

	public string SessionKey { get; set; }

	public string AesKey { get; set; }

	public int RequestSequence { get; set; }

	public Dictionary<string, string> Headers { get; private set; }

	public Func<BaseResultType, Response, bool> OnBaseResultError { get; set; }

	public Action<Response> OnBaseResponse { get; set; }

	public Func<string, bool, bool, IEnumerator> OnRetryRequest { get; set; }

	public WebServiceRequester()
	{
		_serviceRequestList = new List<WebServiceRequest>();
		_serviceDisablerList = new List<IServiceDisabler>();
		Headers = new Dictionary<string, string>();
		Reset();
	}

	public void OnSceneChanged()
	{
		if (_serviceRequestList != null && _serviceRequestList.Count == 0)
		{
			_serviceRequestList = new List<WebServiceRequest>();
		}
	}

	public void Reset()
	{
		Host = null;
		SessionKey = null;
		AesKey = null;
		RequestSequence = 0;
		_serviceDisablerList.Clear();
		Headers.Clear();
		StopCurrentRequest();
	}

	public void AddServiceDisabler(IServiceDisabler disabler)
	{
		if (!_serviceDisablerList.Contains(disabler))
		{
			_serviceDisablerList.Add(disabler);
		}
	}

	public bool RemoveServiceDisabler(IServiceDisabler disabler)
	{
		return _serviceDisablerList.Remove(disabler);
	}

	private void StopCurrentRequest()
	{
		foreach (WebServiceRequest serviceRequest in _serviceRequestList)
		{
			serviceRequest.Dispose();
		}
		_serviceRequestList.Clear();
		_serviceRequestList = new List<WebServiceRequest>();
	}

	public void SetHost(string host)
	{
		if (!string.IsNullOrEmpty(host))
		{
			Host = host;
			GlobalAutoBehaviour<HostInfoManager>.instance.HostIP = Host;
		}
	}

	private string[] GetHostTokens()
	{
		return SprkAccounts.GetHostTokens(Host);
	}

	public string GetHostAddress()
	{
		string[] hostTokens = GetHostTokens();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(hostTokens))
		{
			return null;
		}
		if (hostTokens.Length == 0)
		{
			return null;
		}
		return hostTokens[0];
	}

	public int GetHostPort()
	{
		string[] hostTokens = GetHostTokens();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(hostTokens))
		{
			return 0;
		}
		if (hostTokens.Length <= 1)
		{
			return 0;
		}
		return Vespa_String_Extension_Method.TOINT(hostTokens[1], 0);
	}

	public WebServiceRequest Request<TRequest, TResponse>(TRequest request, Action<TResponse> onResponse = null) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		return RequestInternal(Host, request, WebRequestFlag.ShowLoadingGauge, onResponse);
	}

	public WebServiceRequest Request<TRequest, TResponse>(TRequest request, WebRequestFlag flag, Action<TResponse> onResponse = null, bool isPriority = false) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		return RequestInternal(Host, request, flag, onResponse, isPriority);
	}

	public WebServiceRequest SecureRequest<TRequest, TResponse>(TRequest request, Action<TResponse> onResponse = null) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		return SecureRequest(request, WebRequestFlag.ShowLoadingGauge, onResponse);
	}

	public WebServiceRequest SecureRequest<TRequest, TResponse>(TRequest request, WebRequestFlag flag, Action<TResponse> onResponse = null) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		return RequestInternal(SprkAccounts.SecureHost(Host), request, flag, onResponse);
	}

	private WebServiceRequest RequestInternal<TRequest, TResponse>(string host, TRequest request, WebRequestFlag flag, Action<TResponse> onResponse = null, bool isPriority = false) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		string serviceName = request.ServiceName;
		Dictionary<string, object> dictionary = JsonMarshaler.EncodeJsonObject(request);
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, object>();
		}
		_ = IAPWrapper.StoreType;
		if (string.IsNullOrEmpty(host))
		{
			Vespa_System_Action_Extension_Method.fire(onResponse, new TResponse
			{
				BaseResult = BaseResultType.EmptyHost
			});
			return null;
		}
		if ((Application.internetReachability & (NetworkReachability)3) == 0)
		{
			VespaDebug.LogError("Not connected to internet");
			Vespa_System_Action_Extension_Method.fire(onResponse, new TResponse
			{
				BaseResult = BaseResultType.Offline
			});
			return null;
		}
		bool flag2 = true;
		for (int i = 0; i < _serviceDisablerList.Count; i++)
		{
			IServiceDisabler serviceDisabler = _serviceDisablerList[i];
			if (serviceDisabler != null && serviceDisabler.IsDisabled(host, serviceName))
			{
				flag2 = false;
				break;
			}
		}
		if (!flag2 && (flag & WebRequestFlag.Ping) == 0)
		{
			VespaDebug.LogError("Service disabled. service=" + serviceName);
			Vespa_System_Action_Extension_Method.fire(onResponse, new TResponse
			{
				BaseResult = BaseResultType.Disabled
			});
			return null;
		}
		dictionary.Add("T1", GetEpochTime());
		dictionary.Add("T2", (long)UnityEngine.Time.realtimeSinceStartup);
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		foreach (KeyValuePair<string, object> item in dictionary)
		{
			if (item.Value == null)
			{
				continue;
			}
			if (item.Value is IList list2)
			{
				foreach (object item2 in list2)
				{
					list.Add(new KeyValuePair<string, string>(item.Key, WWW.EscapeURL(item2.ToString())));
				}
			}
			else
			{
				list.Add(new KeyValuePair<string, string>(item.Key, WWW.EscapeURL(item.Value.ToString())));
			}
		}
		WWWForm wWWForm = new WWWForm();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item3 in list)
		{
			stringBuilder.Append($"{item3.Key}={item3.Value}&");
		}
		string text = stringBuilder.ToString();
		if (!string.IsNullOrEmpty(AesKey) && (flag & WebRequestFlag.DisableEncrypt) == 0)
		{
			string s = _aes.EncryptTextLegacy(text, AesKey);
			wWWForm.AddField("Encrypted", WWW.EscapeURL(s));
		}
		else
		{
			foreach (KeyValuePair<string, string> item4 in list)
			{
				wWWForm.AddField(item4.Key, item4.Value);
			}
			int i2 = UnityEngine.Random.Range(0, 10000);
			wWWForm.AddField("TEMP", i2);
		}
		if (!string.IsNullOrEmpty(SessionKey))
		{
			wWWForm.AddField("SessionKey", WWW.EscapeURL(SessionKey));
		}
		if (host.IndexOf("://") < 0)
		{
			host = "http://" + host;
		}
		if (!host.EndsWith("/"))
		{
			host += "/";
		}
		int requestSequence = ++RequestSequence;
		WebServiceRequest<TRequest, TResponse> webServiceRequest = new WebServiceRequest<TRequest, TResponse>
		{
			RequestSequence = requestSequence,
			Form = wWWForm,
			Headers = Headers,
			ServiceName = serviceName,
			Host = host,
			Argument = text,
			Flag = flag,
			Request = request,
			OnResponse = onResponse,
			Runner = StartRequest
		};
		SprkNetworkPerformance.Prepare(webServiceRequest, SessionKey);
		if (isPriority)
		{
			_serviceRequestList.Insert(0, webServiceRequest);
		}
		else
		{
			_serviceRequestList.Add(webServiceRequest);
		}
		object obj = null;
		if ((flag & WebRequestFlag.ShowLoadingGauge) != WebRequestFlag.None)
		{
			obj = new object();
			WaitingPopup.Open(obj);
			waiterList.Add(webServiceRequest, obj);
		}
		return webServiceRequest;
	}

	private IEnumerator RequestCoroutine<TRequest, TResponse>(WebServiceRequest<TRequest, TResponse> serviceRequest) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		if (serviceRequest == null)
		{
			yield break;
		}
		int retryCount = 0;
		Action<TResponse> onResponse;
		string text;
		string text2;
		bool compression;
		BaseResultType baseResultType;
		object waitingPopupContext;
		bool flag2;
		bool flag3;
		while (true)
		{
			serviceRequest.CreateRequest();
			string serviceName = serviceRequest.ServiceName;
			WebRequestFlag flag = serviceRequest.Flag;
			onResponse = serviceRequest.OnResponse;
			DateTime requestStartTime = DateTime.UtcNow;
			int num = (SingletonAuto<ContentsValueManager>.isValidInstance ? SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue($"api:{serviceRequest.ServiceName}", (int)WebServiceRequest.GetTimeout()) : 40);
			DateTime expiredTime = requestStartTime.AddSeconds(num);
			while (serviceRequest.Www != null && !serviceRequest.Www.isDone && DateTime.UtcNow < expiredTime)
			{
				yield return null;
			}
			int time = (int)(DateTime.UtcNow - requestStartTime).TotalMilliseconds;
			if (ProfileManager.IsLogEnabled)
			{
				GlobalAutoBehaviour<ProfileManager>.instance.RecordHttpResponseTime(time, serviceRequest.ServiceName);
			}
			text = "";
			text2 = "";
			compression = false;
			try
			{
				if (serviceRequest.Www != null && serviceRequest.Www.isDone)
				{
					if (String_Extension_Method.IsNullOrEmpty(serviceRequest.Www.error))
					{
						baseResultType = BaseResultType.Success;
						text2 = serviceRequest.Www.text;
						foreach (KeyValuePair<string, string> responseHeader in serviceRequest.Www.responseHeaders)
						{
							if (responseHeader.Key == "VESPA-CONTENT-ENCODING")
							{
								compression = true;
							}
						}
					}
					else
					{
						baseResultType = BaseResultType.HttpError;
						text = serviceRequest.Www.error;
					}
				}
				else
				{
					baseResultType = BaseResultType.Timeout;
					text = "Timeout: " + serviceName;
				}
			}
			catch (Exception ex)
			{
				baseResultType = BaseResultType.InternalError;
				text = ex.ToString();
			}
			waitingPopupContext = null;
			if (waiterList.ContainsKey(serviceRequest))
			{
				waitingPopupContext = waiterList[serviceRequest];
				WaitingPopup.Close(waitingPopupContext);
				waiterList.Remove(serviceRequest);
			}
			serviceRequest.Dispose();
			flag2 = (baseResultType == BaseResultType.Timeout || baseResultType == BaseResultType.HttpError) && (flag & WebRequestFlag.DisableRetry) == 0;
			flag3 = (flag & WebRequestFlag.SilenceError) == 0;
			if (!flag2 || OnRetryRequest == null || retryCount >= 2)
			{
				break;
			}
			int num2 = retryCount + 1;
			retryCount = num2;
			waitingPopupContext = null;
			if ((flag & WebRequestFlag.ShowLoadingGauge) != WebRequestFlag.None)
			{
				waitingPopupContext = new object();
				WaitingPopup.Open(waitingPopupContext);
				waiterList.Add(serviceRequest, waitingPopupContext);
			}
			Debug.Log("RetryCount : " + retryCount);
		}
		if (baseResultType != BaseResultType.Success)
		{
			VespaDebug.LogError("Response error (sent on " + serviceRequest.RequestedTime.ToString() + "): " + baseResultType.ToString() + ". error=" + text + ". text=" + text2 + ". host=" + serviceRequest.Host);
			if (!flag3 || !Vespa_System_Action_Extension_Method.fire(OnBaseResultError, baseResultType, null))
			{
				if (flag2 && OnRetryRequest != null)
				{
					yield return VespaCoroutine.Start(OnRetryRequest(text, arg2: false, flag3));
				}
				TResponse i_argA = new TResponse
				{
					BaseResult = baseResultType
				};
				Vespa_System_Action_Extension_Method.fire(onResponse, i_argA);
			}
		}
		else
		{
			WaitingPopup.Open(waitingPopupContext);
			Executer executer = SingletonUpdater<Executer>.instance;
			SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
			{
				ProcessWebResponseThreaded(executer, serviceRequest, text2, compression);
			}, (Action)delegate
			{
				WaitingPopup.Close(waitingPopupContext);
			});
		}
	}

	private void ProcessWebResponseThreaded<TRequest, TResponse>(Executer executer, WebServiceRequest<TRequest, TResponse> serviceRequest, string text, bool compression) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		SprkNetworkPerformance.BeginResponse(serviceRequest);
		WebServiceRequest<TRequest, TResponse> serviceRequest2 = serviceRequest;
		if (serviceRequest2 == null)
		{
			return;
		}
		_ = text.Length;
		if (!string.IsNullOrEmpty(AesKey) && (serviceRequest2.Flag & WebRequestFlag.DisableEncrypt) == 0)
		{
			try
			{
				text = _aes.DecryptTextLegacy(text, AesKey);
			}
			catch (Exception)
			{
			}
		}
		if (compression)
		{
			byte[] compressedData = Convert.FromBase64String(text);
			text = GZipCompressor.Decompress(compressedData);
		}
		TResponse response = JsonMarshaler.Parse<TResponse>(text);
		SprkNetworkPerformance.Capture(text, serviceRequest, response);
		SprkDispatch.CaptureRewards(text, response);
		Action<TResponse> onResponse = serviceRequest2.OnResponse;
		executer.NextRun(delegate
		{
			SprkDispatch.ApplyRewards(response);
			Vespa_System_Action_Extension_Method.fire(Listen, serviceRequest2.Request, response, text);
			if (response == null)
			{
				VespaDebug.LogError("Parsing error : " + text);
				Vespa_System_Action_Extension_Method.fire(onResponse, new TResponse
				{
					BaseResult = BaseResultType.ParsingError
				});
			}
			else
			{
				if (ClientDebugNetwork.ForceSessionExpired)
				{
					response.BaseResult = BaseResultType.SessionError;
				}
				if (response.BaseResult != BaseResultType.Success)
				{
					VespaDebug.LogError("Base result error : " + response.BaseResult);
					if (!Vespa_System_Action_Extension_Method.fire(OnBaseResultError, response.BaseResult, response))
					{
						Vespa_System_Action_Extension_Method.fire(onResponse, response);
					}
				}
				else
				{
					Vespa_System_Action_Extension_Method.fire(OnBaseResponse, response);
					SprkNetworkPerformance.ProgressionApplied(response);
					Vespa_System_Action_Extension_Method.fire(onResponse, response);
				}
			}
		});
	}

	private void StartRequest<TRequest, TResponse>(WebServiceRequest<TRequest, TResponse> serviceRequest) where TRequest : Request, IServiceRequest where TResponse : Response, new()
	{
		VespaCoroutine.Start(RequestCoroutine(serviceRequest));
	}

	public override void ChainUpdate()
	{
		SprkNetworkPerformance.Pump(_serviceRequestList);
	}

	public bool HasAnyRequestOfFlag(WebRequestFlag flag)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_serviceRequestList); i++)
		{
			if ((_serviceRequestList[i].Flag & flag) != WebRequestFlag.None)
			{
				return true;
			}
		}
		return false;
	}

	public long GetEpochTime()
	{
		return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
	}

	public static string MakeKeyValueString(string key, string value)
	{
		return $"{key}={value}&";
	}

	public void SetRequestHeader(string name, string value)
	{
		Headers[name] = value;
	}
}
