using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Adds a Donate button next to the member count of the guild Construction
/// window. It opens a choice of Wood, Stone and Metal; any member trades
/// Rubies for the chosen guild material through guild/sprk_donate_construction
/// (server rule GuildDonateConstruction).
/// </summary>
public sealed class SprkGuildDonate : MonoBehaviour
{
	// Keep in step with server/tables/ArenaGuildRules.json GuildDonateConstruction.
	// The server refuses a request whose Gem does not match its rule.
	public const int GemCost = 1000;

	public const int Amount = 1000;

	private const string ButtonName = "SprkGuildDonate";

	private const float Gap = 16f;

	private const float ChoiceWidth = 150f;

	private const float ChoiceGap = 12f;

	private static readonly GuildConstructionResource[] Resources = new GuildConstructionResource[3]
	{
		GuildConstructionResource.Wood,
		GuildConstructionResource.Stone,
		GuildConstructionResource.Metal
	};

	private GuildConstruction window;

	private GameObject buttonRoot;

	private UIButton button;

	private readonly List<GameObject> choices = new List<GameObject>();

	private bool requesting;

	private bool choosing;

	public GameObject ButtonRoot => buttonRoot;

	public IList<GameObject> Choices => choices;

	public bool Choosing => choosing;

	/// <summary>The material the last confirm popup offered (probe).</summary>
	public GuildConstructionResource? Offered { get; private set; }

	public static void Attach(GuildConstruction window, bool show)
	{
		if (window == null)
		{
			return;
		}
		SprkGuildDonate sprkGuildDonate = window.GetComponent<SprkGuildDonate>();
		if (sprkGuildDonate == null)
		{
			sprkGuildDonate = window.gameObject.AddComponent<SprkGuildDonate>();
		}
		sprkGuildDonate.window = window;
		// The member count shows on both tabs, and so does the button.
		sprkGuildDonate.Place();
	}

	private void OnDisable()
	{
		// Reopening the window starts with the choices closed.
		choosing = false;
		foreach (GameObject choice in choices)
		{
			if (choice != null)
			{
				choice.SetActive(value: false);
			}
		}
	}

	private Transform MemberBox()
	{
		if (window == null || window.Label_NumMember == null)
		{
			return null;
		}
		return window.Label_NumMember.transform.parent;
	}

	public void Place()
	{
		Transform box = MemberBox();
		if (box == null || !EnsureButton(box.parent))
		{
			return;
		}
		Transform parent = box.parent;
		// Right edge of the button meets the member box's left edge, centred on it.
		Bounds boxBounds = NGUIMath.CalculateRelativeWidgetBounds(parent, box, considerInactive: false);
		float width = Width(buttonRoot);
		float x = boxBounds.min.x - Gap - width / 2f;
		SetPosition(buttonRoot, x, boxBounds.center.y);
		for (int i = 0; i < choices.Count; i++)
		{
			// Choices run leftwards from the Donate button: Metal nearest, Wood farthest,
			// so they read Wood, Stone, Metal from left to right.
			int fromRight = choices.Count - i;
			float cx = x - width / 2f - Gap - ChoiceWidth / 2f - (fromRight - 1) * (ChoiceWidth + ChoiceGap);
			SetPosition(choices[i], cx, boxBounds.center.y);
			if (choices[i].activeSelf != choosing)
			{
				choices[i].SetActive(choosing);
			}
			UIHelper.EnableButton(choices[i].GetComponent<UIButton>(), !requesting, grayscaleIfDisable: true);
		}
		if (!buttonRoot.activeSelf)
		{
			buttonRoot.SetActive(value: true);
		}
		UIHelper.EnableButton(button, !requesting, grayscaleIfDisable: true);
	}

	private static float Width(GameObject root)
	{
		UIWidget widget = root.GetComponent<UIWidget>();
		if (widget != null)
		{
			return widget.width;
		}
		return NGUIMath.CalculateRelativeWidgetBounds(root.transform, root.transform, considerInactive: true).size.x;
	}

	private static void SetPosition(GameObject root, float x, float y)
	{
		Vector3 target = new Vector3(Mathf.Round(x), Mathf.Round(y), 0f);
		if (root.transform.localPosition != target)
		{
			root.transform.localPosition = target;
		}
	}

	private bool EnsureButton(Transform parent)
	{
		if (buttonRoot != null)
		{
			return true;
		}
		// Clone a progress row's blue Max button so the style matches the window.
		ProgressComponent progress = (window.Grid_ProgressBars == null) ? null : window.Grid_ProgressBars.GetComponentInChildren<ProgressComponent>(includeInactive: true);
		if (progress == null || progress.Button_Max == null)
		{
			return false;
		}
		buttonRoot = Clone(progress.Button_Max.gameObject, parent, ButtonName, SprkText.Get("Donate"), out button);
		EventDelegate.Add(button.onClick, OnClick);
		foreach (GuildConstructionResource resource in Resources)
		{
			GuildConstructionResource picked = resource;
			GameObject choice = Clone(progress.Button_Max.gameObject, parent, ButtonName + "_" + resource, ResourceName(resource), out UIButton choiceButton);
			UIWidget widget = choice.GetComponent<UIWidget>();
			if (widget != null)
			{
				widget.width = (int)ChoiceWidth;
			}
			UILabel choiceLabel = choice.GetComponentInChildren<UILabel>(includeInactive: true);
			if (choiceLabel != null)
			{
				choiceLabel.width = (int)ChoiceWidth - 24;
			}
			foreach (BoxCollider collider in choice.GetComponentsInChildren<BoxCollider>(includeInactive: true))
			{
				collider.size = new Vector3(ChoiceWidth, collider.size.y, collider.size.z);
			}
			EventDelegate.Add(choiceButton.onClick, delegate
			{
				Pick(picked);
			});
			choice.SetActive(value: false);
			choices.Add(choice);
		}
		RaiseDepth(parent);
		return true;
	}

