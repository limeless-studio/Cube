using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Snowy.Settings
{
    [RequireComponent(typeof(Camera))]
    public class CameraLoader : MonoBehaviour
    {
        private Camera m_camera;
        private UniversalAdditionalCameraData m_cameraData;

        private void Awake()
        {
            m_camera = GetComponent<Camera>();
            m_cameraData = m_camera.GetUniversalAdditionalCameraData();
            Settings.OnProfileChanged += OnProfileChanged;
        }

        private void Start()
        {
            OnProfileChanged(Settings.Instance.currentProfile);
        }
        
        private void OnProfileChanged(GraphicProfile profile)
        {
            if (profile == null) return;
            if(!m_camera) m_camera = GetComponent<Camera>();
            if(!m_cameraData) m_cameraData = m_camera.GetUniversalAdditionalCameraData();
            
            m_cameraData.antialiasing = (AntialiasingMode)profile.antiAliasing;
        }
    }
}