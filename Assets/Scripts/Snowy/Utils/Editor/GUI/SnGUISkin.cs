using UnityEngine;

namespace Snowy.Utils
{
    //[CreateAssetMenu(fileName = "SnowyGUI", menuName = "Snowy/GUI Skin", order = 0)]
    [ExecuteAlways]
    public class SnGUISkin : ScriptableObject
    {
        private GUIStyle m_boxStyle;
        private GUIStyle m_titleStyle;
        private Texture2D darkTex;
        private GUIStyle m_sectionTitleStyle;

        public Texture2D DarkTex
        {
            get
            {
                if (darkTex == null)
                {
                    darkTex = new Texture2D(1, 1);
                    darkTex.SetPixel(0, 0, new Color(0.17f, 0.17f, 0.17f));
                    darkTex.Apply();
                }

                return darkTex;
            }
        }
        
        
        public GUIStyle boxStyle
        {
            get
            {
                if (m_boxStyle == null)
                {
                    m_boxStyle = new GUIStyle
                    {
                        border = new RectOffset(4, 4, 4, 4),
                        padding = new RectOffset(4, 4, 4, 4),
                        margin = new RectOffset(4, 4, 4, 4),
                        normal =
                        {
                            textColor = Color.white,
                            background = !UnityEditor.EditorGUIUtility.isProSkin ? UnityEditor.EditorGUIUtility.whiteTexture : DarkTex
                        }
                    };
                }
                
                return m_boxStyle;
            }
        }
        
        public GUIStyle titleStyle
        {
            get
            {
                if (m_titleStyle == null)
                {
                    m_titleStyle = new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 16,
                        fontStyle = FontStyle.Bold,
                        normal = {textColor = Color.white},
                        border = new RectOffset(4, 4, 4, 4),
                    };
                }
                
                return m_titleStyle;
            }
        }
        
        public GUIStyle sectionTitleStyle
        {
            get
            {
                if (m_sectionTitleStyle == null)
                {
                    m_sectionTitleStyle = new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        normal = {textColor = Color.white},
                        border = new RectOffset(4, 4, 4, 4),
                    };
                }
                
                return m_sectionTitleStyle;
            }
        }
        
        
        private void OnEnable()
        {
            // Generate the dark texture
            DarkTex.hideFlags = HideFlags.HideAndDontSave;
            
            // Create a box style with border radius with white outline
            m_boxStyle = new GUIStyle
            {
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset(4, 4, 4, 4),
                normal =
                {
                    textColor = Color.white,
                    background = !UnityEditor.EditorGUIUtility.isProSkin ? UnityEditor.EditorGUIUtility.whiteTexture : DarkTex
                }
            };
            
            // Create a title style
            m_titleStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = {textColor = Color.white},
                border = new RectOffset(4, 4, 4, 4),
            };
            
            // Create a section title style
            m_sectionTitleStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = {textColor = Color.white},
                border = new RectOffset(4, 4, 4, 4),
            };
        }
        
        public void Reset()
        {
            m_boxStyle = null;
            m_titleStyle = null;
            m_sectionTitleStyle = null;
        }
    }
}