using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class IllustratedBookList : LobbyFullWindow
{
	public interface Listener
	{
		void OnEnterHeroListBook();

		void OnEnterMonsterListBook();

		void OnEnterItemListBook();

		void OnEnterPetListBook();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/IllustratedBookList";

	public UISprite SpriteHeroGauge;

	public UILabel LabelHeroGauge;

	public UISprite SpriteMonsterGauge;

	public UILabel LabelMonsterGauge;

	public UISprite SpriteItemGauge;

	public UILabel LabelItemGauge;

	public UISprite SpritePetGauge;

	public UILabel LabelPetGauge;

	public UITexture TextureArchiveHero;

	public UITexture TextureArchiveMonster;

	public UITexture TextureArchiveItem;

	public UITexture TextureArchivePet;

	public UIScrollView ScrollView;

	public string TextureArchivePathHero;

	public string TextureArchivePathMonster;

	public string TextureArchivePathItem;

	public string TextureArchivePathPet;

	private Listener _listener;

	public void Open(Listener listener)
	{
		if (listener == null)
		{
			VespaDebug.LogError("IllustratedBookList::Open / listener == null");
			return;
		}
		_listener = listener;
		Open(delegate
		{
			Init();
		});
	}

	public void Init()
	{
		ScrollView.ResetPosition();
		SetupHeroArchiveImage();
		SetupMonsterArchiveImage();
		SetupItemArchiveImage();
		SetupPetArchiveImage();
		SetupHeroGauge();
		SetupMonsterGauge();
		SetupItemGauge();
		SetupPetGauge();
	}

	private void SetupHeroArchiveImage()
	{
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(TextureArchivePathHero);
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureArchiveHero, assetTexture);
	}

	private void SetupMonsterArchiveImage()
	{
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(TextureArchivePathMonster);
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureArchiveMonster, assetTexture);
	}

	private void SetupItemArchiveImage()
	{
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(TextureArchivePathItem);
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureArchiveItem, assetTexture);
	}

	private void SetupPetArchiveImage()
	{
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(TextureArchivePathPet);
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureArchivePet, assetTexture);
	}

	private void SetupHeroGauge()
	{
		int num = 0;
		foreach (CreatureData allDatum in GameRoot.Instance.CreatureDataContainer.GetAllData())
		{
			if (allDatum.OpenType != CreatureOpenType.None && allDatum.OpenType != CreatureOpenType.CommingSoon)
			{
				num++;
			}
		}
		int heroArchiveCount = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetHeroArchiveCount(CreatureTagType.None);
		SetupGauge(SpriteHeroGauge, LabelHeroGauge, num, heroArchiveCount);
	}

	private void SetupMonsterGauge()
	{
		int num = 0;
		foreach (CreatureData allDatum in GameRoot.Instance.CreatureDataContainer.GetAllData())
		{
			if (allDatum.ArchiveType != ArchiveType.None)
			{
				num++;
			}
		}
		int subArchiveCount = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetSubArchiveCount(ArchiveType.None);
		SetupGauge(SpriteMonsterGauge, LabelMonsterGauge, num, subArchiveCount);
	}

	private void SetupItemGauge()
	{
		int num = 0;
		num += Common.GetUniqueItemArchiveTotalCount(ArchiveSubType.UniqueWeapon);
		num += Common.GetUniqueItemArchiveTotalCount(ArchiveSubType.UniqueTreasure);
		num += Common.GetUniqueItemArchiveTotalCount(ArchiveSubType.Artifact);
		num += Common.GetEquipItemArchiveTotalCount();
		num += Common.GetNormlaItemArchiveTotalCount();
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetItemCount();
		SetupGauge(SpriteItemGauge, LabelItemGauge, num, itemCount);
	}

	private void SetupPetGauge()
	{
		int num = 0;
		foreach (CreatureData allDatum in GameRoot.Instance.CreatureDataContainer.GetAllData())
		{
			if (allDatum.Race == CreatureRaceType.Pet)
			{
				num++;
			}
		}
		int petArchiveCount = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetPetArchiveCount(CreatureGrade.None);
		SetupGauge(SpritePetGauge, LabelPetGauge, num, petArchiveCount);
	}

	private void SetupGauge(UISprite spriteGauge, UILabel labelGauge, int totalHeroCount, int curHeroCount)
	{
		if (curHeroCount >= totalHeroCount)
		{
			string text = $"{100:N2}% ";
			UIHelper.SetSpriteFillAmount(spriteGauge, 1f);
			UIHelper.SetLabelText(labelGauge, text);
		}
		else
		{
			float num = Mathf.Clamp((float)curHeroCount / (float)totalHeroCount, 0f, 100f);
			string text2 = $"{num * 100f:N2}%";
			UIHelper.SetLabelText(labelGauge, text2);
			UIHelper.SetSpriteFillAmount(spriteGauge, num);
		}
	}

	public override void Close()
	{
		base.Close();
	}

	public void OnClickHeroBook()
	{
		if (_listener != null)
		{
			_listener.OnEnterHeroListBook();
		}
	}

	public void OnClickMonsterBook()
	{
		if (_listener != null)
		{
			_listener.OnEnterMonsterListBook();
		}
	}

	public void OnClickItemBook()
	{
		if (_listener != null)
		{
			_listener.OnEnterItemListBook();
		}
	}

	public void OnClickPetBook()
	{
		if (_listener != null)
		{
			_listener.OnEnterPetListBook();
		}
	}
}
