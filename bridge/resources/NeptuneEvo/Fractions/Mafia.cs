using System.Collections.Generic;
using GTANetworkAPI;

namespace NeptuneEvo.Fractions
{
    class Mafia : Script
    {
        public static Dictionary<int, Vector3> EnterPoints = new Dictionary<int, Vector3>()
        {
           // { 10, new Vector3(1392.066, 1153.032, 113.3233) },// вход на склад итальянцев
            //{ 11, new Vector3(-113.4213, 985.761, 234.6341) }, // вход на склад русских
            { 12, new Vector3(-1549.331, -90.05454, 53.80917) }, // вход на склад якудзе
            { 13, new Vector3(-1805.049, 438.1696, 127.5874) }, // вход на склад армяне
        };
        public static Dictionary<int, Vector3> ExitPoints = new Dictionary<int, Vector3>()
        {
            //{ 10, new Vector3(1396.62, 1142.823, 83.24014) },// выход со склада итальянцев
            //{ 11, new Vector3(-123.8163, 975.3881, 58.63158) }, // выход со склада русских
            { 12, new Vector3(-1550.298, -94.81767, -193.2058) }, // выход со склада якудзе
            { 13, new Vector3(-1812.82, 466.4906, -185.7867) }, // выход со склада армяне
        };

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
           // Нпс мафии, //npc mafia
            NAPI.TextLabel.CreateTextLabel("~r~Вова Медведев", new Vector3(-115.33, 987.30, 236.754), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); //Русские
            NAPI.TextLabel.CreateTextLabel("~r~Бабкен Хасагян", new Vector3(-1811.368, 438.4105, 129.7074), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); //Армяне
            NAPI.TextLabel.CreateTextLabel("~r~Энрике Крус", new Vector3(-1521.50, 851.42, 182.59), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); //Мексиканский Картель
            NAPI.TextLabel.CreateTextLabel("~r~Соломон Карбоне", new Vector3(1392.098, 1155.892, 115.4433), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); //Итальянцы

            foreach (var point in EnterPoints)
            {
                NAPI.Marker.CreateMarker(1, point.Value - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, NAPI.GlobalDimension);

                var col = NAPI.ColShape.CreateCylinderColShape(point.Value, 1.2f, 2, NAPI.GlobalDimension);
                col.SetData("FRAC", point.Key);

                col.OnEntityEnterColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("FRACTIONCHECK", s.GetData("FRAC"));
                    e.SetData("INTERACTIONCHECK", 64);
                };
                col.OnEntityExitColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("INTERACTIONCHECK", -1);
                };
            }

            foreach (var point in ExitPoints)
            {
                NAPI.Marker.CreateMarker(1, point.Value - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, NAPI.GlobalDimension);

                var col = NAPI.ColShape.CreateCylinderColShape(point.Value, 1.2f, 2, NAPI.GlobalDimension);
                col.SetData("FRAC", point.Key);

                col.OnEntityEnterColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("FRACTIONCHECK", s.GetData("FRAC"));
                    e.SetData("INTERACTIONCHECK", 65);
                };
                col.OnEntityExitColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("INTERACTIONCHECK", -1);
                };
            }
        }
    }
}
