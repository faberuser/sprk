using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NDataHelper;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

public class ContentUIGatewayMain : BaseWindow
{
	public const string PATH_PREFAB = "Resources/Prefab2/Window/Tower/ContentUIGatewayMain";

	public UIScrollGrid ScrollGridDungeons;

	private ContentUIGatewayComponent.IListener _listener;

	private List<GatewayToTrialData> _dataList = new List<GatewayToTrialData>(10);

	private List<ContentUIGatewayComponent> _compList = new List<ContentUIGatewayComponent>(10);

	private PrefabAuto<ContentUIGatewayComponent> _prefabAutoComp = new PrefabAuto<ContentUIGatewayComponent>("Resources/Prefab2/Component/Tower/ContentUIGatewayComponent");

	public void Open(ContentUIGatewayComponent.IListener listener, Action onOpen)
	{
		_listener = listener;
		base.Open(delegate
		{
			UpdateDataList();
			UpdateUI();
			MoveToTargetDungeon(GetLastDungeonIndex());
			Vespa_System_Action_Extension_Method.fire(onOpen);
		});
	}

	private void UpdateDataList()
	{
		_dataList.Clear();
		IList<GatewayToTrialData> allData = GameRoot.Instance.GatewayToTrialDataContainer.GetAllData();
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(allData); i < num; i++)
		{
			GatewayToTrialData gatewayToTrialData = allData[i];
			if (gatewayToTrialData != null)
			{
				_dataList.Add(gatewayToTrialData);
			}
		}
	}

	private void UpdateUI(int step = 0)
	{
		UpdateUIList(step);
	}

	private void UpdateUIList(int step = 0, bool isPosFix = false)
	{
		int i = 0;
		for (int count = _dataList.Count; i < count; i++)
		{
			GatewayToTrialData gatewayToTrialData = _dataList[i];
			if (gatewayToTrialData != null)
			{
				ContentUIGatewayComponent contentUIGatewayComponent = null;
				if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_compList, i))
				{
					contentUIGatewayComponent = _compList[i];
				}
				if (contentUIGatewayComponent != null)
				{
					UpdateDungeonComponent(contentUIGatewayComponent, gatewayToTrialData);
				}
				else
				{
					AddDungeonComponent(gatewayToTrialData);
				}
			}
		}
		ScrollGridDungeons.RepositionGridWithFixableScroll(isPosFix);
		int num = __Extension_Method_ICollection__.COUNT(_compList) - __Extension_Method_ICollection__.COUNT(_dataList);
		if (num > 0)
		{
			int j = 0;
			for (int num2 = num; j < num2; j++)
			{
				int index = _compList.Count - 1;
				FactoryBase.ReleaseComponent(_compList[index]);
			}
		}
	}

	private void AddDungeonComponent(GatewayToTrialData data)
	{
		if (data != null)
		{
			ContentUIGatewayComponent contentUIGatewayComponent = ScrollGridDungeons.AddGridItem(_prefabAutoComp);
			if (!(contentUIGatewayComponent == null))
			{
				_compList.Add(contentUIGatewayComponent);
				UpdateDungeonComponent(contentUIGatewayComponent, data);
			}
		}
	}

	private void UpdateDungeonComponent(ContentUIGatewayComponent comp, GatewayToTrialData data)
	{
		if (data != null)
		{
			comp.Open(data, _listener);
		}
	}

	private int GetLastDungeonIndex()
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_dataList); i < num; i++)
		{
			GatewayToTrialData gatewayToTrialData = _dataList[i];
			if (gatewayToTrialData != null && GatewayDataHelper.IsOpened(gatewayToTrialData) && !GatewayDataHelper.IsCleared(gatewayToTrialData))
			{
				return gatewayToTrialData.Step;
			}
		}
		return 1;
	}

	private void MoveToTargetDungeon(int step)
	{
		if (!(ScrollGridDungeons == null))
		{
			ContentUIGatewayComponent stepComp = GetStepComp(step);
			if (!(stepComp == null))
			{
				ScrollGridDungeons.ScrollTo(stepComp, 0f, 0f);
			}
		}
	}

	private ContentUIGatewayComponent GetStepComp(int step)
	{
		int i = 0;
		for (int count = _compList.Count; i < count; i++)
		{
			ContentUIGatewayComponent contentUIGatewayComponent = _compList[i];
			if (!(contentUIGatewayComponent == null) && contentUIGatewayComponent.Data != null && contentUIGatewayComponent.Data.Step == step)
			{
				return contentUIGatewayComponent;
			}
		}
		return null;
	}
}
