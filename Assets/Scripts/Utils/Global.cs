using Game;
using Snowy.Utils;
using UnityEngine;
using System.Linq;

namespace Utils
{
    public class Global : MonoSingleton<Global>
    {
        [SerializeField] private Minigame[] minigameData;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        public Minigame GetMinigameData(int id)
        {
            return minigameData.FirstOrDefault(minigame => minigame.Data.id == id);
        }
        
        public Minigame[] GetMinigameData()
        {
            return minigameData;
        }
        
        public Minigame GetRandomMinigameData()
        {
            return minigameData[Random.Range(0, minigameData.Length)];
        }
    }
}