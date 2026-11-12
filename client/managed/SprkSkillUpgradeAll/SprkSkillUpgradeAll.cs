using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NShared;
using NShared.ExtendHeroSkill;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkSkillUpgradeAll : MonoBehaviour
{
	private sealed class Extras
	{
		public ItemResultInfo[] Items;

		public readonly List<KeyValuePair<int, int>> Extends = new List<KeyValuePair<int, int>>();
	}

	private struct Step
	{
		public int Slot;

		public string Name;

		public int From;

		public int To;

		public int Cost;

		public int Tickets;

		public string Book;
	}

	private sealed class NarrowedCounter : MonoBehaviour
	{
		public int RootWidth;

		public int BgWidth;

		public Vector3 Bg;

		public Vector3 Icon;

		public Vector3 Label;

		public Vector3 Update;

		public UIWidget.Pivot LabelPivot;
	}

	public const int ExtendAllMarker = -1;

	private const string ButtonText = "Upgrade";

	private const float ItemGap = 2f;

	private const float Gap = 12f;

	private const float Margin = 20f;

	private const float ButtonHeight = 0.88f;

	private const float LabelSize = 0.42f;

	private const float MaxScale = 1.8f;

	private const float CountBox = 0.72f;

	private const float LabelShift = 0.5f;

	private const float ButtonDrop = 1.5f;

	private static readonly FieldInfo IsMine = typeof(HeroSkillListPanel).GetField("_isMine", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly ConditionalWeakTable<NShared.ExtendHeroSkill.Response, Extras> Captured = new ConditionalWeakTable<NShared.ExtendHeroSkill.Response, Extras>();

	private HeroSkillListPanel panel;

	private UIButton button;

	private GameObject buttonRoot;

	private Vector3 gridScale;

	private Vector3 gridPosition;

	private float cellWidth;

	private bool laidOut;

	private bool requesting;

	public static void Attach(HeroSkillListPanel panel)
	{
		SprkSkillUpgradeAll sprkSkillUpgradeAll = panel.GetComponent<SprkSkillUpgradeAll>();
		if (sprkSkillUpgradeAll == null)
		{
			sprkSkillUpgradeAll = panel.gameObject.AddComponent<SprkSkillUpgradeAll>();
		}
		sprkSkillUpgradeAll.panel = panel;
		sprkSkillUpgradeAll.Refresh();
	}

	public static void Changed(HeroSkillListPanel panel)
	{
		SprkSkillUpgradeAll component = panel.GetComponent<SprkSkillUpgradeAll>();
		if (component != null)
		{
			component.Refresh();
		}
		if (panel.SkillBookGrid != null)
		{
			SprkSkillBookTickets.RefreshCounters(panel.SkillBookGrid.GetComponentsInChildren<UpgradeBookItemIconComponent>(includeInactive: true));
		}
	}

	public static void Encode(Dictionary<string, object> json, NShared.ExtendHeroSkill.Request request)
	{
		if (json != null && request != null && request.SkillExtend == -1)
		{
			json["ExtendAll"] = "1";
		}
	}

	public static void Capture(IDictionary json, NShared.ExtendHeroSkill.Response response)
	{
		if (json == null || response == null || !json.Contains("SkillExtends"))
		{
			return;
		}
		Extras extras = new Extras();
		if (json["ItemResults"] is IList list)
		{
			List<ItemResultInfo> list2 = new List<ItemResultInfo>();
			foreach (object item in list)
			{
				if (item is IDictionary d)
				{
					list2.Add(JM_NShared_ItemResultInfo.Parse(d));
				}
			}
			extras.Items = list2.ToArray();
		}
		if (json["SkillExtends"] is IList list3)
		{
			foreach (object item2 in list3)
			{
				if (item2 is IDictionary dictionary && dictionary.Contains("SkillSlotIndex") && dictionary.Contains("SkillExtend"))
				{
					extras.Extends.Add(new KeyValuePair<int, int>(Convert.ToInt32(dictionary["SkillSlotIndex"]), Convert.ToInt32(dictionary["SkillExtend"])));
				}
			}
		}
		Captured.Remove(response);
		Captured.Add(response, extras);
	}

	private List<Step> Plan()
	{
		SprkSkillBookTickets.Pool tickets;
		return Plan(out tickets);
	}

	private List<Step> Plan(out SprkSkillBookTickets.Pool tickets)
	{
		List<Step> list = new List<Step>();
		tickets = new SprkSkillBookTickets.Pool();
		CachedHeroInfo cachedHeroInfo = ((panel == null) ? null : panel.CachedHeroInfo);
		if (cachedHeroInfo == null)
		{
			return list;
		}
		for (int i = 1; i <= 4; i++)
		{
			Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(cachedHeroInfo, i);
			if (skillDetailInfo.ExtendInfos == null || skillDetailInfo.BasicInfo.IsLocked || skillDetailInfo.BasicInfo.Level <= 0 || skillDetailInfo.BookInfo.BookItemData == null)
			{
				continue;
			}
			int curItemCount = skillDetailInfo.BookInfo.CurItemCount;
			int index = skillDetailInfo.BookInfo.BookItemData.Index;
			int skillExtend = cachedHeroInfo.GetSkillExtend(i);
			int j = skillExtend;
			int num = 0;
			long num2 = (long)curItemCount + (long)tickets.Available(index);
			for (; j < skillDetailInfo.ExtendInfos.Length; j++)
			{
				int extendItemCount = skillDetailInfo.ExtendInfos[j].ExtendItemCount;
				if (extendItemCount <= 0 || num + extendItemCount > num2)
				{
					break;
				}
				num += extendItemCount;
			}
			if (j > skillExtend)
			{
				int tickets2 = tickets.Take(index, System.Math.Max(0, num - curItemCount));
				list.Add(new Step
				{
					Slot = i,
					From = skillExtend,
					To = j,
					Cost = num,
					Tickets = tickets2,
					Name = ((skillDetailInfo.BasicInfo.TableData == null) ? ("Skill " + i) : SkillDescContainer.GetSkillName(cachedHeroInfo, skillDetailInfo.BasicInfo.TableData)),
					Book = LocalizationManager.GetStringDirect(skillDetailInfo.BookInfo.BookItemData.Name)
				});
			}
		}
		return list;
	}

	private void Refresh()
	{
		bool flag = panel != null && (bool)IsMine.GetValue(panel) && panel.CachedHeroInfo != null;
		if (flag)
		{
			EnsureButton();
		}
		if (buttonRoot != null)
		{
			buttonRoot.SetActive(flag);
		}
		Layout(flag && buttonRoot != null);
		if (button != null && flag)
		{
			UIHelper.EnableButton(button, !requesting && Plan().Count > 0, grayscaleIfDisable: true);
		}
	}

	private void EnsureButton()
	{
		if (buttonRoot != null)
		{
			return;
		}
		Transform parent = panel.transform;
		while (parent != null && !parent.name.StartsWith("HeroList"))
		{
			parent = parent.parent;
		}
		Transform transform = ((parent == null) ? null : FindChild(parent, "Button_AllHero"));
		if (transform == null)
		{
			return;
		}
		Transform parent2 = panel.SkillBookGrid.transform.parent;
		buttonRoot = UnityEngine.Object.Instantiate(transform.gameObject, parent2, worldPositionStays: false);
		buttonRoot.name = "SprkSkillUpgradeAll";
		MonoBehaviour[] componentsInChildren = buttonRoot.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (monoBehaviour.GetType().Name == "UILocalizeEx")
			{
				UnityEngine.Object.DestroyImmediate(monoBehaviour);
			}
		}
		UILabel componentInChildren = buttonRoot.GetComponentInChildren<UILabel>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.text = "Upgrade";
		}
		button = buttonRoot.GetComponentInChildren<UIButton>(includeInactive: true);
		button.onClick.Clear();
		EventDelegate.Add(button.onClick, OnClick);
		int num = 0;
		UIWidget[] componentsInChildren2 = panel.SkillBookGrid.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren2)
		{
			num = System.Math.Max(num, uIWidget.depth);
		}
		componentsInChildren2 = buttonRoot.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget obj in componentsInChildren2)
		{
			obj.leftAnchor.target = null;
			obj.rightAnchor.target = null;
			obj.topAnchor.target = null;
			obj.bottomAnchor.target = null;
			obj.depth += num + 1;
		}
		NGUITools.SetLayer(buttonRoot, panel.gameObject.layer);
		NGUITools.MarkParentAsChanged(buttonRoot);
	}

	public static void Release(HeroSkillListPanel panel)
	{
		if (!(panel == null) && !(panel.SkillBookGrid == null))
		{
			UpgradeBookItemIconComponent[] componentsInChildren = panel.SkillBookGrid.GetComponentsInChildren<UpgradeBookItemIconComponent>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Restore(componentsInChildren[i].transform);
			}
		}
	}

	private static void Narrow(Transform item)
	{
		UIWidget component = item.GetComponent<UIWidget>();
		Transform transform = FindChild(item, "Sprite_Bg");
		UIWidget uIWidget = ((transform == null) ? null : transform.GetComponent<UIWidget>());
		Transform transform2 = FindChild(item, "Label_Count");
		UILabel uILabel = ((transform2 == null) ? null : transform2.GetComponent<UILabel>());
		Transform transform3 = FindChild(item, "Texture_ItemIcon");
		Transform transform4 = FindChild(item, "Label_UpdateCount");
		if (!(component == null) && !(uIWidget == null) && !(uILabel == null) && !(transform3 == null))
		{
			NarrowedCounter narrowedCounter = item.GetComponent<NarrowedCounter>();
			if (narrowedCounter == null)
			{
				narrowedCounter = item.gameObject.AddComponent<NarrowedCounter>();
				narrowedCounter.RootWidth = component.width;
				narrowedCounter.BgWidth = uIWidget.width;
				narrowedCounter.Bg = transform.localPosition;
				narrowedCounter.Icon = transform3.localPosition;
				narrowedCounter.Label = transform2.localPosition;
				narrowedCounter.Update = ((transform4 == null) ? Vector3.zero : transform4.localPosition);
				narrowedCounter.LabelPivot = uILabel.pivot;
			}
			else
			{
				Restore(item);
			}
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(item, transform, considerInactive: true);
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(item, transform3, considerInactive: true);
			uIWidget.width = Mathf.RoundToInt((float)narrowedCounter.BgWidth * 0.72f);
			Bounds bounds3 = NGUIMath.CalculateRelativeWidgetBounds(item, transform, considerInactive: true);
			transform.localPosition = narrowedCounter.Bg + new Vector3(bounds.min.x - bounds3.min.x, 0f, 0f);
			float num = bounds.min.x + bounds3.size.x;
			float num2 = num - bounds.max.x;
			uILabel.pivot = UIWidget.Pivot.Center;
			float a = (Mathf.Max(bounds2.max.x, bounds.min.x) + num) * 0.5f;
			float b = (bounds.min.x + num) * 0.5f;
			transform2.localPosition = new Vector3(Mathf.Lerp(a, b, 0.5f), narrowedCounter.Label.y, narrowedCounter.Label.z);
			if (transform4 != null)
			{
				transform4.localPosition = narrowedCounter.Update + new Vector3(num2, 0f, 0f);
			}
			Bounds bounds4 = NGUIMath.CalculateRelativeWidgetBounds(item.parent, item, considerInactive: false);
			component.width = Mathf.Max(1, Mathf.RoundToInt((float)narrowedCounter.RootWidth + num2));
			Bounds bounds5 = NGUIMath.CalculateRelativeWidgetBounds(item.parent, item, considerInactive: false);
			transform.localPosition += new Vector3(bounds4.min.x - bounds5.min.x, 0f, 0f);
			transform2.localPosition += new Vector3(bounds4.min.x - bounds5.min.x, 0f, 0f);
			transform3.localPosition += new Vector3(bounds4.min.x - bounds5.min.x, 0f, 0f);
			if (transform4 != null)
			{
				transform4.localPosition += new Vector3(bounds4.min.x - bounds5.min.x, 0f, 0f);
			}
			NGUITools.UpdateWidgetCollider(item.gameObject);
		}
	}

	private static void Restore(Transform item)
	{
		NarrowedCounter component = item.GetComponent<NarrowedCounter>();
		if (!(component == null))
		{
			UIWidget component2 = item.GetComponent<UIWidget>();
			Transform transform = FindChild(item, "Sprite_Bg");
			Transform transform2 = FindChild(item, "Label_Count");
			Transform transform3 = FindChild(item, "Texture_ItemIcon");
			Transform transform4 = FindChild(item, "Label_UpdateCount");
			if (component2 != null)
			{
				component2.width = component.RootWidth;
			}
			if (transform != null)
			{
				transform.GetComponent<UIWidget>().width = component.BgWidth;
				transform.localPosition = component.Bg;
			}
			if (transform2 != null)
			{
				transform2.GetComponent<UILabel>().pivot = component.LabelPivot;
				transform2.localPosition = component.Label;
			}
			if (transform3 != null)
			{
				transform3.localPosition = component.Icon;
			}
			if (transform4 != null)
			{
				transform4.localPosition = component.Update;
			}
			NGUITools.UpdateWidgetCollider(item.gameObject);
		}
	}

	private static Transform FindChild(Transform root, string name)
	{
		Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	private void Layout(bool show)
	{
		Transform transform = panel.SkillBookGrid.transform;
		if (!laidOut)
		{
			gridScale = transform.localScale;
			gridPosition = transform.localPosition;
			laidOut = true;
			if (panel.SkillBookGrid.Grid != null)
			{
				cellWidth = panel.SkillBookGrid.Grid.cellWidth;
			}
		}
		transform.localScale = gridScale;
		transform.localPosition = gridPosition;
		if (panel.SkillBookGrid.Grid != null && cellWidth > 0f)
		{
			panel.SkillBookGrid.Grid.cellWidth = cellWidth;
		}
		if (!show)
		{
			return;
		}
		Transform parent = transform.parent;
		List<Transform> list = new List<Transform>();
		UpgradeBookItemIconComponent[] componentsInChildren = panel.SkillBookGrid.GetComponentsInChildren<UpgradeBookItemIconComponent>(includeInactive: false);
		foreach (UpgradeBookItemIconComponent upgradeBookItemIconComponent in componentsInChildren)
		{
			list.Add(upgradeBookItemIconComponent.transform);
		}
		if (list.Count == 0)
		{
			return;
		}
		list.Sort((Transform a, Transform b) => a.localPosition.x.CompareTo(b.localPosition.x));
		foreach (Transform item in list)
		{
			Narrow(item);
		}
		UIGrid grid = panel.SkillBookGrid.Grid;
		Transform transform2 = FindChild(list[0], "Sprite_Bg");
		Transform transform3 = FindChild(list[0], "Texture_ItemIcon");
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(list[0].parent, (transform2 != null) ? transform2 : list[0], considerInactive: false);
		if (transform2 != null && transform3 != null)
		{
			bounds.Encapsulate(NGUIMath.CalculateRelativeWidgetBounds(list[0].parent, transform3, considerInactive: false));
		}
		if (bounds.size.x <= 0f || grid == null)
		{
			return;
		}
		grid.cellWidth = bounds.size.x + 2f;
		grid.Reposition();
		Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(parent, transform, considerInactive: false);
		if (bounds2.size.x <= 0f || bounds2.size.y <= 0f)
		{
			return;
		}
		float num = bounds2.size.y * 0.88f;
		UIWidget component = button.GetComponent<UIWidget>();
		UILabel componentInChildren = buttonRoot.GetComponentInChildren<UILabel>(includeInactive: true);
		float num2 = num * 2f;
		if (componentInChildren != null)
		{
			componentInChildren.overflowMethod = UILabel.Overflow.ResizeFreely;
			componentInChildren.pivot = UIWidget.Pivot.Center;
			componentInChildren.fontSize = Mathf.RoundToInt(num * 0.42f);
			componentInChildren.text = "Upgrade";
			componentInChildren.transform.localScale = Vector3.one;
			componentInChildren.transform.localPosition = new Vector3(0f, num * 0.04f, 0f);
			num2 = Mathf.Max(num2, componentInChildren.printedSize.x + num * 0.9f);
		}
		UIWidget[] array = new UIWidget[2]
		{
			buttonRoot.GetComponent<UIWidget>(),
			component
		};
		foreach (UIWidget uIWidget in array)
		{
			if (uIWidget != null)
			{
				uIWidget.pivot = UIWidget.Pivot.Center;
				uIWidget.width = Mathf.RoundToInt(num2);
				uIWidget.height = Mathf.RoundToInt(num);
			}
		}
		if (component != null)
		{
			component.transform.localScale = Vector3.one;
			component.transform.localPosition = Vector3.zero;
			NGUITools.UpdateWidgetCollider(component.gameObject);
		}
		Transform transform4 = FindChild(panel.transform, "Sprite_MainFrameSmall");
		Bounds bounds3 = bounds2;
		if (transform4 != null && transform4.GetComponent<UIWidget>() != null)
		{
			bounds3 = NGUIMath.CalculateRelativeWidgetBounds(parent, transform4, considerInactive: false);
		}
		float num3 = bounds3.size.x - 40f;
		float num4 = Mathf.Min(1.8f, (num3 - 12f - num2) / bounds2.size.x);
		float num5 = bounds2.size.x * num4 + 12f + num2;
		float num6 = bounds3.center.x - num5 * 0.5f;
		float y = bounds2.center.y;
		transform.localScale = new Vector3(gridScale.x * num4, gridScale.y * num4, gridScale.z);
		transform.localPosition = new Vector3(num6 - (bounds2.min.x - gridPosition.x) * num4, y - (bounds2.center.y - gridPosition.y) * num4, gridPosition.z);
		buttonRoot.transform.localScale = Vector3.one;
		buttonRoot.transform.localPosition = new Vector3(num6 + bounds2.size.x * num4 + 12f + num2 * 0.5f, y - 1.5f, 0f);
	}

	public void OnClick()
	{
		if (requesting || panel == null || panel.CachedHeroInfo == null)
		{
			return;
		}
		SprkSkillBookTickets.Pool tickets;
		List<Step> list = Plan(out tickets);
		if (list.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Step item in list)
		{
			stringBuilder.Append(item.Name).Append("  ").Append(Roman(item.From))
				.Append(" → ")
				.Append(Roman(item.To))
				.Append("  (")
				.Append(item.Book)
				.Append(" ×")
				.Append(item.Cost);
			if (item.Tickets > 0)
			{
				stringBuilder.Append(", ").Append(item.Tickets).Append(" from tickets");
			}
			stringBuilder.Append(")\n");
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (SprkSkillBookTickets.Conversion conversion in tickets.Conversions)
		{
			dictionary.TryGetValue(conversion.Ticket, out var value);
			dictionary[conversion.Ticket] = value + conversion.Count;
		}
		if (dictionary.Count > 0)
		{
			stringBuilder.Append("Uses:");
			foreach (KeyValuePair<int, int> item2 in dictionary)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item2.Key);
				stringBuilder.Append("\n  ").Append((data == null) ? item2.Key.ToString() : LocalizationManager.GetStringDirect(data.Name)).Append(" ×")
					.Append(item2.Value);
			}
		}
		int hero = panel.CachedHeroInfo.HeroIndex;
		List<SprkSkillBookTickets.Conversion> conversions = tickets.Conversions;
		MessagePopup.Open(LocalizationManager.GetString(HeroSkill.LEARN_EXTEND_SKILL), "Upgrade all skills with Skill Books?\n" + stringBuilder.ToString().TrimEnd(), delegate
		{
			RedeemThenSend(hero, conversions);
		}, null);
	}

	private void RedeemThenSend(int heroIndex, List<SprkSkillBookTickets.Conversion> conversions)
	{
		if (requesting)
		{
			return;
		}
		if (conversions.Count == 0)
		{
			Send(heroIndex);
			return;
		}
		requesting = true;
		Refresh();
		SprkSkillBookTickets.Redeem(conversions, delegate(bool ok)
		{
			requesting = false;
			if (!(this == null))
			{
				if (ok && panel != null && panel.CachedHeroInfo != null && panel.CachedHeroInfo.HeroIndex == heroIndex)
				{
					Send(heroIndex);
				}
				else
				{
					Refresh();
				}
			}
		});
	}

	private static string Roman(int extend)
	{
		return extend switch
		{
			0 => "I", 
			1 => "II", 
			2 => "III", 
			3 => "IV", 
			_ => (extend + 1).ToString(), 
		};
	}

	private void Send(int heroIndex)
	{
		if (requesting)
		{
			return;
		}
		requesting = true;
		Refresh();
		NShared.ExtendHeroSkill.Request request = new NShared.ExtendHeroSkill.Request
		{
			HeroIndex = heroIndex,
			SkillIndex = 0,
			SkillExtend = -1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ExtendHeroSkill.Response response)
		{
			requesting = false;
			try
			{
				if (response != null)
				{
					if (response.BaseResult != BaseResultType.Success)
					{
						MessagePopup.OpenError(response.BaseResult);
					}
					else if (response.Result != ResultType.Success)
					{
						MessagePopup.OpenError(response.Result);
					}
					else
					{
						AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
						if (Captured.TryGetValue(response, out var value))
						{
							if (value.Items != null)
							{
								instance.ItemManager.Apply(value.Items);
							}
							{
								foreach (KeyValuePair<int, int> extend in value.Extends)
								{
									instance.HeroManager.SetSkillExtend(heroIndex, extend.Key, extend.Value);
								}
								return;
							}
						}
						if (response.ItemResult != null)
						{
							instance.ItemManager.Apply(response.ItemResult);
						}
					}
				}
			}
			finally
			{
				if (this != null)
				{
					Refresh();
				}
			}
		});
	}
}
