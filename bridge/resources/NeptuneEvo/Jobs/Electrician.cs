using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEvo.GUI;
using NeptuneEvo.Core;
using Redage.SDK;

namespace NeptuneEvo.Jobs
{
    class Electrician : Script
    {
        private static int checkpointPayment = 25;
        private static nLog Log = new nLog("Electrician");

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                NAPI.TextLabel.CreateTextLabel("~r~Павел Ливинский", new Vector3(2281.31, 2983.57, 46.58), 30f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

                #region Objects Delete
                NAPI.World.DeleteWorldProp(-1767254195, new Vector3(2279.57, 2972.47, 45.58), 30f);
                
                #endregion

                var col = NAPI.ColShape.CreateCylinderColShape(new Vector3(2281.31, 2983.57, 45.58), 1, 2, 0);
                col.OnEntityEnterColShape += (shape, player) => {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 8);
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                col.OnEntityExitColShape += (shape, player) => {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                };
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~r~Начать/Закончить рабочий день"), new Vector3(2281.31, 2983.57, 46.58), 30f, 0.4f, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(1, new Vector3(2281.31, 2983.57, 45.58) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));

                int i = 0;
                foreach (var Check in Checkpoints)
                {
                    col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 1, 2, 0);
                    col.SetData("NUMBER", i);
                    col.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    i++;
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void StartWorkDay(Client player)
        {
            if (Main.Players[player].WorkID != 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не работаете электриком. Устроиться можно в мэрии", 3000);
                return;
            }
            if (player.GetData("ON_WORK"))
            {
                Customization.ApplyCharacter(player);
                player.SetData("ON_WORK", false);
                Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                Trigger.ClientEvent(player, "deleteWorkBlip");
                
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                //player.SetData("PAYMENT", 0);
                return;
            }
            else
            {
                Customization.ClearClothes(player, Main.Players[player].Gender);
                if (Main.Players[player].Gender)
                {
                    player.SetAccessories(1, 24, 2);
                    player.SetClothes(3, 16, 0);
                    player.SetClothes(11, 153, 10);
                    player.SetClothes(4, 0, 5);
                    player.SetClothes(6, 24, 0);
                }
                else
                {
                    player.SetAccessories(1, 26, 2);
                    player.SetClothes(3, 17, 0);
                    player.SetClothes(11, 150, 1);
                    player.SetClothes(4, 1, 5);
                    player.SetClothes(6, 52, 0);
                }

                var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                player.SetData("WORKCHECK", check);
                Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position, 1, 0, 255, 0, 0);
                Trigger.ClientEvent(player, "createWorkBlip", Checkpoints[check].Position);

                player.SetData("ON_WORK", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы начали рабочий день", 3000);
                return;
            }
        }

        private static List<Checkpoint> Checkpoints = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(2279.57, 2972.47, 45.58), 271.26),
            new Checkpoint(new Vector3(2278.41, 2952.80, 45.58), 240.73),
            new Checkpoint(new Vector3(2293.14, 2965.50, 45.58), 76.43),
            new Checkpoint(new Vector3(2270.97, 2965.85, 45.58), 79.31),
            new Checkpoint(new Vector3(2264.00, 2976.09, 45.58), 179.10),
            new Checkpoint(new Vector3(2264.36, 2962.03, 45.58), 349.43),
            new Checkpoint(new Vector3(2282.10, 2962.56, 45.58), 359.04),
            new Checkpoint(new Vector3(2281.27, 2946.70, 45.58), 0.03),
        };

        private static void PlayerEnterCheckpoint(ColShape shape, Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != 1 || !player.GetData("ON_WORK") || shape.GetData("NUMBER") != player.GetData("WORKCHECK")) return;

                if (Checkpoints[(int)shape.GetData("NUMBER")].Position.DistanceTo(player.Position) > 3) return;

                var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                //player.SetData("PAYMENT", player.GetData("PAYMENT") + payment);
                MoneySystem.Wallet.Change(player, payment);
                GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"electricianCheck");

                NAPI.Entity.SetEntityPosition(player, Checkpoints[shape.GetData("NUMBER")].Position + new Vector3(0, 0, 1.2));
                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, Checkpoints[shape.GetData("NUMBER")].Heading));
                Main.OnAntiAnim(player);
                player.PlayAnimation("amb@prop_human_movie_studio_light@base", "base", 39);
                player.SetData("WORKCHECK", -1);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null && Main.Players.ContainsKey(player))
                        {
                            player.StopAnimation();
                            Main.OffAntiAnim(player);
                            var nextCheck = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                            while (nextCheck == shape.GetData("NUMBER")) nextCheck = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                            player.SetData("WORKCHECK", nextCheck);
                            Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints[nextCheck].Position, 1, 0, 255, 0, 0);
                            Trigger.ClientEvent(player, "createWorkBlip", Checkpoints[nextCheck].Position);
                        }
                    }
                    catch { }
                }, 4000);

            } catch (Exception e) { Log.Write("PlayerEnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }

        internal class Checkpoint
        {
            public Vector3 Position { get; }
            public double Heading { get; }

            public Checkpoint(Vector3 pos, double rot)
            {
                Position = pos;
                Heading = rot;
            }
        }
    }
}
