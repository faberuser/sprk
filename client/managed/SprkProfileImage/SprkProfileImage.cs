using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniJSON;
using NGame2.NUI;
using NVespa;
using NVespa.NGlobal;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Custom profile pictures. A player may upload one picture (stored by the
/// server, /sprk/profile_image/*), shown instead of the leader hero's icon
/// wherever a loader is <see cref="Watch"/>ed: the lobby profile icon and the
/// My Info "Profile" tab. The leader hero itself is never changed.
/// </summary>
public static partial class SprkProfileImage
{
	/// <summary>Uploaded pictures are cropped to a centred square of this size.</summary>
	public const int Side = 256;

	public const int MinSourceSide = 16;

	public const int MaxFileBytes = 20 * 1024 * 1024;

	private const float RetryDelay = 30f;

	public enum Outcome
	{
		Success,
		Cancelled,
		Unreadable,
		Rejected,
		Unsupported
	}

	private static readonly HashSet<UIBundleLoader> watched = new HashSet<UIBundleLoader>();

	private static SprkProfileImageRunner runner;

	private static Texture2D picture;

	private static string session;

	private static bool loaded;

	private static bool busy;

	private static float retryAt;

	/// <summary>Raised when the picture or the busy state changes.</summary>
	public static event Action Changed;

	/// <summary>The current account's picture, or null when it has none (or it is not loaded yet).</summary>
	public static Texture2D Picture => picture;

	public static bool Loaded => loaded;

	public static bool Busy => busy;

	public static bool CanPickFiles
	{
		get
		{
			if (Application.platform != RuntimePlatform.WindowsPlayer)
			{
				return Application.platform == RuntimePlatform.WindowsEditor;
			}
			return true;
		}
	}

	/// <summary>Show the custom picture on this loader whenever there is one.</summary>
	public static void Watch(UIBundleLoader loader)
	{
		if (loader == null)
		{
			return;
		}
		watched.RemoveWhere((UIBundleLoader l) => l == null);
		watched.Add(loader);
		EnsureRunner();
		if (picture != null)
		{
			loader.SprkReload();
		}
	}

	public static void Unwatch(UIBundleLoader loader)
	{
		if (loader != null && watched.Remove(loader))
		{
			loader.SprkReload();
		}
	}

	/// <summary>Called by <see cref="UIBundleLoader"/> before it loads its asset.</summary>
	public static bool TryApply(UIBundleLoader loader)
	{
		if (picture == null || loader == null || !watched.Contains(loader))
		{
			return false;
		}
		UITexture uiTex = loader.UiTex;
		if (uiTex != null)
		{
			uiTex.material = null;
			uiTex.mainTexture = picture;
			uiTex.uvRect = new Rect(0f, 0f, 1f, 1f);
		}
		return true;
	}

	private static void EnsureRunner()
	{
		if (!(runner != null) && Application.isPlaying)
		{
			GameObject gameObject = new GameObject("SprkProfileImage");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			runner = gameObject.AddComponent<SprkProfileImageRunner>();
		}
	}

	/// <summary>The persistent object running SPRK profile picture coroutines.</summary>
	public static MonoBehaviour Runner
	{
		get
		{
			EnsureRunner();
			return runner;
		}
	}

	private static WebServiceRequester Requester()
	{
		if (!SingletonUpdater<WebServiceManager>.isValidInstance)
		{
			return null;
		}
		return SingletonUpdater<WebServiceManager>.instance.Requester;
	}

	/// <summary>Follows the logged-in session: a new session (re)loads that account's picture.</summary>
	internal static void Tick()
	{
		WebServiceRequester webServiceRequester = Requester();
		string text = webServiceRequester?.SessionKey;
		if (string.IsNullOrEmpty(text))
		{
			text = null;
		}
		if (text != session)
		{
			session = text;
			loaded = false;
			retryAt = 0f;
			if (picture != null)
			{
				SetPicture(null);
			}
			ClearRemote();
		}
		if (text != null && !loaded && !busy && !(Time.realtimeSinceStartup < retryAt))
		{
			Fetch();
		}
		if (text != null)
		{
			TickRemote();
		}
	}

	private static void Fetch()
	{
		string key = session;
		Start("get", string.Empty, delegate(IDictionary reply)
		{
			loaded = true;
			string text = reply["Image"] as string;
			SetPicture(string.IsNullOrEmpty(text) ? null : Decode(Convert.FromBase64String(text)));
		}, delegate(Outcome outcome)
		{
			if (outcome != Outcome.Success && key == session)
			{
				retryAt = Time.realtimeSinceStartup + RetryDelay;
			}
		});
	}

	/// <summary>Ask for a picture file, crop and scale it, and upload it.</summary>
	public static void Upload(Action<Outcome> done)
	{
		if (busy || session == null)
		{
			Finish(done, Outcome.Rejected);
			return;
		}
		if (!CanPickFiles)
		{
			Finish(done, Outcome.Unsupported);
			return;
		}
		string text = SprkProfileImagePicker.PickWindows(SprkText.Get("Profile Picture"));
		if (text == null)
		{
			Finish(done, Outcome.Cancelled);
			return;
		}
		byte[] png = Prepare(text);
		if (png == null)
		{
			Finish(done, Outcome.Unreadable);
			return;
		}
		Start("set", "&Image=" + UnityWebRequest.EscapeURL(Convert.ToBase64String(png)), delegate
		{
			loaded = true;
			SetPicture(Decode(png));
		}, done);
	}

	public static void Remove(Action<Outcome> done)
	{
		if (busy || session == null)
		{
			Finish(done, Outcome.Rejected);
			return;
		}
		Start("remove", string.Empty, delegate
		{
			SetPicture(null);
		}, done);
	}

	private static void Finish(Action<Outcome> done, Outcome outcome)
	{
		if (done != null)
		{
			done(outcome);
		}
	}

	private static byte[] Prepare(string path)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			if (!fileInfo.Exists || fileInfo.Length <= 0 || fileInfo.Length > MaxFileBytes)
			{
				return null;
			}
			return Crop(File.ReadAllBytes(path));
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
	}

	/// <summary>PNG/JPG bytes to a <see cref="Side"/>-pixel square PNG (centred crop), or null.</summary>
	public static byte[] Crop(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true);
		try
		{
			if (!texture2D.LoadImage(data) || texture2D.width < MinSourceSide || texture2D.height < MinSourceSide)
			{
				return null;
			}
			texture2D.filterMode = FilterMode.Trilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			int num = Mathf.Min(texture2D.width, texture2D.height);
			Vector2 scale = new Vector2((float)num / (float)texture2D.width, (float)num / (float)texture2D.height);
			Vector2 offset = new Vector2((1f - scale.x) * 0.5f, (1f - scale.y) * 0.5f);
			RenderTexture temporary = RenderTexture.GetTemporary(Side, Side, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			RenderTexture active = RenderTexture.active;
			Texture2D texture2D2 = null;
			try
			{
				Graphics.Blit(texture2D, temporary, scale, offset);
				RenderTexture.active = temporary;
				texture2D2 = new Texture2D(Side, Side, TextureFormat.RGBA32, mipChain: false);
				texture2D2.ReadPixels(new Rect(0f, 0f, Side, Side), 0, 0);
				texture2D2.Apply();
				return texture2D2.EncodeToPNG();
			}
			finally
			{
				RenderTexture.active = active;
				RenderTexture.ReleaseTemporary(temporary);
				if (texture2D2 != null)
				{
					UnityEngine.Object.Destroy(texture2D2);
				}
			}
		}
		finally
		{
			UnityEngine.Object.Destroy(texture2D);
		}
	}

	private static Texture2D Decode(byte[] png)
	{
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
		if (!texture2D.LoadImage(png, markNonReadable: true))
		{
			UnityEngine.Object.Destroy(texture2D);
			return null;
		}
		texture2D.name = "SPRK profile picture";
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.filterMode = FilterMode.Bilinear;
		texture2D.hideFlags = HideFlags.HideAndDontSave;
		return texture2D;
	}

	/// <summary>Replace the shown picture (null: back to the leader hero icon). Probes use it for fixtures.</summary>
	public static void SetPicture(Texture2D texture)
	{
		Texture2D texture2D = picture;
		picture = texture;
		foreach (UIBundleLoader item in new List<UIBundleLoader>(watched))
		{
			if (item != null)
			{
				item.SprkReload();
			}
		}
		if (texture2D != null && texture2D != texture)
		{
			UnityEngine.Object.Destroy(texture2D);
		}
		RaiseChanged();
	}

	private static void RaiseChanged()
	{
		if (SprkProfileImage.Changed != null)
		{
			SprkProfileImage.Changed();
		}
	}

	/// <summary>`own`: a request about the player's own picture, shown as busy in the Profile tab.</summary>
	private static void Start(string action, string fields, Action<IDictionary> onSuccess, Action<Outcome> done, bool own = true)
	{
		EnsureRunner();
		WebServiceRequester webServiceRequester = Requester();
		if (runner == null || webServiceRequester == null || session == null)
		{
			Finish(done, Outcome.Rejected);
			return;
		}
		runner.StartCoroutine(Send(webServiceRequester.Host, session, action, fields, onSuccess, done, own));
	}

	private static IEnumerator Send(string host, string key, string action, string fields, Action<IDictionary> onSuccess, Action<Outcome> done, bool own)
	{
		if (own)
		{
			busy = true;
			RaiseChanged();
		}
		Outcome outcome = Outcome.Rejected;
		try
		{
			using UnityWebRequest web = new UnityWebRequest(host.TrimEnd('/') + "/sprk/profile_image/" + action, "POST");
			web.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("SessionKey=" + UnityWebRequest.EscapeURL(key) + fields));
			web.downloadHandler = new DownloadHandlerBuffer();
			web.timeout = 30;
			web.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			yield return web.SendWebRequest();
			// The marker distinguishes these routes from the server's generic
			// fallback, which answers unknown requests (an older server) with Success.
			if (web.result == UnityWebRequest.Result.Success && key == session && Json.Deserialize(web.downloadHandler.text) is IDictionary dictionary && dictionary["SprkProfileImage"] != null && dictionary["Result"] as string == "Success")
			{
				onSuccess?.Invoke(dictionary);
				outcome = Outcome.Success;
			}
		}
		finally
		{
			if (own)
			{
				busy = false;
				RaiseChanged();
			}
			Finish(done, outcome);
		}
	}
}

public sealed class SprkProfileImageRunner : MonoBehaviour
{
	private void Update()
	{
		SprkProfileImage.Tick();
	}
}
