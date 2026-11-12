using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ChapterComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void SelectChapter(CampaignChapterData chapterData);
	}

	public const string PrefabPath = "Prefab2/Component/Portal/ChapterComponent";

	public static PrefabAuto<ChapterComponent> PrefabAuto = new PrefabAuto<ChapterComponent>("Prefab2/Component/Portal/ChapterComponent");

	public UILabel Label_ChapterName;

	public UITexture Texture_Chapter;

	private CampaignChapterData _ChapterData;

	private Listener _listener;

	public void Open(Listener listener, CampaignChapterData chapterData)
	{
		_listener = listener;
		_ChapterData = chapterData;
		Setup();
	}

	private void Setup()
	{
		if (_ChapterData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(_ChapterData.Name);
			if (_ChapterData.Type == ChapterType.City)
			{
				UIHelper.SetLabelText(Label_ChapterName, stringDirect);
			}
			else
			{
				UIHelper.SetLabelText(Label_ChapterName, LocalizationManager.GetString(WorldMap.WORLDMAP_CHAPTER_NAME, _ChapterData.Index, stringDirect));
			}
			AssetTexture portalBackground = UIHelper.GetPortalBackground(_ChapterData.BasePortalImageName);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Chapter, portalBackground);
		}
	}

	public void OnClickComponent()
	{
		if (_listener != null && _ChapterData != null)
		{
			_listener.SelectChapter(_ChapterData);
		}
	}
}
