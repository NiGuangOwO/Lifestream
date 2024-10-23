﻿using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using Lifestream.Tasks.SameWorld;
using Lumina.Excel.GeneratedSheets;
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
        if(Kind == CustomAliasKind.跨服)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            if(World != Player.Object.CurrentWorld.Id)
            {
                var world = ExcelWorldHelper.GetName(World);
                if(P.IPCProvider.CanVisitCrossDC(world))
                {
                    P.TPAndChangeWorld(world, true, skipChecks: true);
                }
                else if(P.IPCProvider.CanVisitSameDC(world))
                {
                    P.TPAndChangeWorld(world, false, skipChecks: true);
                }
            }
        }
        else if(Kind == CustomAliasKind.步行到坐标)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            P.TaskManager.Enqueue(() => P.FollowPath.Move([Point, .. appendMovement], true));
            P.TaskManager.Enqueue(() => P.FollowPath.Waypoints.Count == 0);
        }
        else if(Kind == CustomAliasKind.寻路到坐标)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable && P.VnavmeshManager.IsReady() == true);
            P.TaskManager.Enqueue(() =>
            {
                var task = P.VnavmeshManager.Pathfind(Player.Position, Point, false);
                P.TaskManager.InsertMulti(
                    new(() => task.IsCompleted),
                    new(() => P.FollowPath.Move([.. task.Result, .. appendMovement], true)),
                    new(() => P.FollowPath.Waypoints.Count == 0)
                    );
            });
        }
        else if(Kind == CustomAliasKind.传送到以太之光)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            P.TaskManager.Enqueue(() =>
            {
                var aetheryte = Svc.Data.GetExcelSheet<Aetheryte>().GetRow(Aetheryte);
                var nearestAetheryte = Svc.Objects.OrderBy(Player.DistanceTo).FirstOrDefault(x => x.IsTargetable && x.IsAetheryte());
                if(nearestAetheryte == null || Player.Territory != aetheryte.Territory.Row || Player.DistanceTo(nearestAetheryte) > SkipTeleport)
                {
                    P.TaskManager.InsertMulti(
                        new((Action)(() => S.TeleportService.TeleportToAetheryte(Aetheryte))),
                        new(() => !IsScreenReady()),
                        new(() => IsScreenReady())
                        );
                }
            });
        }
        else if(Kind == CustomAliasKind.使用传送网)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            var aethernetPoint = Svc.Data.GetExcelSheet<Aetheryte>().GetRow(Aetheryte).AethernetName.Value.Name.ExtractText();
            TaskTryTpToAethernetDestination.Enqueue(aethernetPoint);
            P.TaskManager.Enqueue(() => !IsScreenReady());
            P.TaskManager.Enqueue(() => IsScreenReady());
        }
        else if(Kind == CustomAliasKind.Circular_movement)
        {
            P.TaskManager.Enqueue(() => IsScreenReady() && Player.Interactable);
            P.TaskManager.Enqueue(() => P.FollowPath.Move([.. MathHelper.CalculateCircularMovement(CenterPoint, Player.Position.ToVector2(), CircularExitPoint.ToVector2(), out _, Precision, Tolerance, Clamp).Select(x => x.ToVector3(Player.Position.Y)).ToList(), .. (Vector3[])(WalkToExit ? [CircularExitPoint] : []), .. appendMovement], true));
            P.TaskManager.Enqueue(() => P.FollowPath.Waypoints.Count == 0);
        }
    }
}
