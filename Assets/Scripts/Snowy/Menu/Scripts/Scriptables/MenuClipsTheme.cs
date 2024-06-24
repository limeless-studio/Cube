using UnityEngine;

namespace Snowy.Menu
{
    [CreateAssetMenu(fileName = "MenuClipsTheme", menuName = "Snowy/Menu/Menu Clips Theme")]
    public class MenuClipsTheme : ScriptableObject
    {
        public AudioClip buttonClick;
        public AudioClip buttonDisabledClick;
        public AudioClip buttonHover;
        public AudioClip menuOpen;
        public AudioClip menuClose;
    }
}