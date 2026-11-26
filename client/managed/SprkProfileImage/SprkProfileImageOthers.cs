using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Other players' profile pictures. Portrait call sites (chat, friends, guild,
/// party, rankings, inspect) <see cref="Bind(UITexture, long)"/> their texture
/// to the player's account; a <see cref="SprkProfileImageTarget"/> then shows
/// that player's picture over the native hero icon while one exists.
///
/// Requests are batched: one /list asks which of up to 100 visible players have
/// a picture (most have none) and at which version, then only new or changed
/// pictures are downloaded, one at a time. Answers are reused for
/// <see cref="Fresh"/> seconds, so a changed picture shows within that time.
/// </summary>
public static partial class SprkProfileImage
{
	private sealed class Remote
	{
		public string Version;

		public Texture2D Texture;

		public float CheckedAt = -1f;

		public float UsedAt;

		/// <summary>No download before this time (after a failed one).</summary>
		public float RetryAt;
	}

	private const float Fresh = 300f;

	private const float FailedRetry = 30f;

	private const int MaxList = 100;

	/// <summary>Decoded pictures kept (256 KiB each); least recently shown ones are dropped first.</summary>
	private const int MaxTextures = 64;

	/// <summary>Only pictures shown this recently are downloaded (lists scroll by quickly).</summary>
	private const float Visible = 30f;

	private static readonly Dictionary<long, Remote> remote = new Dictionary<long, Remote>();

	private static readonly HashSet<long> pending = new HashSet<long>();

	private static readonly List<long> downloads = new List<long>();

	private static bool listing;

	private static bool downloading;

	/// <summary>Show this player's picture on the texture while they have one (0: none).</summary>
	public static void Bind(UITexture texture, long accountId)
	{
		if (!(texture == null))
		{
			SprkProfileImageTarget sprkProfileImageTarget = texture.GetComponent<SprkProfileImageTarget>();
			if (sprkProfileImageTarget == null)
			{
				sprkProfileImageTarget = texture.gameObject.AddComponent<SprkProfileImageTarget>();
				sprkProfileImageTarget.Texture = texture;
			}
			sprkProfileImageTarget.Rebind(accountId);
			EnsureRunner();
		}
	}

	public static void Bind(UIBundleLoader loader, long accountId)
	{
		if (loader != null)
		{
			Bind(loader.UiTex, accountId);
		}
	}

