using System;
using System.Reflection;
using HarmonyLib;

namespace Orbblade
{
    public static class Orbs
    {
        public const string FireId = "raimumaker_orbblade_fire";
        public const string SocketKey = "raimumaker.orbblade.socket";
        public const int StoneCost = 5;

        public static bool Owned(Thing item) => item != null && EClass.pc != null && item.GetRootCard() == EClass.pc;
        public static bool IsSword(Thing item) => item != null && item.IsMeleeWeapon &&
            (item.category.id == "sword" || item.category.id == "longsword");
        public static string Attached(Thing sword) => sword.GetStr(SocketKey, "");

        public static void Craft(Thing stones)
        {
            if (!Owned(stones) || stones.id != "stone" || stones.Num < StoneCost) return;
            Thing orb = ThingGen.Create(FireId);
            if (EClass.pc.things.IsFull(orb, false, false)) return;
            EClass.pc.AddCard(orb);
            stones.ModNum(-StoneCost);
            LayerInventory.SetDirtyAll();
        }

        public static void Attach(Thing sword, Thing orb)
        {
            if (!Owned(sword) || !Owned(orb) || !IsSword(sword) || sword.Num != 1 ||
                Attached(sword) != "" || orb.id != FireId || orb.Num < 1) return;
            sword.SetStr(SocketKey, Mastery.Fire);
            orb.ModNum(-1);
            LayerInventory.SetDirtyAll();
        }

        public static void Detach(Thing sword)
        {
            if (!Owned(sword) || Attached(sword) != Mastery.Fire) return;
            Thing orb = ThingGen.Create(FireId);
            if (EClass.pc.things.IsFull(orb, false, false)) return;
            EClass.pc.AddCard(orb);
            sword.SetStr(SocketKey, "");
            LayerInventory.SetDirtyAll();
        }
    }

    [HarmonyPatch(typeof(SourceCard), "Init")]
    internal static class RegisterOrb
    {
        private static void Prefix()
        {
            var source = EClass.sources.things;
            if (source.map.ContainsKey(Orbs.FireId)) return;
            // Clone once during source initialization. Runtime operations use normal game data.
            var clone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            var row = (SourceThing.Row)clone.Invoke(source.map["stone"], null);
            row.id = Orbs.FireId;
            row.name = "Fire Orb";
            row.name_JP = "���̃I�[�u";
            row.name_L = "���̃I�[�u";
            row.detail = row.detail_JP = row.detail_L = "���̏����i���j���[���瑕���ł��܂��B";
            row.trait = new string[0];
            row.tag = new string[0];
            row.factory = new string[0];
            row.recipeKey = new string[0];
            row.chance = 0;
            row.value = 100;
            row._model = null;
            source.rows.Add(row);
            source.map.Add(row.id, row);
        }
    }

    [HarmonyPatch(typeof(InvOwner), "ListInteractions", new Type[] {
        typeof(InvOwner.ListInteraction), typeof(Thing), typeof(Trait), typeof(ButtonGrid), typeof(bool) })]
    internal static class OrbMenu
    {
        private static void Postfix(InvOwner.ListInteraction list, Thing t)
        {
            if (!Orbs.Owned(t)) return;
            if (t.id == "stone" && t.Num >= Orbs.StoneCost)
                list.Add("���̃I�[�u���쐬�i��5�j", 80, () => Orbs.Craft(t));
            if (!Orbs.IsSword(t) || t.Num != 1) return;
            list.Add("���̏K�n�x���m�F", 79, () => Msg.SayRaw(
                "���̏K�n�x " + Mastery.Get(EClass.pc, Mastery.Fire) +
                " / �Z��� " + Mastery.FireUnlock));
            if (Orbs.Attached(t) == Mastery.Fire)
            {
                list.Add("���̃I�[�u�����O��", 80, () => Orbs.Detach(t));
                return;
            }
            if (Orbs.Attached(t) != "") return;
            Thing orb = EClass.pc.things.Find(item => item.id == Orbs.FireId && item.Num > 0, true);
            if (orb != null) list.Add("���̃I�[�u�𑕒�", 80, () => Orbs.Attach(t, orb));
        }
    }

    [HarmonyPatch(typeof(Thing), "CanStackTo")]
    internal static class PreserveSocket
    {
        private static void Postfix(Thing __instance, Thing __0, ref bool __result)
        {
            if (__result && (Orbs.Attached(__instance) != "" || Orbs.Attached(__0) != ""))
                __result = false;
        }
    }
}
