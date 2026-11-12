using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NScene;
using NGame2.NUI.NGame3DWidget;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class UIGame3DWidget : UIWidgetExtraRenderQ
{
	public enum EContentsType
	{
		Effect,
		NodeTree,
		Sound
	}

	[Serializable]
	public class EntryData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public EContentsType ContentsType;

		public string EventCode = string.Empty;

		public string PackCode = string.Empty;

		public TransformData ExtraTransform = new TransformData(100f);

		[NonSerialized]
		public IContents Contents;

		[NonSerialized]
		public bool AutoDestroy;

		bool ILoadJson.Load(JsonData json)
		{
			ContentsType = json["ContentsType"].GETENUM(ContentsType);
			EventCode = json["EventCode"].GETSTRING(EventCode);
			PackCode = json["PackCode"].GETSTRING(PackCode);
			ExtraTransform.LOADFROMJSON(json["ExtraTransform"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldEnum("ContentsType", ContentsType);
			jsonData.AddFieldString("EventCode", EventCode);
			jsonData.AddFieldString("PackCode", PackCode);
			jsonData.AddFieldJsonMake("ExtraTransform", ExtraTransform);
			return jsonData;
		}
	}

	[HideInInspector]
	public List<EntryData> EntryList = new List<EntryData>();

	private bool _reserveContents;

	private RenderQueueVisitor _renderQueueVisitor = new RenderQueueVisitor();

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

	public int RenderLayer
	{
		get
		{
			if (panel != null)
			{
				return panel.gameObject.layer;
			}
			return base.gameObject.layer;
		}
	}

	public bool IsPlaying
	{
		get
		{
			for (int i = 0; i < EntryList.Count; i++)
			{
				EntryData entryData = EntryList[i];
				if (entryData.Contents != null && entryData.Contents.GetInstanceCount() > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public IContents PlayInstantEffect(string effectCode, TransformData extraTransform, Action onComplete, bool playSync = false)
	{
		return PlayInstantEffect(effectCode, extraTransform, LocalTimer, onComplete, playSync);
	}

	public IContents PlayInstantEffect(string effectCode, TransformData extraTransform, ILocalTimer localTimer, Action onComplete, bool playSync)
	{
		return PlayInstantEffect(effectCode, null, extraTransform, localTimer, onComplete, playSync);
	}

	public IContents PlayInstantEffect(string effectCode, Transform from, TransformData extraTransform, ILocalTimer localTimer, Action onComplete, bool playSync)
	{
		ReserveContents();
		ITransformData extraTransform2 = ((extraTransform == null) ? new TransformData(100f) : ((!(from != null)) ? extraTransform : extraTransform.Convert(from, base.transform)));
		EntryData entryData = new EntryData();
		entryData.EventCode = effectCode;
		entryData.Contents = new ContentsEffect(this, effectCode);
		entryData.AutoDestroy = true;
		EntryList.Add(entryData);
		entryData.Contents.Play(extraTransform2, (localTimer == null) ? LocalTimer : localTimer, onComplete, playSync);
		return entryData.Contents;
	}

	public IContents PlayInstantSound(string soundCode, Action onComplete)
	{
		return PlayInstantSound(soundCode, LocalTimer, onComplete);
	}

	public IContents PlayInstantSound(string soundCode, ILocalTimer localTimer, Action onComplete)
	{
		ReserveContents();
		EntryData entryData = new EntryData();
		entryData.EventCode = soundCode;
		entryData.Contents = new ContentsSound(this, soundCode);
		entryData.AutoDestroy = true;
		EntryList.Add(entryData);
		entryData.Contents.Play(entryData.ExtraTransform, (localTimer == null) ? LocalTimer : localTimer, onComplete);
		return entryData.Contents;
	}

	public void PlayEntry(int entryIndex, Action onComplete)
	{
		ReserveContents();
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else if (valid.Contents != null)
		{
			valid.Contents.Play(valid.ExtraTransform, LocalTimer, onComplete);
		}
	}

	public bool IsPlayEntry(int entryIndex)
	{
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid == null)
		{
			return false;
		}
		if (valid.Contents == null)
		{
			return false;
		}
		if (valid.Contents.GetInstanceCount() == 0)
		{
			return false;
		}
		return true;
	}

	public void PlayNow()
	{
		Play(null);
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
			EntryData entryData = EntryList[i];
			if (entryData.Contents == null)
			{
				continue;
			}
			playCount++;
			entryData.Contents.Play(entryData.ExtraTransform, LocalTimer, delegate
			{
				completeCount++;
				if (playCount == completeCount)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete);
				}
			});
		}
	}

	public void PlayEntryPhase(int entryIndex, string phaseName)
	{
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid != null && valid.Contents != null)
		{
			valid.Contents.PlayPhase(phaseName);
		}
	}

	public void PlayPhase(string phaseName)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.PlayPhase(phaseName);
			}
		}
	}

	public void StopEntry(int entryIndex, bool immediatelyClear)
	{
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid != null && valid.Contents != null)
		{
			valid.Contents.Stop(immediatelyClear);
		}
	}

	public void Stop(bool immediatelyClear)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.Stop(immediatelyClear);
			}
		}
	}

	public bool ISDONE()
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			if (!EntryList[i].Contents.ISDONE())
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnStart()
	{
		base.OnStart();
		ReserveContents();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.ChainUpdate();
				if (entryData.Contents.GetInstanceCount() == 0 && entryData.AutoDestroy)
				{
					entryData.Contents.ReleaseAll();
					EntryList.RemoveAt(i);
					i--;
				}
			}
		}
	}

	protected override void OnDisable()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDisable();
			Stop(immediatelyClear: true);
		}
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
			if (entryData.Contents != null)
			{
				entryData.Contents.ReleaseAll();
				entryData.Contents = null;
			}
		}
	}

	public void ReserveCache(IResourcePreloader preloader)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.ContentsType == EContentsType.Effect)
			{
				preloader.ReserveCacheEffect(entryData.EventCode);
			}
			else if (entryData.ContentsType == EContentsType.Sound)
			{
				preloader.ReserveCacheSound(entryData.EventCode);
			}
		}
	}

	public void ReserveContents()
	{
		if (!_reserveContents)
		{
			_reserveContents = true;
			for (int i = 0; i < EntryList.Count; i++)
			{
				EntryData entry = EntryList[i];
				ReserveContents(entry);
			}
		}
	}

	private void ReserveContents(EntryData entry)
	{
		if (entry != null && entry.Contents == null)
		{
			if (entry.ContentsType == EContentsType.Effect)
			{
				entry.Contents = new ContentsEffect(this, entry.EventCode);
			}
			else if (entry.ContentsType == EContentsType.NodeTree)
			{
				entry.Contents = new ContentsNodeTree(this, entry.PackCode, entry.EventCode);
			}
			else if (entry.ContentsType == EContentsType.Sound)
			{
				entry.Contents = new ContentsSound(this, entry.EventCode);
			}
			else
			{
				entry.Contents = new ContentsInvalid();
			}
		}
	}

	public void AddEntryList(EntryData entryData)
	{
		EntryList.Add(entryData);
		ReserveContents(entryData);
	}

	public override int UpdateExtraRenderQueue(UIPanel panel, int renderQueue, bool isExplicit)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.UpdateExtraRenderQueue(panel, _renderQueueVisitor);
			}
		}
		return _renderQueueVisitor.UpdateExtraRenderQueue(renderQueue, isExplicit);
	}

	public override void Invalidate(bool includeChildren)
	{
		base.Invalidate(includeChildren);
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.UpdateAlpha(this);
			}
		}
	}
}