	private static long OwnAccountId()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.UserInfo == null)
		{
			return 0L;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
	}

	/// <summary>The player's picture if known; otherwise null, and it is looked up.</summary>
	public static Texture2D PictureFor(long accountId)
	{
		// Hidden in Settings: every portrait keeps its hero icon.
		if (accountId <= 0 || !Shown)
		{
			return null;
		}
		if (accountId == OwnAccountId())
		{
			return picture;
		}
		if (!remote.TryGetValue(accountId, out var value))
		{
			if (session == null)
			{
				return null;
			}
			value = (remote[accountId] = new Remote());
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		value.UsedAt = realtimeSinceStartup;
		if (session == null)
		{
			return (value.Texture != null) ? value.Texture : null;
		}
		if (value.CheckedAt < 0f || realtimeSinceStartup - value.CheckedAt > Fresh)
		{
			pending.Add(accountId);
		}
		if (value.Texture != null)
		{
			return value.Texture;
		}
		// Known to have a picture but not downloaded (skipped while off screen,
		// evicted or failed): fetch it now that it is shown again.
		if (value.Version != null && !(realtimeSinceStartup < value.RetryAt) && !downloads.Contains(accountId))
		{
			downloads.Add(accountId);
		}
		return null;
	}

	/// <summary>Probe fixture: another player's picture as if downloaded (null: none).</summary>
	public static void SetRemotePicture(long accountId, Texture2D texture)
	{
		if (!remote.TryGetValue(accountId, out var value))
		{
			value = (remote[accountId] = new Remote());
		}
		value.Texture = texture;
		value.Version = ((texture != null) ? "fixture" : null);
		value.CheckedAt = Time.realtimeSinceStartup;
	}

	private static void ClearRemote()
	{
		foreach (Remote value in remote.Values)
		{
			if (value.Texture != null)
			{
				UnityEngine.Object.Destroy(value.Texture);
			}
		}
		remote.Clear();
		pending.Clear();
		downloads.Clear();
	}

	private static void TickRemote()
	{
		if (!listing && pending.Count > 0)
		{
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
			ListVersions(list);
		}
		if (downloading)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (downloads.Count > 0)
		{
			long num = downloads[0];
			downloads.RemoveAt(0);
			if (remote.TryGetValue(num, out var value) && !(realtimeSinceStartup - value.UsedAt > Visible))
			{
				Download(num, value);
				break;
			}
		}
	}

	private static void ListVersions(List<long> ids)
	{
		listing = true;
		string text = "[" + string.Join(",", ids) + "]";
		Start("list", "&AccountIds=" + UnityWebRequest.EscapeURL(text), delegate(IDictionary reply)
		{
			IDictionary dictionary = reply["Versions"] as IDictionary;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (long id in ids)
			{
				if (!remote.TryGetValue(id, out var value))
				{
					continue;
				}
				value.CheckedAt = realtimeSinceStartup;
				string text2 = ((dictionary != null) ? (dictionary[id.ToString()] as string) : null);
				if (string.IsNullOrEmpty(text2))
				{
					Drop(value);
				}
				else if (text2 != value.Version || value.Texture == null)
				{
					value.Version = text2;
					if (!downloads.Contains(id))
					{
						downloads.Add(id);
					}
				}
			}
		}, delegate(Outcome outcome)
		{
			listing = false;
			if (outcome == Outcome.Success)
			{
				return;
			}
			// Retry these players later instead of every frame.
			float checkedAt = Time.realtimeSinceStartup - Fresh + FailedRetry;
			foreach (long id2 in ids)
			{
				if (remote.TryGetValue(id2, out var value2))
				{
					value2.CheckedAt = checkedAt;
				}
			}
		}, own: false);
	}

	private static void Download(long id, Remote entry)
	{
		downloading = true;
		Start("get", "&AccountId=" + id, delegate(IDictionary reply)
		{
			string text = reply["Image"] as string;
			Texture2D texture2D = (string.IsNullOrEmpty(text) ? null : Decode(Convert.FromBase64String(text)));
			if (remote.TryGetValue(id, out var value) && value == entry)
			{
				if (value.Texture != null)
				{
					UnityEngine.Object.Destroy(value.Texture);
				}
				value.Texture = texture2D;
				value.Version = ((texture2D != null) ? (reply["Version"] as string) : null);
				Evict();
			}
			else if (texture2D != null)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
		}, delegate(Outcome outcome)
		{
			downloading = false;
			if (outcome != Outcome.Success)
			{
				entry.RetryAt = Time.realtimeSinceStartup + FailedRetry;
			}
		}, own: false);
	}

	private static void Drop(Remote entry)
	{
		if (entry.Texture != null)
		{
			UnityEngine.Object.Destroy(entry.Texture);
		}
		entry.Texture = null;
		entry.Version = null;
	}

	private static void Evict()
	{
		int num = 0;
		foreach (Remote value in remote.Values)
		{
			if (value.Texture != null)
			{
				num++;
			}
		}
		while (num > MaxTextures)
		{
			Remote remote2 = null;
			foreach (Remote value2 in remote.Values)
			{
				if (value2.Texture != null && (remote2 == null || value2.UsedAt < remote2.UsedAt))
				{
					remote2 = value2;
				}
			}
			if (remote2 == null)
			{
				break;
			}
			// Forget it entirely: shown again later, it is listed and downloaded anew.
			Drop(remote2);
			remote2.CheckedAt = -1f;
			num--;
		}
	}
}

/// <summary>
/// Shows a player's picture over the native portrait. The native code keeps
/// loading the hero icon into the texture as before; this remembers that icon
/// and puts it back when the player has no picture or the slot shows someone else.
/// </summary>
public sealed class SprkProfileImageTarget : MonoBehaviour
{
	public UITexture Texture;

	public long AccountId;

	private Texture native;

	private Rect nativeUv = new Rect(0f, 0f, 1f, 1f);

	private Texture2D shown;

	private bool showing;

	internal void Rebind(long accountId)
	{
		if (accountId != AccountId)
		{
			AccountId = accountId;
			Restore();
		}
	}

	private void LateUpdate()
	{
		if (Texture == null)
		{
			return;
		}
		Texture mainTexture = Texture.mainTexture;
		if (!showing || (object)mainTexture != shown)
		{
			// Nothing of ours is shown: whatever is there is the native icon.
			if (showing)
			{
				showing = false;
				shown = null;
			}
			native = mainTexture;
			nativeUv = Texture.uvRect;
		}
		Texture2D texture2D = SprkProfileImage.PictureFor(AccountId);
		if (texture2D != null)
		{
			if (!showing || (object)mainTexture != texture2D)
			{
				Texture.mainTexture = texture2D;
				Texture.uvRect = new Rect(0f, 0f, 1f, 1f);
				shown = texture2D;
				showing = true;
			}
		}
		else
		{
			Restore();
		}
	}

	private void OnDisable()
	{
		Restore();
	}

	private void Restore()
	{
		if (showing)
		{
			showing = false;
			if (Texture != null && (object)Texture.mainTexture == shown)
			{
				Texture.mainTexture = native;
				Texture.uvRect = nativeUv;
			}
			shown = null;
		}
	}
}
