using UnityEngine;

namespace Snowy.Utils
{
    public class SnGUI
    {
        private static SnGUISkin m_skin;
        public static SnGUISkin skin
        {
            get
            {
                // Load from the resources folder
                if (m_skin == null)
                {
                    m_skin = Resources.Load<SnGUISkin>("SnowyGUI");
                }
                
                return m_skin;
            }
        }
    }
}