using System;
using System.Reflection;
using Orbblade;

class Program
{
    static void Invoke(Type type, string method, params object[] args) =>
        type.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);

    static void Check(bool value) { if (!value) throw new Exception("Combat check failed"); }

    static FireAttack Begin(ActMelee action)
    {
        object[] args = { action, null };
        Invoke(typeof(FireAttackScope), "Prefix", args);
        Act.CurrentAct = action;
        return (FireAttack)args[1];
    }

    static void Hit(AttackProcess attack, bool success)
    {
        object[] args = { attack, default(FireHit) };
        Invoke(typeof(CaptureFireHit), "Prefix", args);
        Invoke(typeof(CaptureFireHit), "Postfix", success, args[1]);
    }

    static void End(FireAttack scope, Exception error = null) =>
        Invoke(typeof(FireAttackScope), "Finalizer", error, scope);

    static void Main()
    {
        Act.CC = new Chara();
        var target = new Card();
        var attack = new AttackProcess { CC = Act.CC, TC = target, weapon = new Thing() };
        var scope = Begin(new ActMelee());
        Hit(attack, false);
        Hit(attack, true);
        attack.weapon = new Thing { Fire = false };
        Hit(attack, true);
        Check(target.Damage == 0);
        End(scope);
        Check(target.Damage == 3 && FireAttack.Current == null);
        Check(Mastery.Get(Act.CC, Mastery.Fire) == 1);

        attack.weapon.Fire = true;
        scope = Begin(new ActMelee());
        Hit(attack, true);
        End(scope, new Exception());
        Check(target.Damage == 3 && FireAttack.Current == null);
        Check(Mastery.Get(Act.CC, Mastery.Fire) == 1);

        scope = Begin(new ActMelee());
        Hit(attack, true);
        target.IsAliveInCurrentZone = false;
        End(scope);
        Check(target.Damage == 3);
        Check(Mastery.Get(Act.CC, Mastery.Fire) == 2);

        target.IsAliveInCurrentZone = true;
        scope = Begin(new ActMelee());
        Act.CurrentAct = new ActMelee();
        Hit(attack, true);
        End(scope);
        Check(target.Damage == 3);
        scope = Begin(new ActMelee());
        attack.isThrow = true;
        Hit(attack, true);
        attack.isThrow = false;
        attack.IsRanged = true;
        Hit(attack, true);
        attack.IsRanged = false;
        End(scope);
        Check(target.Damage == 3);
        Console.WriteLine("PASS: deferred damage, miss, off-hand, exception cleanup, dead target, nested action exclusion");
        Mastery.Add(Act.CC, Mastery.Fire, 27);
        Mastery.RecordFireAttack(Act.CC);
        Mastery.RecordFireAttack(Act.CC);
        Check(Msg.Notifications == 1);
        Check(Mastery.Get(new Chara(), Mastery.Fire) == 0);
        Mastery.Add(Act.CC, Mastery.Fire, int.MaxValue);
        Check(Mastery.Get(Act.CC, Mastery.Fire) == int.MaxValue);
        Console.WriteLine("PASS: mastery threshold, one-time notification, character isolation, overflow protection");
    }
}

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)]
    class HarmonyPatch : Attribute { public HarmonyPatch(Type t, string n) {} public HarmonyPatch(Type t, string n, Type[] a) {} }
}
class Act { public static Chara CC; public static Act CurrentAct; }
class ActMelee : Act {}
public class Card
{
    private readonly System.Collections.Generic.Dictionary<string, int> values = new System.Collections.Generic.Dictionary<string, int>();
    public int GetInt(string key) => values.TryGetValue(key, out int value) ? value : 0;
    public void SetInt(string key, int value) => values[key] = value;
    public bool IsAliveInCurrentZone = true;
    public bool isChara = true;
    public int Damage;
    public void DamageHP(long damage, int element, int power, AttackSource source, Card origin, bool effect, Thing weapon)
    { if (element != 910) throw new Exception(); Damage += (int)damage; }
}
public class Chara : Card { public bool IsPC = true; }
public class Thing : Card { public bool Fire = true; }
class AttackProcess { public Chara CC; public Card TC; public Thing weapon; public bool isThrow; public bool IsRanged; }
public enum AttackSource { None }
class SKILL { public const int eleFire = 910; }
namespace Orbblade
{
    class Orbs { public static bool IsSword(Thing t) => t != null; public static string Attached(Thing t) => t.Fire ? "fire" : ""; }
}
class Msg { public static int Notifications; public static void SayRaw(string text) { Notifications++; } }
