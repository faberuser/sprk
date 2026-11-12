using System;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PetExploreListComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickButton(PetExploreListComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Pet/PetExploreListComponent";

	public static PrefabAuto<PetExploreListComponent> PrefabAuto = new PrefabAuto<PetExploreListComponent>("Prefab2/Component/Pet/PetExploreListComponent");

	public UITexture Texture;

	public UILabel LabelName;

	public UILabel LabelDesc;

	public UILabel LabelExploreCount;

	public UILabel LabelExploreButton;

	public GameObject PetExploreNoticGo;

	private IListener _listener;

	private NGame2.NUI.NWindow.IntervalUpdater _updater;

	public int ExploreIndex { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_listener = null;
		_updater = null;
		ExploreIndex = 0;
		UIHelper.ClearPlaneAssetTexture(Texture);
	}

	public void Open(PetAdventureData data, IListener listener)
	{
		if (data != null)
		{
			string imagePath = data.ImagePath;
			string nameKey = data.NameKey;
			string descKey = data.DescKey;
			ExploreIndex = data.Index;
			Open(imagePath, nameKey, descKey, listener);
		}
	}

	private void Open(string imagePath, string nameKey, string descKey, IListener listener)
	{
		_listener = listener;
		SetImage(imagePath);
		SetNameLabel(nameKey);
		SetDescLabel(descKey);
		SetExploreNotice(ExploreIndex);
		SetExploreCountLabel("EXPLORE_PARTY_COUNT", ExploreIndex);
	}

	private void SetImage(string imagePath)
	{
		if (!String_Extension_Method.IsNullOrEmpty(imagePath))
		{
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(imagePath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture, assetTexture);
		}
	}

	private void SetLabel(UILabel label, string localizeKey, params object[] args)
	{
		bool flag = !String_Extension_Method.IsNullOrEmpty(localizeKey);
		UIHelper.EnableGameObject(label, flag);
		if (flag)
		{
			string text = (__Extension_Method_ICollection__.IsNullOrEmpty(args) ? LocalizationManager.GetStringDirect(localizeKey) : LocalizationManager.GetStringDirect(localizeKey, args));
			UIHelper.SetLabelText(label, text);
		}
	}

	private void SetNameLabel(string nameKey)
	{
		SetLabel(LabelName, nameKey, null);
	}

	private void SetDescLabel(string descKey)
	{
		SetLabel(LabelDesc, descKey, null);
	}

	private void SetExploreCountLabel(string localizeKey, int exploreIndex)
	{
		int maxExploreCount = PetHelper.GetMaxExploreCount();
		int curExploreCount = PetHelper.GetCurExploreCount(exploreIndex);
		SetLabel(LabelExploreCount, localizeKey, curExploreCount, maxExploreCount);
		string localizeKey2 = ((maxExploreCount == curExploreCount) ? "EXPLORE_PROGRESS" : "EXPLORE_READY");
		SetLabel(LabelExploreButton, localizeKey2);
	}

	private void SetExploreNotice(int exploreIndex)
	{
		_updater = new NGame2.NUI.NWindow.IntervalUpdater(delegate
		{
			DateTime exploreEndTime = PetHelper.GetExploreEndTime(exploreIndex);
			if (exploreEndTime == DateTime.MaxValue)
			{
				SetExploreNotice(show: false);
			}
			else if (PetHelper.GetCurServerTimeUTCTime() <= exploreEndTime)
			{
				SetExploreNotice(show: false);
			}
			else
			{
				SetExploreNotice(show: true);
			}
		});
		_updater.SetInterval(1f);
		_updater.ForcedUpdate();
	}

	public void Refresh()
	{
		SetExploreCountLabel("EXPLORE_PARTY_COUNT", ExploreIndex);
		SetExploreNotice(ExploreIndex);
	}

	public void OnUpdate()
	{
		if (_updater != null)
		{
			_updater.Update();
		}
	}

	private void SetExploreNotice(bool show)
	{
		UIHelper.EnableGameObject(PetExploreNoticGo, show);
	}

	public void OnClickButton()
	{
		if (_listener != null)
		{
			_listener.OnClickButton(this);
		}
	}
}
