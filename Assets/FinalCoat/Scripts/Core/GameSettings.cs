using UnityEngine;

namespace FinalCoat{
    public static class GameSettings{
        public const int PlayerCount = 2;
        public static bool DropInNextScene;
        const string ColourKey = "FinalCoat.Colour.P";
        public static readonly string[] ColourNames = { "Red", "Blue", "Yellow", "Green", "Purple", "Orange", "Pink", "Teal" };
        public static readonly Color[] Colours = {
            new Color(0.93f, 0.3f, 0.3f),
            new Color(0.25f, 0.5f, 0.95f),
            new Color(0.98f, 0.78f, 0.15f),
            new Color(0.3f, 0.75f, 0.35f),
            new Color(0.62f, 0.35f, 0.85f),
            new Color(0.98f, 0.55f, 0.2f),
            new Color(0.95f, 0.5f, 0.75f),
            new Color(0.2f, 0.72f, 0.72f)
        };
        public static int GetColourIndex(int player){
            int value = PlayerPrefs.GetInt(ColourKey + player, player);
            return Mathf.Clamp(value, 0, Colours.Length - 1);
        }

        public static Color GetColour(int player) => Colours[GetColourIndex(player)];
        public static string GetColourName(int player) => ColourNames[GetColourIndex(player)];
        public static void CycleColour(int player, int step){
            int next = GetColourIndex(player);
            for (int i = 0; i < Colours.Length; i++){
                next = (next + step + Colours.Length) % Colours.Length;
                if (!TakenByOther(player, next)) break;
            }
            PlayerPrefs.SetInt(ColourKey + player, next);
            PlayerPrefs.Save();
        }

        static bool TakenByOther(int player, int index){
            for (int other = 0; other < PlayerCount; other++)
                if (other != player && GetColourIndex(other) == index) return true;
            return false;
        }
    }
}
