using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using NGame2.NLobby;
using NVespa;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;
using Vuplex.WebView;

namespace NGame2.NUI.NComponent2;

public class WebViewComponent : MonoBehaviour
{
	public interface IListener
	{
		void OnWebViewClose();
	}

	private WebViewPrefab _pcWebView3D;

	private IWebView _pcWebView;

	private bool _pcBackEnabled = true;

	private string _pendingUrl;

	private int _pcRetryCount;

	private const int _pcMaxRetry = 2;

	private bool _pcInitLoadHandled;

	public UIWidget PivotWidget;

	public string WebViewName = "WebView";

	public bool openLinksInWebview;

	private IListener _listener;

	private UniWebView _webView;

	private void Awake()
	{
		int id = Process.GetCurrentProcess().Id;
		string text = Path.Combine(Application.persistentDataPath, "vuplex_chromium");
		Directory.CreateDirectory(text);
		string text2 = Path.Combine(text, $"cache_{id}");
		Directory.CreateDirectory(text2);
		string chromiumLogPath = Path.Combine(text, $"chromium_{id}.log");
		StandaloneWebView.SetCachePath(text2);
		StandaloneWebView.SetChromiumLogPath(chromiumLogPath);
	}

	public void SetBackgroundColor(Color webViewBackgroundColor)
	{
	}

	public void SetEnableBackButton(bool isEnabled)
	{
		_pcBackEnabled = isEnabled;
	}

	public void OpenUrl(string url, IListener listener)
	{
		_pendingUrl = url;
		_listener = listener;
		if (_pcWebView == null)
		{
			CreatePcWebview();
		}
		else
		{
			StartCoroutine(LoadWhenReady(url));
		}
	}

	private IEnumerator LoadWhenReady(string url)
	{
		if (!(_pcWebView3D == null))
		{
			yield return _pcWebView3D.WaitUntilInitialized();
			_pcWebView = _pcWebView3D.WebView;
			if (_pcWebView != null)
			{
				_pcWebView.SetFocused(focused: true);
			}
			_pcRetryCount = 0;
			_pcInitLoadHandled = true;
			if (!string.IsNullOrEmpty(url) && _pcWebView != null)
			{
				_pcWebView.LoadUrl(url);
			}
		}
	}

	public void Close()
	{
		if (_pcWebView3D != null)
		{
			UnityEngine.Object.Destroy(_pcWebView3D.gameObject);
			_pcWebView3D = null;
			_pcWebView = null;
			_pendingUrl = null;
			_pcInitLoadHandled = false;
		}
	}

	private static void SetLayerRecursively(GameObject go, int layer)
	{
		if (!(go == null))
		{
			go.layer = layer;
			for (int i = 0; i < go.transform.childCount; i++)
			{
				SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
			}
		}
	}

