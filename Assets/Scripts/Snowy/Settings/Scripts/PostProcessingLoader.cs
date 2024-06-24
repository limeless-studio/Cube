using GI.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Snowy.Settings
{
    [RequireComponent(typeof(Volume))]
    public class PostProcessingLoader : MonoBehaviour
    {
        private Volume m_volume;
        Bloom m_bloom;
        ChromaticAberration m_chromaticAberration;
        Vignette m_vignette;
        MotionBlur m_motionBlur;
        DepthOfField m_depthOfField;
        FilmGrain m_filmGrain;
        GlobalIllumination m_globalIllumination;

        private void Awake()
        {
            Settings.OnProfileChanged += OnProfileChanged;
            m_volume = GetComponent<Volume>();

            LoadComponents();
        }

        private void Start()
        {
            OnProfileChanged(Settings.Instance.currentProfile);
        }

        private void LoadComponents()
        {
            m_volume.profile.TryGet(out m_bloom);
            m_volume.profile.TryGet(out m_chromaticAberration);
            m_volume.profile.TryGet(out m_vignette);
            m_volume.profile.TryGet(out m_motionBlur);
            m_volume.profile.TryGet(out m_depthOfField);
            m_volume.profile.TryGet(out m_filmGrain);
            m_volume.profile.TryGet(out m_globalIllumination);
        }
        
        private void OnProfileChanged(GraphicProfile profile)
        {
            if (m_bloom)
                m_bloom.active = profile.bloom;
            if (m_chromaticAberration)
                m_chromaticAberration.active = profile.chromaticAberration;
            if (m_filmGrain)
                m_filmGrain.active = profile.filmNoise;
            if (m_motionBlur)
                m_motionBlur.active = profile.motionBlur;
            if (m_depthOfField)
                m_depthOfField.active = profile.depthOfField;
            if (m_globalIllumination)
                m_globalIllumination.active = profile.globalIllumination;
        }
    }
}