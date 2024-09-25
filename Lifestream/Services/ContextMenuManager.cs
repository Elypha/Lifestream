﻿using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text;
using ECommons.ExcelServices;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lifestream.GUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifestream.Services;
public unsafe class ContextMenuManager : IDisposable
{
    private ContextMenuManager()
    {
        Svc.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened;
    }

    public void Dispose()
    {
        Svc.ContextMenu.OnMenuOpened -= ContextMenu_OnMenuOpened;
    }

    private void ContextMenu_OnMenuOpened(IMenuOpenedArgs args)
    {
        if(TryGetAddonMaster<AddonMaster._CharaSelectListMenu>(out var m))
        {
            if(args.Target is MenuTargetDefault target && m.Characters.TryGetFirst(x => x.IsSelected, out var chara))
            {
                args.AddMenuItem(ConstructMenuItemFor(chara.Name, chara.HomeWorld, chara.CurrentWorld));
                if(chara.HomeWorld != chara.CurrentWorld)
                {
                    args.AddMenuItem(ConstructReturnHomeMenuItemFor(chara.Name, chara.HomeWorld, chara.CurrentWorld));
                }
            }
        }
    }

    private MenuItem ConstructMenuItemFor(string name, uint homeWorld, uint currentWorld)
    {
        var ret = new MenuItem()
        {
            Name = "Lifestream",
            Prefix = (SeIconChar)'',
            OnClicked = (args) =>
            {
                P.CharaSelectOverlay.Open(name, homeWorld);
            }
        };
        return ret;
    }

    private MenuItem ConstructReturnHomeMenuItemFor(string name, uint homeWorld, uint currentWorld)
    {
        var ret = new MenuItem()
        {
            Name = "To Home World",
            Prefix = (SeIconChar)'',
            OnClicked = (args) =>
            {
                P.TaskManager.Enqueue(() => !(TryGetAddonByName<AtkUnitBase>("ContextMenu", out var c) && c->IsVisible));
                P.TaskManager.Enqueue(() => CharaSelectOverlay.Command(name, currentWorld, homeWorld, ExcelWorldHelper.Get(homeWorld)));
            }
        };
        return ret;
    }
}