	private bool LooksLikeHtml(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return false;
		}
		string text = s.TrimStart();
		if (text.StartsWith("<"))
		{
			if (text.IndexOf("<html", StringComparison.OrdinalIgnoreCase) < 0)
			{
				return text.IndexOf("<!doctype", StringComparison.OrdinalIgnoreCase) >= 0;
			}
			return true;
		}
		return false;
	}

	private void HandleKingsRaidScheme(string fullUrl)
	{
		try
		{
			if (string.IsNullOrEmpty(fullUrl))
			{
				return;
			}
			string[] array = fullUrl.Split(new char[1] { '?' }, 2);
			if (array.Length != 2 || string.IsNullOrEmpty(array[1]))
			{
				VespaDebug.LogError("Fail to parse url from kingsraid scheme");
				ClosePcWebView();
				return;
			}
			string[] array2 = array[1].Split(new char[1] { '=' }, 2);
			if (array2.Length != 2 || string.IsNullOrEmpty(array2[1]))
			{
				VespaDebug.LogError("Fail to parse url from kingsraid scheme");
				ClosePcWebView();
				return;
			}
			string text = array2[1];
			string taskType = array[0].Replace("kingsraid://", string.Empty);
			SceneAutoBehaviour<ActivityManager>.instance.Run(taskType, new string[1] { text });
			ClosePcWebView();
		}
		catch (Exception arg)
		{
			VespaDebug.LogError($"[WebViewComponent][PC] HandleKingsRaidScheme exception: {arg}");
			ClosePcWebView();
		}
	}

	private static bool IsUnknownUrlSchemeError(object nativeErrorCodeObj)
	{
		if (nativeErrorCodeObj == null)
		{
			return false;
		}
		return nativeErrorCodeObj.ToString().IndexOf("ERR_UNKNOWN_URL_SCHEME", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	private void CreatePcWebview()
	{
		if (_pcWebView3D != null && _pcWebView != null)
		{
			return;
		}
		Camera cameraByLayer = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(PivotWidget.gameObject.layer);
		if (cameraByLayer == null)
		{
			VespaDebug.LogError("[WebViewComponent] Not Found Current Camera!");
			return;
		}
		if (PivotWidget == null || !PivotWidget.gameObject.activeInHierarchy)
		{
			VespaDebug.LogError("[WebViewComponent] Not Found Pivot Widget!");
			return;
		}
		float num = Screen.width;
		float num2 = Screen.height;
		float num3 = SingletonAuto<ResolutionInfo>.instance.OriginalWidth;
		float num4 = SingletonAuto<ResolutionInfo>.instance.OriginalHeight;
		float num5 = num3 / num;
		float num6 = num4 / num2;
		Vector3[] worldCorners = PivotWidget.worldCorners;
		Vector3 vector = cameraByLayer.WorldToScreenPoint(worldCorners[0]);
		float num7 = vector.y;
		float num8 = vector.x;
		float num9 = vector.x;
		float num10 = vector.y;
		for (int i = 1; i < worldCorners.Length; i++)
		{
			Vector3 vector2 = cameraByLayer.WorldToScreenPoint(worldCorners[i]);
			num7 = Mathf.Max(num7, vector2.y);
			num10 = Mathf.Min(num10, vector2.y);
			num8 = Mathf.Min(num8, vector2.x);
			num9 = Mathf.Max(num9, vector2.x);
		}
		float z = Mathf.Max(cameraByLayer.WorldToScreenPoint(base.transform.position).z, cameraByLayer.nearClipPlane + 0.5f);
		Vector3 vector3 = cameraByLayer.ScreenToWorldPoint(new Vector3(num8, num7, z));
		Vector3 b = cameraByLayer.ScreenToWorldPoint(new Vector3(num9, num7, z));
		Vector3 b2 = cameraByLayer.ScreenToWorldPoint(new Vector3(num8, num10, z));
		Vector3 vector4 = cameraByLayer.ScreenToWorldPoint(new Vector3(num9, num10, z));
		float num11 = Vector3.Distance(vector3, b);
		float num12 = Vector3.Distance(vector3, b2);
		Vector3 position = (vector3 + vector4) * 0.5f;
		float num13 = num11 / num12;
		float num14 = 1.7777778f;
		if (!(num13 > num14))
		{
			_ = num11 / num14;
		}
		_pcWebView3D = WebViewPrefab.Instantiate(num11, num12);
		_pcWebView3D.Resolution = 500f;
		_pcWebView3D.gameObject.name = WebViewName + "_PC";
		_pcWebView3D.gameObject.transform.position = Vector3.zero;
		_pcWebView3D.transform.SetParent(base.transform, worldPositionStays: false);
		SetLayerRecursively(_pcWebView3D.gameObject, PivotWidget.gameObject.layer);
		_pcWebView3D.transform.position = position;
		_pcWebView3D.transform.LookAt(cameraByLayer.transform);
		_pcWebView3D.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		_pcWebView3D.PixelDensity = 2f;
		_pcWebView3D.Initialized += delegate
		{
			_pcWebView = _pcWebView3D.WebView;
			_pcWebView.MessageEmitted += OnPcMessageEmitted;
			_pcWebView.UrlChanged += delegate(object s2, UrlChangedEventArgs ev2)
			{
				string url = ev2.Url;
				if (!string.IsNullOrEmpty(url) && url.StartsWith("kingsraid://", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						_pcWebView.StopLoad();
					}
					catch
					{
					}
					HandleKingsRaidScheme(url);
				}
			};
			_pcWebView.PageLoadScripts.Add("\n                    (function(){\n                        function handleKR(href){\n                            try{\n                                if(/^kingsraid:\\/\\//i.test(href)){\n                                    if(window.vuplex && window.vuplex.postMessage){\n                                        window.vuplex.postMessage(href);\n                                        return true;\n                                    }\n                                }\n                            }catch(e){}\n                            return false;\n                        }\n                        try{\n                            // 1) 링크 클릭 가로채기\n                            document.addEventListener('click', function(ev){\n                                var a = ev.target && ev.target.closest ? ev.target.closest('a[href]') : null;\n                                if(!a) return;\n                                var href = a.getAttribute('href');\n                                if(!href) return;\n                                if(handleKR(href)){\n                                    ev.preventDefault();\n                                    ev.stopPropagation();\n                                }\n                            }, true);\n\n                            // 2) window.open 가로채기\n                            var _open = window.open;\n                            window.open = function(u){\n                                if(handleKR(u)) { return null; }\n                                return _open.apply(window, arguments);\n                            };\n\n                            // 3) location.assign/replace 가로채기\n                            var _assign = window.location.assign.bind(window.location);\n                            window.location.assign = function(u){\n                                if(handleKR(u)) { return; }\n                                _assign(u);\n                            };\n                            var _replace = window.location.replace.bind(window.location);\n                            window.location.replace = function(u){\n                                if(handleKR(u)) { return; }\n                                _replace(u);\n                            };\n                        }catch(_){}\n                    })();\n                ");
			_pcWebView.LoadProgressChanged += delegate
			{
			};
			_pcWebView.LoadFailed += delegate(object s, LoadFailedEventArgs ev)
			{
				VespaDebug.LogError("[WebViewComponent][PC] LoadFailed: code=" + ev.NativeErrorCode + ", url=" + ev.Url);
				if ((string.IsNullOrEmpty(ev.Url) || !ev.Url.StartsWith("kingsraid://", StringComparison.OrdinalIgnoreCase)) && !IsUnknownUrlSchemeError(ev.NativeErrorCode) && _pcRetryCount < 2 && !string.IsNullOrEmpty(ev.Url))
				{
					_pcRetryCount++;
					StartCoroutine(_PcRetry(ev.Url, 0.4f * (float)_pcRetryCount));
				}
			};
			if (!string.IsNullOrEmpty(_pendingUrl) && !_pcInitLoadHandled)
			{
				_pcRetryCount = 0;
				if (LooksLikeHtml(_pendingUrl))
				{
					_pcWebView.LoadHtml(_pendingUrl);
				}
				else
				{
					_pcWebView.LoadUrl(_pendingUrl);
				}
			}
			else if (string.IsNullOrEmpty(_pendingUrl) && !_pcInitLoadHandled)
			{
				_pcWebView.LoadUrl("https://example.com");
			}
		};
		Transform transform = _pcWebView3D.transform.Find("WebViewPrefabResizer");
		if (transform != null)
		{
			Transform transform2 = transform.Find("WebViewPrefabView");
			if (transform2 != null)
			{
				UIWidget component = GameObject.Find("WebView").GetComponent<UIWidget>();
				transform2.localPosition = Vector3.zero;
				transform2.localRotation = Quaternion.identity;
				transform2.localScale = new Vector3((float)component.width / num11, (float)component.height / num12, 1f);
				WebViewPointerBridge component2 = transform2.gameObject.GetComponent<WebViewPointerBridge>();
				if (!component2)
				{
					transform2.GetComponent<MeshRenderer>().sortingOrder = 1;
					component2 = transform2.gameObject.AddComponent<WebViewPointerBridge>();
					Camera cameraByLayer2 = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(PivotWidget.gameObject.layer);
					if (cameraByLayer2 != null)
					{
						component2.inputCamera = cameraByLayer2;
						component2.webViewPrefab = _pcWebView3D;
						component2.webViewMask = LayerMask.GetMask(LayerMask.LayerToName(PivotWidget.gameObject.layer));
						component2.enableDrag = true;
						component2.enableScroll = true;
						component2.scrollNormPerTick = 0.12f;
					}
				}
			}
		}
		_pcWebView3D.gameObject.SetActive(value: true);
	}

	private IEnumerator _PcRetry(string url, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (_pcWebView != null && !string.IsNullOrEmpty(url) && !url.StartsWith("kingsraid://", StringComparison.OrdinalIgnoreCase))
		{
			_pcWebView.LoadUrl(url);
		}
	}

	public void Show()
	{
		if (_pcWebView3D != null)
		{
			_pcWebView3D.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if (_pcWebView3D != null)
		{
			_pcWebView3D.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (_pcBackEnabled && _pcWebView != null && Input.GetKeyDown(KeyCode.Escape))
		{
			_pcWebView.GoBack();
		}
	}

	private void OnPcMessageEmitted(object sender, EventArgs<string> e)
	{
		string value = e.Value;
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		if (value.StartsWith("external:"))
		{
			string text = value.Substring("external:".Length);
			if (!string.IsNullOrEmpty(text))
			{
				Application.OpenURL(text);
			}
		}
		else if (value.StartsWith("kingsraid://", StringComparison.OrdinalIgnoreCase))
		{
			HandleKingsRaidScheme(value);
		}
		else if (value.StartsWith("uniwebview://", StringComparison.OrdinalIgnoreCase))
		{
			OpenUrl(value.Replace("uniwebview", "https"), _listener);
		}
	}

	private void ClosePcWebView()
	{
		SingletonUpdater<Executer>.instance.DelayRun(0.5f, this, delegate
		{
			if (_listener != null && !Vespa_Common_Class_Extension_Method.IsUnityNull(_listener))
			{
				_listener.OnWebViewClose();
			}
		});
	}
}
