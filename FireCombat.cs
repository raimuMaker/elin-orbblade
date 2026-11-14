using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Orbblade
{
    // A scope delays orb effects until vanilla melee, including talismans, finishes.
    internal sealed class FireAttack
    {
        [ThreadStatic] internal static FireAttack Current;
        [ThreadStatic] private static bool applying;
        internal readonly FireAttack Parent;
        internal readonly ActMelee Action;
        internal readonly Chara Attacker;
        private List<FireHit> hits;

        internal FireAttack(ActMelee action)
        {
            Parent = Current;
            Action = action;
            Attacker = Act.CC;
        }

        internal static bool CanCapture => !applying && Current != null;

        internal void Add(FireHit hit)
        {
            if (hits == null) hits = new List<FireHit>(2);
            hits.Add(hit);
        }

        internal void Apply()
        {
            if (hits == null || applying) return;
            applying = true;
            try
            {
                if (Attacker.IsAliveInCurrentZone) Mastery.RecordFireAttack(Attacker);
                foreach (FireHit hit in hits)
                {
                    if (!Attacker.IsAliveInCurrentZone || !hit.Target.IsAliveInCurrentZone) continue;
                    hit.Target.DamageHP(3, SKILL.eleFire, 100, AttackSource.None,
                        Attacker, true, hit.Weapon);
                }
            }
            finally { applying = false; }
        }
    }

    internal struct FireHit
    {
        internal FireAttack Scope;
        internal Card Target;
        internal Thing Weapon;
    }

    [HarmonyPatch(typeof(ActMelee), "Perform", new Type[0])]
    internal static class FireAttackScope
    {
        private static void Prefix(ActMelee __instance, out FireAttack __state)
        {
            __state = null;
            // Special attacks and counterattacks have their own action types.
            if (__instance.GetType() != typeof(ActMelee) || Act.CC == null || !Act.CC.IsPC) return;
            __state = new FireAttack(__instance);
            FireAttack.Current = __state;
        }

        private static void Finalizer(Exception __exception, FireAttack __state)
        {
            if (__state == null) return;
            FireAttack.Current = __state.Parent;
            if (__exception == null) __state.Apply();
        }
    }

    [HarmonyPatch(typeof(AttackProcess), "Perform")]
    internal static class CaptureFireHit
    {
        private static void Prefix(AttackProcess __instance, out FireHit __state)
        {
            __state = default;
            if (!FireAttack.CanCapture) return;
            FireAttack scope = FireAttack.Current;
            if (Act.CurrentAct != scope.Action || __instance.CC != scope.Attacker ||
                __instance.isThrow || __instance.IsRanged || __instance.TC == null ||
                !__instance.TC.isChara || !Orbs.IsSword(__instance.weapon) ||
                Orbs.Attached(__instance.weapon) != Mastery.Fire) return;
            __state = new FireHit { Scope = scope, Target = __instance.TC, Weapon = __instance.weapon };
        }

        private static void Postfix(bool __result, FireHit __state)
        {
            if (__result && __state.Scope != null) __state.Scope.Add(__state);
        }
    }
}
