using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz
{
    public class ObjectTrackerInfo
    {
        public Vector3 direction;
        public string Name;
        public GameObject obj;
        public Dictionary<int, GameObject> objList = new Dictionary<int, GameObject>();
        public int OwnerNetworkID;
        public Vector3 position;
        public float timestamp;
        public bool usePosition;

        public ObjectTrackerInfo(GameObject obj)
        {
            this.obj = obj;
            Name = obj.Name;
            timestamp = EvadeUtils.TickCount;
        }

        public ObjectTrackerInfo(GameObject obj, string name)
        {
            this.obj = obj;
            Name = name;
            timestamp = EvadeUtils.TickCount;
        }

        public ObjectTrackerInfo(string name, Vector3 position)
        {
            Name = name;
            usePosition = true;
            this.position = position;

            timestamp = EvadeUtils.TickCount;
        }
    }

    public static class ObjectTracker
    {
        public static Dictionary<int, ObjectTrackerInfo> objTracker = new Dictionary<int, ObjectTrackerInfo>();
        public static int objTrackerID;
        private static bool _loaded;

        static ObjectTracker()
        {
            GameObject.OnCreate += HiuCreate_ObjectTracker;
            //Obj_AI_Minion.OnCreate += HiuDelete_ObjectTracker;

            _loaded = true;
        }

        public static void HuiTrackerForceLoad()
        {
            if (!_loaded)
            {
                GameObject.OnCreate += HiuCreate_ObjectTracker;
                _loaded = true;
            }
        }

        public static void AddObjTrackerPosition(string name, Vector3 position, float timeExpires)
        {
            objTracker.Add(objTrackerID, new ObjectTrackerInfo(name, position));

            var trackerID = objTrackerID; //store the id for deletion
            DelayAction.Add((int) timeExpires, () => objTracker.Remove(objTrackerID));

            objTrackerID += 1;
        }

        private static void HiuCreate_ObjectTracker(GameObject obj)
        {
            if (!objTracker.ContainsKey(obj.NetworkId))
            {
                var minion = obj as Obj_AI_Minion;
                if (minion != null && minion.CheckTeam() && minion.UnitSkinName.ToLower().Contains("testcube"))
                {
                    objTracker.Add(obj.NetworkId, new ObjectTrackerInfo(obj, "hiu"));
                    DelayAction.Add(250, () => objTracker.Remove(obj.NetworkId));
                }
            }
        }

        private static void HiuDelete_ObjectTracker(GameObject obj, EventArgs args)
        {
            if (objTracker.ContainsKey(obj.NetworkId))
                objTracker.Remove(obj.NetworkId);
        }

        public static Vector2 GetLastHiuOrientation()
        {
            var objList = objTracker.Values.Where(o => o.Name == "hiu");
            var sortedObjList = objList.OrderByDescending(o => o.timestamp);

            if (sortedObjList.Count() >= 2)
            {
                var pos1 = sortedObjList.First().obj.Position;
                var pos2 = sortedObjList.ElementAt(1).obj.Position;

                return (pos2.To2D() - pos1.To2D()).Normalized();
            }

            return Vector2.Zero;
        }
    }
}