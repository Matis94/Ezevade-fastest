using System.Linq;
using Aimtec;
using Aimtec.SDK.Util.Cache;

//using SharpDX;

namespace zzzz.SpecialSpells
{
    internal class Orianna : ChampionPlugin
    {
        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "OrianaIzunaCommand")
            {
                var hero = GameObjects.Heroes.FirstOrDefault(h => h.ChampionName == "Orianna");
                if (hero != null && hero.CheckTeam())
                {
                    var info = new ObjectTrackerInfo(hero)
                    {
                        Name = "TheDoomBall",
                        OwnerNetworkID = hero.NetworkId
                    };

                    ObjectTracker.objTracker.Add(hero.NetworkId, info);

                    GameObject.OnCreate += obj => OnCreateObj_OrianaIzunaCommand(obj, hero);
                    //Obj_AI_Minion.OnDelete += (obj, args) => OnDeleteObj_OrianaIzunaCommand(obj, args, hero);
                    Obj_AI_Base.OnProcessSpellCast += ProcessSpell_OrianaRedactCommand;
                    SpellDetector.OnProcessSpecialSpell += ProcessSpell_OrianaIzunaCommand;
                    BuffManager.OnAddBuff += Obj_AI_Base_OnBuffAdd;
                }
            }
        }


        private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Buff buff)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null && hero.CheckTeam())
                if (buff.Name == "orianaghostself")
                    foreach (var entry in ObjectTracker.objTracker)
                    {
                        var info = entry.Value;
                        if (entry.Value.Name == "TheDoomBall")
                        {
                            info.usePosition = false;
                            info.obj = hero;
                        }
                    }
        }

        private static void OnCreateObj_OrianaIzunaCommand(GameObject obj, Obj_AI_Hero hero)
        {
            if (obj.Name.Contains("Orianna") && obj.Name.Contains("Ball_Flash_Reverse") && obj.CheckTeam())
                foreach (var entry in ObjectTracker.objTracker)
                {
                    var info = entry.Value;
                    if (entry.Value.Name == "TheDoomBall")
                    {
                        info.usePosition = false;
                        info.obj = hero;
                    }
                }
        }

        private static void OnDeleteObj_OrianaIzunaCommand(GameObject obj, Obj_AI_Hero hero)
        {
            if (obj.Name.Contains("Orianna") && obj.Name.Contains("ball_glow_red") && obj.CheckTeam())
                foreach (var entry in ObjectTracker.objTracker)
                {
                    var info = entry.Value;

                    if (entry.Value.Name == "TheDoomBall")
                    {
                        info.usePosition = false;
                        info.obj = hero;
                    }
                }
        }

        private static void ProcessSpell_OrianaRedactCommand(Obj_AI_Base hero,
            Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!hero.IsValid && hero.Type == GameObjectType.obj_AI_Hero)
                return;

            var champ = (Obj_AI_Hero) hero;

            if (champ.ChampionName == "Orianna" && champ.CheckTeam())
                if (args.SpellData.Name == "OrianaRedactCommand")
                    foreach (var entry in ObjectTracker.objTracker)
                    {
                        var info = entry.Value;

                        if (entry.Value.Name == "TheDoomBall")
                        {
                            info.usePosition = false;
                            info.obj = args.Target;
                        }
                    }
        }

        private static void ProcessSpell_OrianaIzunaCommand(Obj_AI_Base hero,
            Obj_AI_BaseMissileClientDataEventArgs args, SpellData spellData,
            SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "OrianaIzunaCommand")
            {
                foreach (var entry in ObjectTracker.objTracker)
                {
                    var info = entry.Value;

                    if (entry.Value.Name == "TheDoomBall")
                    {
                        if (info.usePosition)
                        {
                            SpellDetector.CreateSpellData(hero, info.position, args.End, spellData, null, 0, false);
                            SpellDetector.CreateSpellData(hero, info.position, args.End, spellData, null, 150, true,
                                SpellType.Circular, false, spellData.secondaryRadius);
                        }
                        else
                        {
                            if (info.obj != null && info.obj.IsValid && !info.obj.IsDead)
                            {
                                SpellDetector.CreateSpellData(hero, info.obj.Position, args.End, spellData, null, 0,
                                    false);
                                SpellDetector.CreateSpellData(hero, info.obj.Position, args.End, spellData, null, 150,
                                    true, SpellType.Circular, false, spellData.secondaryRadius);
                            }
                        }

                        info.position = args.End;
                        info.usePosition = true;
                    }
                }

                specialSpellArgs.noProcess = true;
            }

            if (spellData.spellName == "OrianaDetonateCommand" || spellData.spellName == "OrianaDissonanceCommand")
            {
                foreach (var entry in ObjectTracker.objTracker)
                {
                    var info = entry.Value;

                    if (entry.Value.Name == "TheDoomBall")
                        if (info.usePosition)
                        {
                            var endPos2 = info.position;
                            SpellDetector.CreateSpellData(hero, endPos2, endPos2, spellData, null, 0);
                        }
                        else
                        {
                            if (info.obj != null && info.obj.IsValid && !info.obj.IsDead)
                            {
                                var endPos2 = info.obj.Position;
                                SpellDetector.CreateSpellData(hero, endPos2, endPos2, spellData, null, 0);
                            }
                        }
                }

                specialSpellArgs.noProcess = true;
            }
        }
    }
}