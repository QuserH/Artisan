using System;
using System.Numerics;
using Artisan.IPC;
using Artisan.RawInformation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace Artisan.UI;

/// <summary>
/// 点击「从雇员提取」时弹出的数量选择框。
/// 上限为雇员库存总数，默认值为当前配方缺口。
/// </summary>
internal static class RetainerWithdrawPrompt
{
    private const string PopupId = "##ArtisanRetainerWithdraw";
    private static bool _openRequested;
    private static uint _itemId;
    private static int _amount = 1;
    private static int _maxAmount = 1;
    private static string _itemName = string.Empty;

    private static string T(string key) => L10n.Tr(key);
    private static string T(string key, params object[] args) => L10n.Tr(key, args);

    public static void Request(uint itemId, int defaultAmount, int maxAmount)
    {
        if (itemId == 0 || maxAmount <= 0)
        {
            return;
        }

        _itemId = itemId;
        _maxAmount = maxAmount;
        _amount = Math.Clamp(defaultAmount <= 0 ? 1 : defaultAmount, 1, _maxAmount);
        _itemName = itemId.NameOfItem();
        _openRequested = true;
    }

    public static void Draw()
    {
        if (_openRequested)
        {
            ImGui.OpenPopup(PopupId);
            _openRequested = false;
        }

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos + viewport.WorkSize / 2, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        using var popup = ImRaii.PopupModal(PopupId);
        if (!popup)
        {
            return;
        }

        ImGui.TextUnformatted(T("Withdraw from Retainer: {0}", _itemName));
        ImGui.TextDisabled(T("Retainer stock limit: {0}", _maxAmount));
        ImGui.SetNextItemWidth(180f);
        ImGui.InputInt(T("Amount"), ref _amount);
        _amount = Math.Clamp(_amount, 1, _maxAmount);
        ImGui.Spacing();

        if (ImGui.Button(T("Withdraw"), new Vector2(90f, 0f)))
        {
            RetainerInfo.RestockFromRetainers(_itemId, _amount);
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button(T("Cancel"), new Vector2(90f, 0f)))
        {
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }
}
