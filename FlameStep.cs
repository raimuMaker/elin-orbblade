using System;
using HarmonyLib;

namespace Orbblade
{
    internal static class FlameStep
    {
        internal const int StaminaCost = 8;

        private static Thing Weapon(Chara actor)
        {
            foreach (BodySlot slot in actor.body.slots)
                if (slot.thing != null && slot.thing.isEquipped && Orbs.IsSword(slot.thing) &&
                    Orbs.Attached(slot.thing) == Mastery.Fire) return slot.thing;
            return null;
        }

        internal static bool CanUse(Chara actor, Card target)
        {
            if (actor == null || !actor.IsPC || target == null || !target.isChara ||
                !target.IsAliveInCurrentZone || !actor.IsAliveInCurrentZone ||
                !actor.IsHostile(target.Chara) || actor.stamina.value < StaminaCost ||
                Mastery.Get(actor, Mastery.Fire) < Mastery.FireUnlock || Weapon(actor) == null ||
                !actor.CanSeeLos(target)) return false;
            int distance = actor.pos.Distance(target.pos);
            if (distance == 1) return actor.CanInteractTo(target);
            if (distance != 2) return false;
            Point step = Step(actor, target);
            return step.IsInBounds && !step.HasChara && !step.IsBlocked && actor.CanMoveTo(step);
        }

        private static Point Step(Chara actor, Card target) => new Point(
            actor.pos.x + Math.Sign(target.pos.x - actor.pos.x),
            actor.pos.z + Math.Sign(target.pos.z - actor.pos.z));

        internal static bool Perform(Chara actor, Card target)
        {
            if (!CanUse(actor, target)) return false;
            Thing weapon = Weapon(actor);
            if (actor.pos.Distance(target.pos) == 2 && actor.TryMove(Step(actor, target)) != Card.MoveResult.Success)
                return false;
            if (!actor.IsAliveInCurrentZone || !target.IsAliveInCurrentZone || !actor.CanInteractTo(target))
                return true; // Movement can trigger traps and other game events.
            actor.stamina.Mod(-StaminaCost);
            Msg.SayRaw("���̌^�E���ݍ��ݎa��I");
            var attack = new AttackProcess();
            attack.Prepare(actor, weapon, target, target.pos);
            bool hit = attack.Perform(0, false, 1.5f);
            if (hit && actor.IsAliveInCurrentZone && target.IsAliveInCurrentZone)
                target.DamageHP(6, SKILL.eleFire, 100, AttackSource.None, actor, true, weapon);
            return true;
        }
    }

    [HarmonyPatch(typeof(ActPlan), "_Update")]
    internal static class FlameStepMenu
    {
        private static void Postfix(ActPlan __instance, PointTarget __0)
        {
            Card target = __0.card;
            Chara actor = __instance.cc;
            if (!FlameStep.CanUse(actor, target)) return;
            __instance.TrySetAct("���̌^�E���ݍ��ݎa��iSP8�j", () => FlameStep.Perform(actor, target),
                target, null, 2, true, true, false);
        }
    }
}
