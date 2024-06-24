using System;
using UnityEngine;

namespace Game
{
    // Create multiple endcases
    [Flags]
    public enum MinigameEndcase
    {
        None = 0,
        Time = 1 << 0,
        LastManStanding = 1 << 1,
        All = ~0
    }
    
    [CreateAssetMenu(fileName = "MinigameData", menuName = "Minigame Data")]
    public class MinigameData : ScriptableObject
    {
        [Title("Minigame Data")]
        [Disable] public int id = -1;
        [ShowWarningIf(nameof(id), -1, "Open the data manager and assign an id to this minigame data")]
        public string title;
        public string description;
        public Sprite icon;
        
        [Title("Game Settings")]
        [EnumToggles] public MinigameEndcase endCases;
        [ShowIf(nameof(endCases), MinigameEndcase.Time, Comparison = UnityComparisonMethod.Mask)] public int time = 60;
        public int minPlayers = 2;
    }
}