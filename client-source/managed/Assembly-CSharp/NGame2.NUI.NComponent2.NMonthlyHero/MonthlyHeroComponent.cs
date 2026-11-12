using NGame2.NUI.NWindow.NMonthlyHero;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NMonthlyHero;

public class MonthlyHeroComponent : LinkFactoryComponent, InitOnCreate, HeroPortraitComponent.Listener
{
	public interface IListener
	{
		bool IsRewardable(int monthlyHeroDataIndex);

		void OnClick(MonthlyHeroComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/MonthlyHero/MonthlyHeroComponent";

	public static PrefabAuto<MonthlyHeroComponent> PrefabAuto = new PrefabAuto<MonthlyHeroComponent>("Prefab2/Component/MonthlyHero/MonthlyHeroComponent");

	public Transform TransformNew;

	private HeroPortraitComponent _heroPortraitComponent;

	private IListener _Listener;

	public MonthlyDataWrapper MonthlyDataWrapper { get; set; }

	public void Open(IListener listener, MonthlyDataWrapper dataWrapper)
	{
		_Listener = listener;
		MonthlyDataWrapper = dataWrapper;
		CreateHeroPortraitComponent();
		SetGrayScale();
		SetRewardableTag();
	}

	private void CreateHeroPortraitComponent()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(MonthlyDataWrapper.Data.HeroIndex);
		_heroPortraitComponent = HeroPortraitComponent.SmallPrefabAuto.Factory.Create(base.transform);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_heroPortraitComponent.transform);
		_heroPortraitComponent.Open(data, this, HeroPortraitComponent.EPortraitMode.Icon, 5);
	}

	private void SetGrayScale()
	{
		UIHelper.SetWidgetGrayScale(_heroPortraitComponent.GetComponent<UIWidget>(), !MonthlyDataWrapper.IsOpen());
	}

	public void SetRewardableTag()
	{
		UIHelper.EnableGameObject(TransformNew, IsRewardable());
	}

	public bool IsRewardable()
	{
		bool num = _Listener.IsRewardable(MonthlyDataWrapper.Data.Index);
		bool flag = MonthlyDataWrapper.IsOpen();
		return num && flag;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _heroPortraitComponent);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		_Listener.OnClick(this);
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}
}
