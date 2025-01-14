﻿using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using Lifestream.Services;
using Lifestream.Tasks.SameWorld;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = System.Action;

namespace Lifestream.Data;
[Serializable]
public class CustomAliasCommand
{
    internal string ID = Guid.NewGuid().ToString();
    public CustomAliasKind Kind;
    public Vector3 Point;
    public uint Aetheryte;
    public int World;
    public Vector2 CenterPoint;
    public Vector3 CircularExitPoint;
    public (float Min, float Max)? Clamp = null;
    public float Precision = 20f;
    public int Tolerance = 1;
    public bool WalkToExit = true;
    public float SkipTeleport = 15f;

    public void Enqueue(List<Vector3> appendMovement)
    {
        if(Kind == CustomAliasKind.Change_world)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            if(World != Player.Object.CurrentWorld.Id)
            {
                var world = ExcelWorldHelper.GetName(World);
                if(P.IPCProvider.CanVisitCrossDC(world))
                {
                    P.TPAndChangeWorld(world, true);
                }
                else if(P.IPCProvider.CanVisitSameDC(world))
                {
                    P.TPAndChangeWorld(world, false);
                }
            }
        }
        else if(Kind == CustomAliasKind.Walk_to_point)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            P.TaskManager.Enqueue(() => P.FollowPath.Move([Point, .. appendMovement], true));
            P.TaskManager.Enqueue(() => P.FollowPath.Waypoints.Count == 0);
        }
        else if(Kind == CustomAliasKind.Navmesh_to_point)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            P.TaskManager.Enqueue(() =>
            {
                var task = P.VnavmeshManager.Pathfind(Player.Position, Point, false);
                P.TaskManager.InsertMulti(
                    new(() => task.IsCompleted),
                    new(() => P.FollowPath.Move([..task.Result, .. appendMovement], true)),
                    new(() => P.FollowPath.Waypoints.Count == 0)
                    );
            });
        }
        else if(Kind == CustomAliasKind.Teleport_to_Aetheryte)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            P.TaskManager.Enqueue(() =>
            {
                var aetheryte = Svc.Data.GetExcelSheet<Aetheryte>().GetRow(Aetheryte);
                var nearestAetheryte = Svc.Objects.OrderBy(Player.DistanceTo).FirstOrDefault(x => x.IsTargetable && x.IsAetheryte());
                if(nearestAetheryte == null || Player.Territory != aetheryte.Territory.Row || Player.DistanceTo(nearestAetheryte) > this.SkipTeleport)
                {
                    P.TaskManager.InsertMulti(
                        new((Action)(() => S.TeleportService.TeleportToAetheryte(Aetheryte))),
                        new(() => !IsScreenReady()),
                        new(() => IsScreenReady())
                        );
                }
            });
        }
        else if(Kind == CustomAliasKind.Use_Aethernet)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            var aethernetPoint = Svc.Data.GetExcelSheet<Aetheryte>().GetRow(Aetheryte).AethernetName.Value.Name.ExtractText();
            TaskTryTpToAethernetDestination.Enqueue(aethernetPoint);
        }
        else if(Kind == CustomAliasKind.Circular_movement)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            P.TaskManager.Enqueue(() => P.FollowPath.Move([..MathHelper.CalculateCircularMovement(CenterPoint, Player.Position.ToVector2(), CircularExitPoint.ToVector2(), out _, Precision, Tolerance, Clamp).Select(x => x.ToVector3(Player.Position.Y)).ToList(), .. (Vector3[])(WalkToExit ? [CircularExitPoint] : []), .. appendMovement], true));
            P.TaskManager.Enqueue(() => P.FollowPath.Waypoints.Count == 0);
        }
    }
}
