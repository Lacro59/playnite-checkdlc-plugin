using CheckDlc.Models;
using CommonPluginsShared.Plugins;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;

namespace CheckDlc.Services
{
    public class CheckDlcExport : PluginExportCsv<GameDlc>
    {
        /// <inheritdoc/>
        protected override string GetExportWindowTitle(string pluginName)
        {
            return string.Format(
                ResourceProvider.GetString("LOCCheckDlcWindowTitleFormat"),
                ResourceProvider.GetString("LOCCheckDlc"),
                ResourceProvider.GetString("LOCCommonExport"));
        }

        protected override Dictionary<string, string> GetHeader()
        {
            return new Dictionary<string, string>
            {
                { "GameName", ResourceProvider.GetString("LOCGameNameTitle") },
                { "Platform", ResourceProvider.GetString("LOCPlatformTitle") },
                { "DlcName", ResourceProvider.GetString("LOCCheckDlcDlc") },
                { "Price", ResourceProvider.GetString("LOCCheckDlcPrice") },
                { "IsOwned", ResourceProvider.GetString("LOCCheckDlcIsOwned") },
                { "IsManualOwned", ResourceProvider.GetString("LOCCheckDlcManuallyOwned") },
                { "IsHidden", ResourceProvider.GetString("LOCCheckDlcHidden") },
                { "DlcLink", ResourceProvider.GetString("LOCCheckDlcLink") },
                { "IsManualAdded", ResourceProvider.GetString("LOCCheckDlcManualAdded") }
            };
        }

        protected override IEnumerable<Dictionary<string, string>> GetRows(GameDlc item)
        {
            if (item?.Items == null || item.Items.Count == 0)
            {
                yield break;
            }

            string platform = item.Source?.Name ?? item.Platforms?.FirstOrDefault()?.Name ?? "Playnite";

            foreach (Dlc dlc in item.Items)
            {
                yield return new Dictionary<string, string>
                {
                    { "GameName", item.Name ?? string.Empty },
                    { "Platform", platform },
                    { "DlcName", dlc.Name ?? string.Empty },
                    { "Price", dlc.Price ?? string.Empty },
                    { "IsOwned", dlc.IsOwned ? "X" : string.Empty },
                    { "IsManualOwned", dlc.IsManualOwned ? "X" : string.Empty },
                    { "IsHidden", dlc.IsHidden ? "X" : string.Empty },
                    { "DlcLink", dlc.Link ?? string.Empty },
                    { "IsManualAdded", item.IsManual ? "X" : string.Empty }
                };
            }
        }
    }
}
