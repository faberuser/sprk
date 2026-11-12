using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class HeroLimitBreakGate : BaseWindow
{
	public class OpenData
	{
		public CachedHeroInfo HeroInfo { get; set; }

		public List<Tuple<string, int>> ReqItemCounts { get; set; }

		public Action<CachedHeroInfo> OnOpenGate { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroLimitBreakGate";

	public const string SealsActivate = "Activate";

	public UISimpleGrid EnterReqItemGrid;

	public UIButton Button_Enter;

	public UIGame3DWidget SealsEffect;

	public UIWidget WidgetGate;

	public UILabel Label_Title;

	public UIPlayAssetSound OpenLockSound;

	private OpenData _openData;

	private CreatureData _creatureData;

	private List<ChallengeReqItemComponent> _ListChallengeReqItems = new List<ChallengeReqItemComponent>();

	public override void Reset()
	{
		base.Reset();
		if (SealsEffect != null)
		{
			SealsEffect.Stop(immediatelyClear: true);
		}
		_openData = null;
		_creatureData = null;
		FactoryBase.ReleaseAndClearList(_ListChallengeReqItems);
	}

	public void Open(OpenData openData)
	{
		Open(delegate
		{
			Init(openData);
		}, null);
	}

	public void Init(OpenData openData)
	{
		if (openData == null)
		{
			VespaDebug.LogError("openData is null");
			return;
		}
		_openData = openData;
		CachedHeroInfo heroInfo = openData.HeroInfo;
		if (heroInfo == null)
		{
			VespaDebug.LogError("heroInfo is null");
			return;
		}
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (_creatureData == null)
		{
			VespaDebug.LogError("creatureData is null");
			return;
		}
		UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect("LIMITBREAK_HERO_NAME", _creatureData.Name));
		SetupGate();
		SetupChallengeHeroReqItemList();
		InitSeals();
	}

	private void SetupGate()
	{
		UIHelper.EnableGameObject(WidgetGate, enable: true);
		UIHelper.EnableGameObject(Button_Enter, enable: true);
		UIHelper.EnableButton(Button_Enter, enable: false);
		UIHelper.EnableButton(Button_Enter, enable: true);
	}

	private void SetupChallengeHeroReqItemList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_openData.ReqItemCounts); i++)
		{
			Tuple<string, int> tuple = _openData.ReqItemCounts[i];
			string item = tuple.Item1;
			int item2 = tuple.Item2;
			if (String_Extension_Method.IsNullOrEmpty(item))
			{
				break;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item);
			if (dataByCode == null)
			{
				break;
			}
			CreateChallengeReqItemComponent(dataByCode, Helper.GetItemCount(dataByCode.Index), item2);
		}
		UIHelper.RepositionGrid(EnterReqItemGrid);
	}

	private void CreateChallengeReqItemComponent(ItemData itemData, int ItemCount, int ReqItemCount)
	{
		ChallengeReqItemComponent challengeReqItemComponent = UIHelper.AddGridItem(EnterReqItemGrid, ChallengeReqItemComponent.PrefabAuto);
		challengeReqItemComponent.Open(itemData, ItemCount, ReqItemCount, null);
		_ListChallengeReqItems.Add(challengeReqItemComponent);
	}

	private void InitSeals()
	{
		for (int i = 0; i < _ListChallengeReqItems.Count; i++)
		{
			_ListChallengeReqItems[i].SetUsed();
		}
		if (OpenLockSound != null)
		{
			OpenLockSound.Play();
		}
		if (SealsEffect != null)
		{
			SealsEffect.Stop(immediatelyClear: true);
			SealsEffect.Play(null);
			for (int j = 0; j <= __Extension_Method_ICollection__.COUNT(_openData.ReqItemCounts); j++)
			{
				SealsEffect.PlayEntryPhase(j, "Activate");
			}
		}
	}

	public void OnClickOpen()
	{
		if (_openData.OnOpenGate != null)
		{
			_openData.OnOpenGate(_openData.HeroInfo);
		}
	}

	public void OnClickBackButton()
	{
		Close();
	}
}
