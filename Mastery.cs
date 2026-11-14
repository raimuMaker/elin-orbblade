using System;

namespace Orbblade
{
    // Card's string-keyed data is serialized with its owning character.
    public static class Mastery
    {
        public const string Fire = "fire";
        public const int FireUnlock = 30;
        private const string Prefix = "raimumaker.orbblade.mastery.";

        public static void RecordFireAttack(Chara character)
        {
            int before = Get(character, Fire);
            int after = Add(character, Fire, 1);
            if (before < FireUnlock && after >= FireUnlock)
                Msg.SayRaw("���̌^�E���ݍ��ݎa����K�������I�G�̑��상�j���[����g�p�ł��܂��B");
        }

        public static int Get(Chara character, string element)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            ValidateElement(element);
            return Math.Max(0, character.GetInt(Prefix + element));
        }

        public static int Add(Chara character, string element, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            int current = Get(character, element);
            int next = (int)Math.Min(int.MaxValue, (long)current + amount);
            character.SetInt(Prefix + element, next);
            return next;
        }

        private static void ValidateElement(string element)
        {
            if (string.IsNullOrWhiteSpace(element))
                throw new ArgumentException("An element ID is required.", nameof(element));
        }
    }
}
