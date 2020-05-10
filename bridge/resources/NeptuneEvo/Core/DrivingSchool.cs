using System.Collections.Generic;
using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEvo.GUI;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class DrivingSchool : Script
    {
        // мотоциклы, легковые машины, грузовые, водный, вертолёты, самолёты
        private static List<int> LicPrices = new List<int>() { 600, 1000, 3000, 6000, 10000, 10000 };
       // private static Vector3 enterSchool = new Vector3(-712.2147, -1298.926, 4.101922); //Действие автошколы,взаимодействие автошколы
        private static Vector3 enterSchool = new Vector3(228.56, 373.65, 105.11); //Действие автошколы,взаимодействие автошколы
        private static List<Vector3> startCourseCoord = new List<Vector3>()
        {
            new Vector3(209.51, 375.84, 106.01),  //Спавн авто при сдаче экзамена в автошколе
            new Vector3(209.51, 375.84, 106.01),
            new Vector3(209.51, 375.84, 106.01),
        };
        private static List<Vector3> startCourseRot = new List<Vector3>()
        {
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
        };
        private static List<Vector3> drivingCoords = new List<Vector3>()
        {
            new Vector3(200.84, 364.54, 105.83),     //as1
            new Vector3(136.08, 361.63, 110.76),     //as2
            new Vector3(90.22, 337.02, 110.78),     //as3
            new Vector3(37.22, 281.73, 108.34),     //as4
            new Vector3(53.35, 240.66, 108.36),     //as5
            new Vector3(143.65, 207.27, 105.32),     //as6
            new Vector3(218.84, 180.89, 103.13),     //as7
            new Vector3(338.66, 145.57, 101.99),     //as8
            new Vector3(375.70, 162.82, 101.77),     //as9
            new Vector3(411.87, 276.53, 101.79),     //as10
            new Vector3(401.48, 305.30, 101.74),     //as11
            new Vector3(333.55, 324.03, 103.72),     //as12
            new Vector3(278.04, 337.37, 104.22),     //as13
            new Vector3(225.94, 356.07, 104.52),     //as14
        };

        private static nLog Log = new nLog("DrivingSc");
        
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var shape = NAPI.ColShape.CreateCylinderColShape(enterSchool, 1, 2, 0);
                shape.OnEntityEnterColShape += onPlayerEnterSchool;
                shape.OnEntityExitColShape += onPlayerExitSchool;

                NAPI.Marker.CreateMarker(1, enterSchool - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Авто школа"), new Vector3(enterSchool.X, enterSchool.Y, enterSchool.Z + 1), 5f, 0.3f, 0, new Color(255, 255, 255));
                var blip = NAPI.Blip.CreateBlip(enterSchool, 0);
                blip.ShortRange = true;
                blip.Name = Main.StringToU16("Автошкола");
                blip.Sprite = 545; //Блип автошкола
                blip.Color = 29;
                for (int i = 0; i < drivingCoords.Count; i++)
                {
                    var colshape = NAPI.ColShape.CreateCylinderColShape(drivingCoords[i], 4, 5, 0);
                    colshape.OnEntityEnterColShape += onPlayerEnterDrive;
                    colshape.SetData("NUMBER", i);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("SCHOOLVEH") && player.GetData("SCHOOLVEH") == vehicle)
                {
                    //player.SetData("SCHOOL_TIMER", Main.StartT(60000, 99999999, (o) => timer_exitVehicle(player), "SCHOOL_TIMER"));
                    player.SetData("SCHOOL_TIMER", Timers.StartOnce(60000, () => timer_exitVehicle(player)));

                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если вы не сядете в машину в течение 60 секунд, то провалите экзамен", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private void timer_exitVehicle(Client player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    if (!player.HasData("SCHOOLVEH")) return;
                    if (player.IsInVehicle && player.Vehicle == player.GetData("SCHOOLVEH")) return;
                    NAPI.Entity.DeleteEntity(player.GetData("SCHOOLVEH"));
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    player.ResetData("IS_DRIVING");
                    player.ResetData("SCHOOLVEH");
                    //Main.StopT(player.GetData("SCHOOL_TIMER"), "timer_36");
                    Timers.Stop(player.GetData("SCHOOL_TIMER"));
                    player.ResetData("SCHOOL_TIMER");
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы провалили экзмен", 3000);
                }
                catch (Exception e) { Log.Write("TimerDrivingSchool: " + e.Message, nLog.Type.Error); }
            });
        }

        public static void onPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player.HasData("SCHOOLVEH")) NAPI.Entity.DeleteEntity(player.GetData("SCHOOLVEH"));
                }
                catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
            }, 0);
        }
        public static void startDrivingCourse(Client player, int index)
        {
            if (player.HasData("IS_DRIVING") || player.GetData("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете сделать это сейчас", 3000);
                return;
            }
            if (Main.Players[player].Licenses[index])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть эта лицензия", 3000);
                return;
            }
            switch (index)
            {
                case 0:
                    if (Main.Players[player].Money < LicPrices[0])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    var vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Chino2, startCourseCoord[0], startCourseRot[0], 38, 38);
                    player.SetIntoVehicle(vehicle, -1);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 0);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[0]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[0];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[0], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести транспорт, нажмите 2", 3000);
                    return;
                case 1:
                    if (Main.Players[player].Money < LicPrices[1])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Panto, startCourseCoord[0], startCourseRot[0], 38, 38);
                    player.SetIntoVehicle(vehicle, -1);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 1);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[1]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[1];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[1], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести транспорт, нажмите 2", 3000);
                    return;
                case 2:
                    if (Main.Players[player].Money < LicPrices[2])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Packer, startCourseCoord[0], startCourseRot[0], 38, 38);
                    player.SetIntoVehicle(vehicle, -1);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 2);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[2]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[2];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[2], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести транспорт, нажмите 2", 3000);
                    return;
                case 3:
                    if (Main.Players[player].Money < LicPrices[3])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[3] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[3]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[3];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[3], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию на водный транспорт", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 4:
                    if (Main.Players[player].Money < LicPrices[4])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"", 3000);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[4] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[4]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[4];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[4], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию управление вертолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 5:
                    if (Main.Players[player].Money < LicPrices[5])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[5] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[5]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[5];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[5], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию управление самолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
            }
        }
        private void onPlayerEnterSchool(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 39);
            }
            catch (Exception e) { Log.Write("onPlayerEnterSchool: " + e.ToString(), nLog.Type.Error); }
        }
        private void onPlayerExitSchool(ColShape shape, Client player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        private void onPlayerEnterDrive(ColShape shape, Client player)
        {
            try
            {
                if (!player.IsInVehicle || player.VehicleSeat != -1) return;
                if (!player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData("ACCESS") != "SCHOOL") return;
                if (!player.HasData("IS_DRIVING")) return;
                if (player.Vehicle != player.GetData("SCHOOLVEH")) return;
                if (shape.GetData("NUMBER") != player.GetData("CHECK")) return;
                //Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                var check = player.GetData("CHECK");
                if (check == drivingCoords.Count - 1)
                {
                    player.ResetData("IS_DRIVING");
                    var vehHP = player.Vehicle.Health;
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(player.Vehicle);
                        } catch { }
                    });
                    player.ResetData("SCHOOLVEH");
                    if (vehHP < 500)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы провалили экзамен", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[player.GetData("LICENSE")] = true;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно сдали экзамен", 3000);
                    Dashboard.sendStats(player);
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    return;
                }

                player.SetData("CHECK", check + 1);
                if (check + 2 < drivingCoords.Count)
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0, drivingCoords[check + 2] - new Vector3(0, 0, 1.12));
                else
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                Trigger.ClientEvent(player, "createWaypoint", drivingCoords[check + 1].X, drivingCoords[check + 1].Y);
            }
            catch (Exception e)
            {
                Log.Write("ENTERDRIVE:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        #region menu
        public static void OpenDriveSchoolMenu(Client player)
        {
            Menu menu = new Menu("driveschool", false, false);
            menu.Callback = callback_driveschool;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Лицензии";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_0", Menu.MenuItem.Button);
            menuItem.Text = $"(A) Мотоциклы - {LicPrices[0]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_1", Menu.MenuItem.Button);
            menuItem.Text = $"(B) Легковые машины - {LicPrices[1]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_2", Menu.MenuItem.Button);
            menuItem.Text = $"(C) Грузовые машины - {LicPrices[2]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_3", Menu.MenuItem.Button);
            menuItem.Text = $"(V) Водный транспорт - {LicPrices[3]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_4", Menu.MenuItem.Button);
            menuItem.Text = $"(LV) Вертолёты - {LicPrices[4]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_5", Menu.MenuItem.Button);
            menuItem.Text = $"(LS) Самолёты - {LicPrices[5]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_driveschool(Client client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(client);
            if (item.ID == "close") return;
            var id = item.ID.Split('_')[1];
            startDrivingCourse(client, Convert.ToInt32(id));
            return;
        }
        #endregion
    }
}