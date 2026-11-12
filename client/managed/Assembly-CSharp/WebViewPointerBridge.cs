using UnityEngine;
using Vuplex.WebView;

public class WebViewPointerBridge : MonoBehaviour
{
	[Header("Refs")]
	public Camera inputCamera;

	public WebViewPrefab webViewPrefab;

	public LayerMask webViewMask;

	[Header("Options")]
	public bool enableDrag = true;

	public bool enableScroll = true;

	[Range(0.02f, 0.5f)]
	public float scrollNormPerTick = 0.12f;

	private bool _isDown;

	private Vector2 _lastUV;

	private bool _prevMouseHeld;

	private IWithPointerDownAndUp _cachedDownUp;

	private void Awake()
	{
		if (!inputCamera)
		{
			inputCamera = Camera.main;
		}
		if (!webViewPrefab)
		{
			Debug.LogError("[WebViewPointerBridge] WebViewPrefab 참조 필요");
		}
		Input.imeCompositionMode = IMECompositionMode.On;
	}

	private void Update()
	{
		if (!webViewPrefab || !inputCamera)
		{
			return;
		}
		IWebView webView = webViewPrefab.WebView;
		if (webView == null)
		{
			return;
		}
		IWithPointerDownAndUp withPointerDownAndUp = webView as IWithPointerDownAndUp;
		IWithMovablePointer withMovablePointer = webView as IWithMovablePointer;
		if (withPointerDownAndUp != null)
		{
			_cachedDownUp = withPointerDownAndUp;
		}
		if (Physics.Raycast(inputCamera.ScreenPointToRay(Input.mousePosition), out var hitInfo, 5000f, webViewMask))
		{
			if (!TryGetUv(webViewPrefab, hitInfo, out var uv))
			{
				return;
			}
			_lastUV = uv;
			if (Input.GetMouseButtonDown(0))
			{
				_isDown = true;
				webView.SetFocused(focused: true);
				if (withPointerDownAndUp != null)
				{
					withPointerDownAndUp.PointerDown(uv, new PointerOptions
					{
						Button = MouseButton.Left
					});
					_cachedDownUp = withPointerDownAndUp;
				}
				else
				{
					webView.Click(uv);
				}
			}
			if (_isDown && enableDrag)
			{
				withMovablePointer?.MovePointer(uv);
			}
			if (Input.GetMouseButtonUp(0))
			{
				_isDown = false;
				(withPointerDownAndUp ?? _cachedDownUp)?.PointerUp(uv, new PointerOptions
				{
					Button = MouseButton.Left
				});
			}
			if (enableScroll)
			{
				float num = Input.GetAxis("Mouse ScrollWheel") * -2f;
				if (Mathf.Abs(num) > 0.0001f)
				{
					Vector2 normalizedScrollDelta = new Vector2(0f, num * scrollNormPerTick);
					webView.Scroll(normalizedScrollDelta, uv);
				}
			}
			Input.compositionCursorPos = Input.mousePosition;
		}
		else if (_isDown && Input.GetMouseButtonUp(0))
		{
			_isDown = false;
			((webViewPrefab.WebView as IWithPointerDownAndUp) ?? _cachedDownUp)?.PointerUp(_lastUV, new PointerOptions
			{
				Button = MouseButton.Left
			});
		}
		bool mouseButton = Input.GetMouseButton(0);
		if (_isDown && _prevMouseHeld && !mouseButton)
		{
			_isDown = false;
			((webViewPrefab.WebView as IWithPointerDownAndUp) ?? _cachedDownUp)?.PointerUp(_lastUV, new PointerOptions
			{
				Button = MouseButton.Left
			});
		}
		_prevMouseHeld = mouseButton;
		if (_isDown)
		{
			if (Cursor.lockState != CursorLockMode.None)
			{
				Cursor.lockState = CursorLockMode.None;
			}
			if (!Cursor.visible)
			{
				Cursor.visible = true;
			}
		}
	}

	private bool TryGetUv(WebViewPrefab webViewPrefab, RaycastHit hit, out Vector2 uv)
	{
		uv = Vector2.zero;
		_ = webViewPrefab.transform;
		MeshFilter componentInChildren = webViewPrefab.GetComponentInChildren<MeshFilter>();
		if (!componentInChildren || componentInChildren.sharedMesh == null)
		{
			return false;
		}
		Vector3 vector = componentInChildren.transform.InverseTransformPoint(hit.point);
		Bounds bounds = componentInChildren.sharedMesh.bounds;
		float x = bounds.extents.x;
		float y = bounds.extents.y;
		float num = Mathf.InverseLerp(0f - x, x, vector.x);
		float num2 = Mathf.InverseLerp(0f - y, y, vector.y);
		uv = new Vector2(num, 1f - num2);
		if (num >= 0f && num <= 1f && num2 >= 0f)
		{
			return num2 <= 1f;
		}
		return false;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			ForcePointerUp();
		}
	}

	private void OnDisable()
	{
		ForcePointerUp();
	}

	private void OnDestroy()
	{
		ForcePointerUp();
	}

	private void ForcePointerUp()
	{
		if (_isDown)
		{
			_isDown = false;
			(((webViewPrefab ? webViewPrefab.WebView : null) as IWithPointerDownAndUp) ?? _cachedDownUp)?.PointerUp(_lastUV, new PointerOptions
			{
				Button = MouseButton.Left
			});
		}
	}
}
