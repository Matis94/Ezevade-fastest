using Aimtec;
using Aimtec.SDK.Extensions;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Zed : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "ZedQ")
            {
                SpellDetector.OnProcessSpecialSpell += ProcessSpell_ZedShuriken;
                GameObject.OnCreate += SpellMissile_ZedShadowDash;
                GameObject.OnCreate += OnCreateObj_ZedShuriken;
                GameObject.OnDestroy += OnDeleteObj_ZedShuriken;
            }
        }

        private static void OnCreateObj_ZedShuriken(GameObject obj)
        {
            if (obj.Name == "Shadow" && obj.IsEnemy)
                if (!ObjectTracker.objTracker.ContainsKey(obj.NetworkId))
                {
                    ObjectTracker.objTracker.Add(obj.NetworkId, new ObjectTrackerInfo(obj));

                    foreach (var entry in ObjectTracker.objTracker)
                    {
                        var info = entry.Value;

                        if (info.Name == "Shadow" && info.usePosition && info.position.Distance(obj.Position) < 5)
                        {
                            info.Name = "Shadow";
                            info.usePosition = false;
                            info.obj = obj;
                        }
                    }
                }
        }

        private static void OnDeleteObj_ZedShuriken(GameObject obj)
        {
            if (obj != null && obj.Name == "Shadow")
                ObjectTracker.objTracker.Remove(obj.NetworkId);
        }

        private static void ProcessSpell_ZedShuriken(Obj_AI_Base hero, Obj_AI_BaseMissileClientDataEventArgs args,
            SpellData spellData,
            SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "ZedQ")
                foreach (var entry in ObjectTracker.objTracker)
                {
                    var info = entry.Value;

                    if (info.Name == "Shadow")
                        if (info.usePosition == false && (info.obj == null || !info.obj.IsValid || info.obj.IsDead))
                        {
                            DelayAction.Add(1, () => ObjectTracker.objTracker.Remove(info.obj.NetworkId));
                        }
                        else
                        {
                            Vector3 endPos2;
                            if (info.usePosition == false)
                            {
                                endPos2 = info.obj.Position.Extend(args.End, spellData.range);
                                SpellDetector.CreateSpellData(hero, info.obj.Position, endPos2, spellData, null, 0,
                                    false);
                            }
                            else
                            {
                                endPos2 = info.position.Extend(args.End, spellData.range);
                                SpellDetector.CreateSpellData(hero, info.position, endPos2, spellData, null, 0, false);
                            }
                        }
                }
        }

        private static void SpellMissile_ZedShadowDash(GameObject obj)
        {
            if (!obj.IsValid && obj.Type == GameObjectType.MissileClient)
                return;

            var missile = (MissileClient) obj;

            if (missile.SpellCaster.IsEnemy && missile.SpellData.Name == "ZedWMissile")
                if (!ObjectTracker.objTracker.ContainsKey(obj.NetworkId))
                {
                    var info = new ObjectTrackerInfo(obj);
                    info.Name = "Shadow";
                    info.OwnerNetworkID = missile.SpellCaster.NetworkId;
                    info.usePosition = true;
                    info.position = missile.EndPosition;

                    ObjectTracker.objTracker.Add(obj.NetworkId, info);

                    DelayAction.Add(1000, () => ObjectTracker.objTracker.Remove(obj.NetworkId));
                }
        }
    }
}