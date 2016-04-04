using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System;
using System.Linq;

namespace FunboxOrbwalking
{
    public class FunboxOrb
    {
        static Menu menu;

        static float lastaa;

        public static float bonusrange;

        static float lastmove;

        static bool reset = false;

        static float lastreset;

        static float resetdelay;

        static bool orbattack = true;

        static bool orbmove = true;
        
        static float cputime;
        
        static float cpuspeed;
        
        static bool cpustop = false;

        static bool customts = false;

        public static AttackableUnit CustomTargetSelector;

        public static void FunboxOrbInit()
        {
            menu = MainMenu.AddMenu("FunboxOrb", "funboxorb");
            menu.AddSeparator();
            menu.AddGroupLabel("CORE");
            menu.Add("orb", new CheckBox("Funbox Orbwalker Enabled"));
            menu.AddSeparator();
            menu.Add("ts", new CheckBox("Funbox TS Enabled"));
            menu.AddSeparator();
            menu.Add("kite", new Slider("Kite: MINUS[performance] PLUS[humanizer or if you have stutter]", 0, -25, 25));
            menu.AddSeparator();
            menu.Add("humanizer", new Slider("Move: MINUS[performance] PLUS[humanizer]", 150, 0, 300));
            menu.AddGroupLabel("Drawings");
            menu.Add("draw", new CheckBox("Enemy next move line"));
            menu.Add("playerrange", new CheckBox("Player Attack Range"));
            menu.Add("enemiesrange", new CheckBox("Enemies Attack Range", false));
            Game.OnUpdate += Game_OnTick;
            Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
            Spellbook.OnStopCast += Spellbook_OnStopCast;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (menu["draw"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var unit in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(1500f)))
                {
                    var start = Drawing.WorldToScreen(unit.Position);
                    var end = Drawing.WorldToScreen(unit.Path.LastOrDefault());
                    Drawing.DrawLine(start[0], start[1], end[0], end[1], 1, System.Drawing.Color.Red);
                    Drawing.DrawText(end[0], end[1], System.Drawing.Color.Red, unit.ChampionName);
                }
            }
            if (menu["playerrange"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.BoundingRadius + 65f + ObjectManager.Player.AttackRange, System.Drawing.Color.Green);
            }
            if (menu["enemiesrange"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var unit in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget()))
                {
                    Drawing.DrawCircle(unit.Position, ObjectManager.Player.BoundingRadius + unit.BoundingRadius + unit.AttackRange, System.Drawing.Color.Red);
                }
            }
        }

        static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                bonusrange = 200f;
                lastaa = Game.Time * 1000;
            }
        }

        static void Game_OnTick(EventArgs args)
        {
            if (menu["orb"].Cast<CheckBox>().CurrentValue)
            {
                ResetDelay(resetdelay);
                if (AfterAttack)
                {
                    bonusrange = 0f;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    if (Orbwalker.DisableAttacking)
                    {
                        Orbwalker.DisableAttacking = false;
                    }
                    if (Orbwalker.DisableMovement)
                    {
                        Orbwalker.DisableMovement = false;
                    }
                }
                else
                {
                    if (!Orbwalker.DisableAttacking)
                    {
                        Orbwalker.DisableAttacking = true;
                    }
                    if (!Orbwalker.DisableMovement)
                    {
                        Orbwalker.DisableMovement = true;
                    }
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Game.Time * 1000 > cputime + 35f)
                    {
                        cputime = Game.Time * 1000;
                        cpustop = false;
                    }
                    if (!cpustop && Game.Time * 1000 > cputime && Game.Time * 1000 <= cputime + 35f)
                    {
                        cpuspeed = Game.Time * 1000 - cputime;
                        cpustop = true;
                    }
                    if (Target != null)
                    {
                        if (ObjectManager.Player.ChampionName == "Graves")
                        {
                            if (orbattack && ObjectManager.Player.HasBuff("GravesBasicAttackAmmo1") && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                            }
                            else if (orbmove && Game.Time * 1000 > lastmove + menu["humanizer"].Cast<Slider>().CurrentValue && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - Game.Ping / 2.15 + (35f - cpuspeed) + menu["kite"].Cast<Slider>().CurrentValue)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                lastmove = Game.Time * 1000;
                            }
                        }
                        else if (ObjectManager.Player.ChampionName == "Jhin")
                        {
                            if (orbattack && !ObjectManager.Player.HasBuff("JhinPassiveReload") && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                            }
                            else if (orbmove && Game.Time * 1000 > lastmove + menu["humanizer"].Cast<Slider>().CurrentValue && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - Game.Ping / 2.15 + (35f - cpuspeed) + menu["kite"].Cast<Slider>().CurrentValue)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                lastmove = Game.Time * 1000;
                            }
                        }
                        else
                        {
                            if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                            }
                            else if (orbmove && Game.Time * 1000 > lastmove + menu["humanizer"].Cast<Slider>().CurrentValue && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - Game.Ping / 2.15 + (35f - cpuspeed) + menu["kite"].Cast<Slider>().CurrentValue)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                lastmove = Game.Time * 1000;
                            }
                        }
                    }
                    else if (orbmove && Game.Time * 1000 > lastmove + menu["humanizer"].Cast<Slider>().CurrentValue && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - Game.Ping / 2.15 + ObjectManager.Player.AttackSpeedMod * 15.2)
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        lastmove = Game.Time * 1000;
                    }
                }
            }
            else
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    if (Orbwalker.DisableAttacking)
                    {
                        Orbwalker.DisableAttacking = false;
                    }
                    if (Orbwalker.DisableMovement)
                    {
                        Orbwalker.DisableMovement = false;
                    }
                }
            }
        }

        static void Spellbook_OnStopCast(Obj_AI_Base sender, SpellbookStopCastEventArgs args)
        {
            if (sender.IsMe && (Game.Time * 1000) - lastaa < ObjectManager.Player.AttackCastDelay * 1000 + 50f && !args.ForceStop)
            {
                lastaa = 0f;
            }
        }

        public static bool AfterAttack
        {
            get
            {
                if (Game.Time * 1000 < lastaa + ObjectManager.Player.AttackDelay * 1000 - ObjectManager.Player.AttackDelay * 1000 / 2.35 && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 + 25f)
                {
                    return true;
                }
                return false;
            }
        }

        public static void EnableCustomTS()
        {
            customts = true;
        }

        public static void DisableCustomTS()
        {
            customts = false;
        }

        static AttackableUnit Target
        {
            get
            {
                if (customts)
                {
                    if (CustomTargetSelector != null)
                    {
                        return CustomTargetSelector;
                    }
                }
                else
                {
                    if (menu["ts"].Cast<CheckBox>().CurrentValue)
                    {
                        foreach (var unit in EntityManager.Heroes.Enemies.OrderBy(x => x.Health + x.Health * (x.Armor / (x.Armor + 100)) - x.TotalAttackDamage * x.AttackSpeedMod - x.TotalMagicalDamage).Where(x => x.IsValidTarget(bonusrange + ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + x.BoundingRadius) && !x.IsZombie && !x.HasBuff("JudicatorIntervention") && !x.HasBuff("ChronoShift") && !x.HasBuff("UndyingRage") && !x.HasBuff("JaxCounterStrike") && !ObjectManager.Player.HasBuff("BlindingDart")))
                        {
                            return unit;
                        }
                    }
                    else
                    {
                        return TargetSelector.GetTarget(ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 50f + bonusrange, DamageType.Physical);
                    }
                }
                return null;
            }
        }

        public static float TimeToNextAttack
        {
            get
            {
                if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15)
                {
                    return 0f;
                }
                return (float)((lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15) - (Game.Time * 1000));
            }
        }

        public static float TimeToNextMove
        {
            get
            {
                if (Game.Time * 1000 >= lastaa + ObjectManager.Player.AttackCastDelay * 1000 - Game.Ping / 2.15)
                {
                    return 0f;
                }
                return (float)((lastaa + ObjectManager.Player.AttackCastDelay * 1000 - Game.Ping / 2.15) - (Game.Time * 1000));
            }
        }

        public static void ResetAutoAttack(float delay)
        {
            resetdelay = delay;
            reset = true;
            lastreset = Game.Time * 1000;
        }

        static void ResetDelay(float delay)
        {
            if (reset)
            {
                delay = resetdelay;
                if (Game.Time * 1000 > lastreset + delay)
                {
                    lastaa = 0f;
                    if (lastaa < 1f)
                    {
                        reset = false;
                        resetdelay = Game.Time * 1000 * 2;
                    }
                }
            }
        }

        public static void DisableAttack()
        {
            orbattack = false;
        }

        public static void EnableAttack()
        {
            orbattack = true;
        }

        public static void DisableMove()
        {
            orbmove = false;
        }

        public static void EnableMove()
        {
            orbmove = true;
        }

        public static bool CanAttack
        {
            get
            {
                if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15)
                {
                    return true;
                }
                return false;
            }
        }

        public static bool CanMove
        {
            get
            {
                if ((Game.Time * 1000 < lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15 && Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - Game.Ping / 2.15 + ObjectManager.Player.AttackSpeedMod * 15.2) || Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - Game.Ping * 2.15)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
