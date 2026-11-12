using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NUI.NGame3DWidget;
using NGame2.NUI.NWindow;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class UIScreenEffectPlayer : MonoBehaviour, IInspectorGuiDrawer
{
	public enum EContentsType
	{
		Effect,
		Sound
	}

	[Serializable]
	public class EntryData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public EContentsType ContentsType;

		public string EventCode = string.Empty;

		public TransformData ExtraTransform = new TransformData(100f);

		[NonSerialized]
		public List<IContents> ContentsList = new List<IContents>();

		bool ILoadJson.Load(JsonData json)
		{
			ContentsType = json["ContentsType"].GETENUM(ContentsType);
			EventCode = json["EventCode"].GETSTRING(EventCode);
			ExtraTransform.LOADFROMJSON(json["ExtraTransform"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldEnum("ContentsType", ContentsType);
			jsonData.AddFieldString("EventCode", EventCode);
			jsonData.AddFieldJsonMake("ExtraTransform", ExtraTransform);
			return jsonData;
		}
	}

	[HideInInspector]
	private GameUIRoot.ELayerType LayerType;

	[HideInInspector]
	public List<EntryData> EntryList = new List<EntryData>();

	private bool _reserveContents;

	private ILocalTimer _localTimer;

	public ILocalTimer LocalTimer
	{
		get
		{
			if (_localTimer == null)
			{
				return UnityLocalTimer.Default;
			}
			return _localTimer;
		}
		set
		{
			_localTimer = value;
		}
	}

	public bool IsPlaying
	{
		get
		{
			for (int i = 0; i < EntryList.Count; i++)
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(EntryList[i].ContentsList))
				{
					return true;
				}
			}
			return false;
		}
	}

	public void PlayEntry(int entryIndex, Action onComplete)
	{
		ReserveContents();
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else if (valid.ContentsType == EContentsType.Effect)
		{
			ScreenEffect.StockData stockData = new ScreenEffect.StockData();
			stockData.Code = valid.EventCode;
			stockData.FromTm = base.transform;
			stockData.ExtraTransform = valid.ExtraTransform;
			stockData.LocalTimer = LocalTimer;
			stockData.OnComplete = onComplete;
			IContents item = ScreenEffect.PlayEffect(LayerType, stockData);
			valid.ContentsList.Add(item);
		}
		else if (valid.ContentsType == EContentsType.Sound)
		{
			IContents item2 = ScreenEffect.PlaySound(valid.EventCode, onComplete);
			valid.ContentsList.Add(item2);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
	}

	public void Play(Action onComplete)
	{
		ReserveContents();
		if (__Extension_Method_ICollection__.COUNT(EntryList) == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		int playCount = 0;
		int completeCount = 0;
		for (int i = 0; i < EntryList.Count; i++)
		{
			PlayEntry(i, delegate
			{
				completeCount++;
				if (playCount == completeCount)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete);
				}
			});
		}
	}

	public void Stop(bool immediatelyClear)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(entryData.ContentsList); j++)
			{
				entryData.ContentsList[j].Stop(immediatelyClear);
			}
		}
	}

	private void Start()
	{
		ReserveContents();
	}

	private void Update()
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(entryData.ContentsList); j++)
			{
				if (entryData.ContentsList[j].GetInstanceCount() == 0)
				{
					entryData.ContentsList.RemoveAt(j);
					j--;
				}
			}
		}
	}

	private void OnDisable()
	{
		Stop(immediatelyClear: true);
	}

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			ReleaseAllContents();
		}
	}

	private void ReleaseAllContents()
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(entryData.ContentsList); j++)
			{
				entryData.ContentsList[j].Stop(immediatelyClear: true);
			}
			entryData.ContentsList.Clear();
		}
	}

	public void ReserveContents()
	{
		if (!_reserveContents)
		{
			_reserveContents = true;
		}
	}
}
