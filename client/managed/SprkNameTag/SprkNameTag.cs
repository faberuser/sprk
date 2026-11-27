using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using NVespa;
using NVespa.NGlobal;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Name tags: Discord-style badges (an optional small icon and a short text in
/// a coloured pill) after a player's name, on every screen and in chat. GMs
/// manage them on the server (gm.py name-tag).
///
/// The server prefixes tagged players' nicks with an invisible marker,
/// <see cref="MarkStart"/> tag id <see cref="MarkEnd"/>, wherever a name is
/// sent. <see cref="UILabel"/>'s text setter passes every text through
/// <see cref="OnText"/>, which removes the marker and, when the label shows the
/// name, attaches a <see cref="SprkNameTagBadge"/>. Each tag's look is fetched
/// once (batched) from /sprk/name_tag/get and refreshed every
/// <see cref="Fresh"/> seconds.
/// </summary>
public static class SprkNameTag
{
	public const char MarkStart = '';

	public const char MarkEnd = '';

	/// <summary>Decoded icons are shrunk to at most this many pixels per side.</summary>
	public const int MaxIconSide = 64;

	private const float Fresh = 300f;

	private const float FailedRetry = 30f;

	private const int MaxList = 100;

	public sealed class Tag
	{
		public string Label = string.Empty;

		public Color TextColor = Color.white;

		public Color BackgroundColor = new Color32(43, 45, 49, byte.MaxValue);

		public Texture2D Icon;

		public string Version;

		/// <summary>False until the server answered: nothing is drawn meanwhile.</summary>
		public bool Known;

		/// <summary>The server has no such tag (deleted): the plain name is shown.</summary>
		public bool Missing;

		public float CheckedAt = -1f;
	}

	private static readonly Dictionary<long, Tag> tags = new Dictionary<long, Tag>();

	private static readonly HashSet<long> pending = new HashSet<long>();

	private static SprkNameTagRunner runner;

	private static string session;

	private static bool busy;

	/// <summary>Bumped whenever a tag's look changes, so badges lay out again.</summary>
	public static int Revision { get; private set; }

	/// <summary>Set while a badge writes its own label's text (padding), which passes unchanged.</summary>
	internal static bool Writing;

	/// <summary>
	/// Called by <see cref="UILabel"/> before it stores new text. Returns the
	/// text without markers, plus trailing spaces that make room for the badge
	/// (so whatever is anchored after the name moves along); shows or hides
	/// the label's badge.
	/// </summary>
	public static string OnText(UILabel label, string value)
	{
		if (Writing)
		{
			return value;
		}
		if (string.IsNullOrEmpty(value) || (value.IndexOf(MarkStart) < 0 && value.IndexOf(MarkEnd) < 0))
		{
			Hide(label);
			return value;
		}
		long tag = 0L;
		string text = value;
		// A name, optionally wrapped in NGUI colour/style markup ("[ff0000]Name[-]").
		int num = SkipMarkup(value);
		if (num < value.Length && value[num] == MarkStart)
		{
			int num2 = value.IndexOf(MarkEnd, num + 1);
			if (num2 > num + 1 && long.TryParse(value.Substring(num + 1, num2 - num - 1), out var result) && result > 0)
			{
				tag = result;
				text = value.Substring(0, num) + value.Substring(num2 + 1);
			}
		}
		text = Strip(text);
		// Only labels that show the name itself get a badge; a name inside a
		// sentence ("X has left the party") is shown plain.
		if (tag > 0 && text.Length > 0 && text.IndexOf('\n') < 0 && label != null)
		{
			SprkNameTagBadge sprkNameTagBadge = label.SprkNameTagBadge as SprkNameTagBadge;
			if (sprkNameTagBadge == null)
			{
				sprkNameTagBadge = label.gameObject.AddComponent<SprkNameTagBadge>();
				sprkNameTagBadge.Label = label;
				label.SprkNameTagBadge = sprkNameTagBadge;
			}
			return sprkNameTagBadge.Show(tag, text);
		}
		Hide(label);
		return text;
	}

	private static void Hide(UILabel label)
	{
		if (label != null && label.SprkNameTagBadge is SprkNameTagBadge sprkNameTagBadge && sprkNameTagBadge != null)
		{
			sprkNameTagBadge.Show(0L, null);
		}
	}

	private static int SkipMarkup(string value)
	{
		int i = 0;
		while (i < value.Length && value[i] == '[')
		{
			int num = value.IndexOf(']', i + 1);
			if (num < 0 || num - i > 10)
			{
				break;
			}
			i = num + 1;
		}
		return i;
	}

	/// <summary>The text without name-tag markers (for anything that is not a label).</summary>
	public static string Strip(string value)
	{
		if (string.IsNullOrEmpty(value) || (value.IndexOf(MarkStart) < 0 && value.IndexOf(MarkEnd) < 0))
		{
			return value;
		}
		StringBuilder stringBuilder = new StringBuilder(value.Length);
		bool flag = false;
		foreach (char c in value)
		{
			if (c == MarkStart)
			{
				flag = true;
			}
			else if (c == MarkEnd)
			{
				flag = false;
			}
			else if (!flag)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// A nick change sets the new name as typed; keep the player's tag marker
	/// so their own badge stays until the next login refreshes it.
	/// </summary>
	public static string Carry(string previous, string next)
	{
		if (string.IsNullOrEmpty(next) || string.IsNullOrEmpty(previous) || next.IndexOf(MarkStart) >= 0 || previous[0] != MarkStart)
		{
			return next;
		}
		int num = previous.IndexOf(MarkEnd);
		return (num > 0) ? (previous.Substring(0, num + 1) + next) : next;
	}

	/// <summary>The tag's look once known; otherwise null, and it is looked up.</summary>
	public static Tag Get(long id)
	{
		if (id <= 0)
		{
			return null;
		}
		EnsureRunner();
		if (!tags.TryGetValue(id, out var value))
		{
			value = (tags[id] = new Tag());
		}
		if (session != null && (value.CheckedAt < 0f || Time.realtimeSinceStartup - value.CheckedAt > Fresh))
		{
			pending.Add(id);
		}
		return (value.Known && !value.Missing) ? value : null;
	}

	/// <summary>True once the server answered for this tag (it may have said: deleted).</summary>
	public static bool Known(long id)
	{
		Tag value;
		return tags.TryGetValue(id, out value) && value.Known;
	}

	/// <summary>Probe fixture: a tag as if the server had sent it.</summary>
	public static void SetFixture(long id, string label, Color text, Color background, Texture2D icon)
	{
		if (!tags.TryGetValue(id, out var value))
		{
			value = (tags[id] = new Tag());
		}
		value.Label = label;
		value.TextColor = text;
		value.BackgroundColor = background;
		value.Icon = icon;
		value.Known = true;
		value.Missing = false;
		value.Version = "fixture";
		value.CheckedAt = float.MaxValue;
		Revision++;
	}

	private static void EnsureRunner()
	{
		if (runner == null && Application.isPlaying)
		{
			GameObject gameObject = new GameObject("SprkNameTag");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			runner = gameObject.AddComponent<SprkNameTagRunner>();
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
			// Another account or server: forget answers, keep fixtures.
			session = text;
			foreach (Tag value in tags.Values)
			{
				if (value.Version != "fixture")
				{
					value.CheckedAt = -1f;
				}
			}
			pending.Clear();
		}
		if (text == null || busy || pending.Count == 0)
		{
			return;
		}
		List<long> list = new List<long>();
		foreach (long item in pending)
		{
			list.Add(item);
			if (list.Count == MaxList)
			{
				break;
			}
		}
		foreach (long item2 in list)
		{
			pending.Remove(item2);
		}
		runner.StartCoroutine(Fetch(webServiceRequester.Host, text, list));
	}

	private static IEnumerator Fetch(string host, string key, List<long> ids)
	{
		busy = true;
		bool flag = false;
		try
		{
			string s = "SessionKey=" + UnityWebRequest.EscapeURL(key) + "&TagIds=" + UnityWebRequest.EscapeURL("[" + string.Join(",", ids) + "]");
			using UnityWebRequest web = new UnityWebRequest(host.TrimEnd('/') + "/sprk/name_tag/get", "POST");
			web.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
			web.downloadHandler = new DownloadHandlerBuffer();
			web.timeout = 30;
			web.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			yield return web.SendWebRequest();
			// The marker tells these answers from an older server's generic fallback.
			if (web.result == UnityWebRequest.Result.Success && key == session && Json.Deserialize(web.downloadHandler.text) is IDictionary dictionary && dictionary["SprkNameTag"] != null && dictionary["Result"] as string == "Success")
			{
				flag = true;
				Apply(ids, dictionary["Tags"] as IDictionary);
			}
		}
		finally
		{
			busy = false;
			if (!flag)
			{
				float checkedAt = Time.realtimeSinceStartup - Fresh + FailedRetry;
				foreach (long id in ids)
				{
					if (tags.TryGetValue(id, out var value))
					{
						value.CheckedAt = checkedAt;
					}
				}
			}
		}
	}

	private static void Apply(List<long> ids, IDictionary answer)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (long id in ids)
		{
			if (!tags.TryGetValue(id, out var value))
			{
				continue;
			}
			value.CheckedAt = realtimeSinceStartup;
			IDictionary dictionary = ((answer != null) ? (answer[id.ToString()] as IDictionary) : null);
			if (dictionary == null)
			{
				if (!value.Missing || !value.Known)
				{
					value.Known = true;
					value.Missing = true;
					SetIcon(value, null);
					Revision++;
				}
				continue;
			}
			string text = dictionary["Version"] as string;
			if (value.Known && !value.Missing && text == value.Version)
			{
				continue;
			}
			value.Version = text;
			value.Label = (dictionary["Label"] as string) ?? string.Empty;
			value.TextColor = ParseColor(dictionary["TextColor"] as string, Color.white);
			value.BackgroundColor = ParseColor(dictionary["BackgroundColor"] as string, new Color32(43, 45, 49, byte.MaxValue));
			string text2 = dictionary["Icon"] as string;
			Texture2D icon = null;
			if (!string.IsNullOrEmpty(text2))
			{
				try
				{
					icon = DecodeIcon(Convert.FromBase64String(text2));
				}
				catch (FormatException)
				{
				}
			}
			SetIcon(value, icon);
			value.Known = true;
			value.Missing = false;
			Revision++;
		}
	}

	private static void SetIcon(Tag tag, Texture2D icon)
	{
		if (tag.Icon != null && tag.Icon != icon)
		{
			UnityEngine.Object.Destroy(tag.Icon);
		}
		tag.Icon = icon;
	}

	private static Color ParseColor(string hex, Color fallback)
	{
		if (hex != null && hex.Length == 6 && ColorUtility.TryParseHtmlString("#" + hex, out var color))
		{
			return color;
		}
		return fallback;
	}

	/// <summary>PNG to a texture of at most <see cref="MaxIconSide"/> px per side (aspect kept), or null.</summary>
	public static Texture2D DecodeIcon(byte[] png)
	{
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
		if (png == null || !texture2D.LoadImage(png))
		{
			UnityEngine.Object.Destroy(texture2D);
			return null;
		}
		int width = texture2D.width;
		int height = texture2D.height;
		if (width <= MaxIconSide && height <= MaxIconSide)
		{
			return Finish(texture2D);
		}
		float num = (float)MaxIconSide / (float)Mathf.Max(width, height);
		int num2 = Mathf.Max(1, Mathf.RoundToInt((float)width * num));
		int num3 = Mathf.Max(1, Mathf.RoundToInt((float)height * num));
		texture2D.filterMode = FilterMode.Bilinear;
		RenderTexture temporary = RenderTexture.GetTemporary(num2, num3, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		RenderTexture active = RenderTexture.active;
		try
		{
			Graphics.Blit(texture2D, temporary);
			RenderTexture.active = temporary;
			Texture2D texture2D2 = new Texture2D(num2, num3, TextureFormat.RGBA32, mipChain: false);
			texture2D2.ReadPixels(new Rect(0f, 0f, num2, num3), 0, 0);
			texture2D2.Apply();
			return Finish(texture2D2);
		}
		finally
		{
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			UnityEngine.Object.Destroy(texture2D);
		}
	}

	private static Texture2D Finish(Texture2D texture)
	{
		texture.name = "SPRK name tag icon";
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Bilinear;
		texture.hideFlags = HideFlags.HideAndDontSave;
		texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		return texture;
	}

	private static Texture2D pill;

	/// <summary>White rounded rectangle, drawn sliced and tinted with the tag's colour.</summary>
	public static Texture2D Pill
	{
		get
		{
			if (pill != null)
			{
				return pill;
			}
			const int size = 24;
			const float radius = 8f;
			pill = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
			pill.name = "SPRK name tag pill";
			pill.wrapMode = TextureWrapMode.Clamp;
			pill.filterMode = FilterMode.Bilinear;
			pill.hideFlags = HideFlags.HideAndDontSave;
			Color32[] array = new Color32[size * size];
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					float num = Mathf.Max(Mathf.Max(radius - (j + 0.5f), j + 0.5f - (size - radius)), 0f);
					float num2 = Mathf.Max(Mathf.Max(radius - (i + 0.5f), i + 0.5f - (size - radius)), 0f);
					float num3 = Mathf.Sqrt(num * num + num2 * num2);
					byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius - num3 + 0.5f) * 255f);
					array[i * size + j] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a);
				}
			}
			pill.SetPixels32(array);
			pill.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			return pill;
		}
	}

	public const int PillBorder = 8;
}

public sealed class SprkNameTagRunner : MonoBehaviour
{
	private void Update()
	{
		SprkNameTag.Tick();
	}
}