	private static GameObject Clone(GameObject template, Transform parent, string name, string text, out UIButton cloneButton)
	{
		GameObject clone = UnityEngine.Object.Instantiate(template, parent, worldPositionStays: false);
		clone.name = name;
		foreach (MonoBehaviour behaviour in clone.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
		{
			if (behaviour.GetType().Name == "UILocalizeEx")
			{
				UnityEngine.Object.DestroyImmediate(behaviour);
			}
		}
		foreach (UIWidget widget in clone.GetComponentsInChildren<UIWidget>(includeInactive: true))
		{
			widget.leftAnchor.target = null;
			widget.rightAnchor.target = null;
			widget.topAnchor.target = null;
			widget.bottomAnchor.target = null;
		}
		UILabel label = clone.GetComponentInChildren<UILabel>(includeInactive: true);
		if (label != null)
		{
			label.overflowMethod = UILabel.Overflow.ShrinkContent;
			label.text = text;
		}
		cloneButton = clone.GetComponent<UIButton>();
		if (cloneButton == null)
		{
			cloneButton = clone.GetComponentInChildren<UIButton>(includeInactive: true);
		}
		cloneButton.onClick.Clear();
		NGUITools.SetLayer(clone, parent.gameObject.layer);
		NGUITools.MarkParentAsChanged(clone);
		return clone;
	}

	/// <summary>Draw the buttons above the member box and the window header.</summary>
	private void RaiseDepth(Transform parent)
	{
		int top = 0;
		Transform box = MemberBox();
		foreach (UIWidget widget in box.GetComponentsInChildren<UIWidget>(includeInactive: true))
		{
			top = System.Math.Max(top, widget.depth);
		}
		List<GameObject> roots = new List<GameObject>(choices) { buttonRoot };
		foreach (GameObject root in roots)
		{
			UIWidget[] own = root.GetComponentsInChildren<UIWidget>(includeInactive: true);
			int lowest = int.MaxValue;
			foreach (UIWidget widget in own)
			{
				lowest = System.Math.Min(lowest, widget.depth);
			}
			foreach (UIWidget widget in own)
			{
				widget.depth += top - lowest + 1;
			}
		}
	}

	private static string ResourceName(GuildConstructionResource resource)
	{
		return LocalizationManager.GetStringDirect("GUILD_MATERIAL_" + resource.ToString().ToUpperInvariant());
	}

	public void OnClick()
	{
		if (requesting)
		{
			return;
		}
		choosing = !choosing;
		Place();
	}

	public void Pick(GuildConstructionResource resource)
	{
		if (requesting)
		{
			return;
		}
		choosing = false;
		Place();
		Offered = resource;
		string message = SprkText.Format("Donate {0} Rubies to add {1} {2} to the guild?", GemCost.ToString("N0"), Amount.ToString("N0"), ResourceName(resource));
		MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), message, delegate
		{
			if (!requesting && isActiveAndEnabled)
			{
				StartCoroutine(Donate(resource));
			}
		}, null);
	}

	private IEnumerator Donate(GuildConstructionResource resource)
	{
		requesting = true;
		Place();
		try
		{
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			string body = "SessionKey=" + UnityWebRequest.EscapeURL(requester.SessionKey) + "&Gem=" + GemCost + "&Resource=" + resource;
			using UnityWebRequest request = new UnityWebRequest(requester.Host.TrimEnd('/') + "/guild/sprk_donate_construction", "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.timeout = 20;
			request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			yield return request.SendWebRequest();
			IDictionary response = (request.isNetworkError || request.isHttpError) ? null : (Json.Deserialize(request.downloadHandler.text) as IDictionary);
			string result = (response == null || !response.Contains("Result")) ? null : Convert.ToString(response["Result"]);
			if (result != "Success")
			{
				if (result == "NotEnoughGem")
				{
					MessagePopup.OpenError(NShared.ContributeGuild.ResultType.NotEnoughGem);
				}
				else
				{
					MessagePopup.OpenError(BaseResultType.Fail);
				}
				yield break;
			}
			Apply(response, resource);
		}
		finally
		{
			requesting = false;
			if (this != null && buttonRoot != null)
			{
				Place();
			}
		}
	}

	private static void Apply(IDictionary response, GuildConstructionResource resource)
	{
		if (response["CurrencyResult"] is IDictionary currency)
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(JM_NShared_CurrencyResultInfo3.Parse(currency));
		}
		GuildManager guilds = GlobalAutoBehaviour<GuildManager>.instance;
		GuildMemberInfo me = (guilds.MemberManager == null) ? null : guilds.MemberManager.GetMyMemberInfo();
		if (me != null)
		{
			switch (resource)
			{
			case GuildConstructionResource.Wood:
				me.AccWood += Amount;
				break;
			case GuildConstructionResource.Stone:
				me.AccStone += Amount;
				break;
			case GuildConstructionResource.Metal:
				me.AccMetal += Amount;
				break;
			}
		}
		if (guilds.MyGuildManager != null)
		{
			// Refreshes the open Construction window through RestoredPortal.
			guilds.MyGuildManager.SetConstructionResource(Convert.ToInt32(response["Wood"]), Convert.ToInt32(response["Stone"]), Convert.ToInt32(response["Metal"]));
		}
	}
}
