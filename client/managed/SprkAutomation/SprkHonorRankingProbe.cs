using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NHonoraryChamber;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

internal static class SprkHonorRankingProbe
{
	internal static List<object> Run(bool verify)
	{
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" ||
			!(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE") ?? "").StartsWith("honor-ranking-"))
			throw new InvalidOperationException("Honor ranking probe requires isolated Login and an honor-ranking profile");
		NVespa.SingletonAuto<NClientData.LocalizationLoader>.instance.SetEnableLoadFromPatch(true);
		var common = SceneAutoBehaviour<CommonManagement>.instance;
		var window = common.GetInstance<HonoraryChamber>();
		if (window == null)
		{
			common.AddInstance<HonoraryChamber>("Resources/Prefab2/Window/Lobby/HonoraryChamber");
			window = common.GetInstance<HonoraryChamber>();
		}
		window.enabled = false;
		window.gameObject.SetActive(true);
		window.ShowRoot(true);
		window.UserName.TransformRoot.gameObject.SetActive(true);
		window.GuildName.TransformRoot.gameObject.SetActive(false);
		window.UserName.SetLevel(200);
		window.UserName.SetName("Lukos");
		var rows = Enumerable.Range(0, 8).Select(rank => new RecordsOfHonorRankingInfo
		{
			Id = rank + 1, Rank = rank, Level = 200, Name = rank == 0 ? "Lukos" : "Ranker " + (rank + 1),
			RankingType = RecordsOfHonorRankingType.Server, ContentType = RecordsOfHonorContentType.Match,
			Season = 1, CountryCode = ""
		}).ToArray();
		typeof(HonoraryChamber).GetMethod("SetRankingList", BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(window, new object[] { rows, rows[0] });
		var report = new List<object>();
		if (verify)
		{
			foreach (NameUI nameUI in new[] { window.UserName, window.GuildName })
			{
				nameUI.TransformRoot.gameObject.SetActive(true);
				foreach (var sample in new[] { Tuple.Create(1, "A"), Tuple.Create(99, "Periwinkles"), Tuple.Create(200, "Lukos"), Tuple.Create(1000, "Long Guild Name"), Tuple.Create(200, "Lukos") })
				{
					nameUI.SetLevel(sample.Item1);
					nameUI.SetName(sample.Item2);
					Bounds level = NGUIMath.CalculateRelativeWidgetBounds(nameUI.TransformRoot, nameUI.LabelLv[0].transform, true);
					Bounds name = NGUIMath.CalculateRelativeWidgetBounds(nameUI.TransformRoot, nameUI.LabelName[0].transform, true);
					Bounds combined = level; combined.Encapsulate(name);
					if (name.min.x - level.max.x < 4f || Mathf.Abs(combined.center.x) > 1f)
						throw new Exception("Honor name is overlapping or off center: " + sample + " gap=" + (name.min.x - level.max.x) + " center=" + combined.center.x);
					if (nameUI.LabelLv.Any(label => label.text != "Lv. " + sample.Item1) || nameUI.LabelName.Any(label => label.text != sample.Item2))
						throw new Exception("Honor name shadow labels were not updated");
					report.Add("PASS: " + nameUI.TransformRoot.name + " " + sample + " centered with gap=" + (name.min.x - level.max.x));
				}
			}
			window.GuildName.TransformRoot.gameObject.SetActive(false);
			window.UserName.SetLevel(200); window.UserName.SetName("Lukos");
		}
		report.Add(new Dictionary<string, object>
		{
			{ "userRoot", window.UserName.TransformRoot.name },
			{ "tables", window.UserName.TransformRoot.GetComponentsInChildren<UITableHorizontal>(true).Select(t => t.name + ":" + t.pivot).ToArray() },
			{ "level", Describe(window.UserName.LabelLv) },
			{ "name", Describe(window.UserName.LabelName) },
			{ "guildLevel", Describe(window.GuildName.LabelLv) },
			{ "guildName", Describe(window.GuildName.LabelName) }
		});
		var components = window.GridRank.GetComponentsInChildren<HonoraryChamberRankComponent>(true);
		if (verify && (components.Length != 8 || components.Any(c => c.LabelRank.text != (c.RankingInfo.Rank + 1).ToString())))
			throw new Exception("Native honor rank rows do not display ranks 1 through 8");
		report.Add("Native rank labels: " + string.Join(",", components.Select(c => c.LabelRank.text).ToArray()));
		if (verify)
		{
			foreach (RecordsOfHonorContentType content in new[] { RecordsOfHonorContentType.WorldBoss, RecordsOfHonorContentType.GuildSuppress, RecordsOfHonorContentType.Match, RecordsOfHonorContentType.BanPick })
			{
				foreach (RecordsOfHonorRankingType type in new[] { RecordsOfHonorRankingType.Server, RecordsOfHonorRankingType.World })
				{
					foreach (var row in rows) { row.ContentType = content; row.RankingType = type; }
					typeof(HonoraryChamber).GetMethod("SetRankingList", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(window, new object[] { rows, rows[0] });
					components = window.GridRank.GetComponentsInChildren<HonoraryChamberRankComponent>(true);
					if (components.Length != 8 || components.Any(c => c.LabelRank.text != (c.RankingInfo.Rank + 1).ToString()))
						throw new Exception("Honor rank numbering regressed for " + content + " " + type);
					report.Add("PASS: native " + content + " " + type + " ranks 1-8 on row reuse");
				}
			}
		}
		return report;
	}

	private static object[] Describe(UILabel[] labels)
	{
		return labels.Select(label => (object)new Dictionary<string, object>
		{
			{ "label", label.name }, { "parent", label.transform.parent.name },
			{ "pivot", label.pivot.ToString() }, { "overflow", label.overflowMethod.ToString() },
			{ "width", label.width }, { "printedWidth", label.printedSize.x },
			{ "x", label.transform.localPosition.x },
			{ "leftAnchor", label.leftAnchor.target == null ? "" : label.leftAnchor.target.name },
			{ "rightAnchor", label.rightAnchor.target == null ? "" : label.rightAnchor.target.name }
		}).ToArray();
	}
}
