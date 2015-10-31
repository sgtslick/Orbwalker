﻿namespace Orbwalker
{
    using System;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;

    using SharpDX;

    internal class Orbwalker
    {
        #region Constants

        private const Key FarmKey = Key.L;

        private const Key ChaseKey = Key.Z;

        private const Key KiteKey = Key.N;

        #endregion

        #region Static Fields

        private static Unit creepTarget;

        private static float lastRange;

        private static bool loaded;

        private static Hero me;

        private static ParticleEffect rangeDisplay;

        private static Hero target;

        #endregion

        #region Public Methods and Operators

        public static void Init()
        {
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.Load();
            if (rangeDisplay == null)
            {
                return;
            }
            rangeDisplay = null;
        }

        #endregion

        #region Methods

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!loaded)
            {
                //Orbwalking.Load();
                me = ObjectMgr.LocalHero;
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                loaded = true;
            }

            if (me == null || !me.IsValid)
            {
                //Orbwalking.Load();
                loaded = false;
                me = ObjectMgr.LocalHero;
                if (rangeDisplay == null)
                {
                    return;
                }
                rangeDisplay = null;
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            if (rangeDisplay == null)
            {
                rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                lastRange = me.GetAttackRange() + me.HullRadius + 25;
                rangeDisplay.SetControlPoint(1, new Vector3(lastRange, 0, 0));
            }
            else
            {
                if (lastRange != (me.GetAttackRange() + me.HullRadius + 25))
                {
                    lastRange = me.GetAttackRange() + me.HullRadius + 25;
                    rangeDisplay.Dispose();
                    rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                    rangeDisplay.SetControlPoint(1, new Vector3(lastRange, 0, 0));
                }
            }
            if (target != null && (!target.IsValid || !target.IsVisible || !target.IsAlive || target.Health <= 0))
            {
                target = null;
            }
            var canCancel = Orbwalking.CanCancelAnimation();
            if (canCancel)
            {
                if (target != null && !target.IsVisible && !Orbwalking.AttackOnCooldown(target))
                {
                    target = me.ClosestToMouseTarget(128);
                }
                else if (target == null || !Orbwalking.AttackOnCooldown(target))
                {
                    var bestAa = me.BestAATarget();
                    if (bestAa != null)
                    {
                        target = me.BestAATarget();
                    }
                }
                if (Game.IsKeyDown(FarmKey)
                    && (creepTarget == null || !creepTarget.IsValid || !creepTarget.IsVisible || !creepTarget.IsAlive || creepTarget.Health <= 0
                         || !Orbwalking.AttackOnCooldown(creepTarget)))
                {
                    creepTarget = TargetSelector.GetLowestHPCreep(me);
                }
            }

            if (Game.IsChatOpen)
            {
                return;
            }

            if (Game.IsKeyDown(FarmKey))
            {
                Orbwalking.Orbwalk(creepTarget);
            }
            if (Game.IsKeyDown(ChaseKey))
            {
                Orbwalking.Orbwalk(target, attackmodifiers: true);
            }
            if (Game.IsKeyDown(KiteKey))
            {
                Orbwalking.Orbwalk(
                    target,
                    attackmodifiers: true,
                    bonusRange: (float)(UnitDatabase.GetAttackRate(me) * 1000));
            }
        }

        #endregion
    }
}
