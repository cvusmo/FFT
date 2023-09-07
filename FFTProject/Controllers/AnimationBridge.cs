//|=====================Summary========================|0|
//|                 Animation Bridge                   |1|
//|by cvusmo===========================================|4|
//|====================================================|2|
using FFT.Modules;
using FFT.Utilities;
using UnityEngine;

namespace FFT.Controllers
{
    public class AnimationBridge : MonoBehaviour
    {
        public Material targetMaterial;
        public Texture3D[] vaporTextures; 
        public float animationValue;

        public float intensity = 0.3f;
        public float stepDistance = 0.01f;

        private int currentTextureIndex = 0;
        private float timeSinceLastChange = 0.0f;
        private void Awake()
        {
            SetTexture();
        }
        private void Update()
        {
            if (targetMaterial != null)
            {
                timeSinceLastChange += Time.deltaTime;

                if (timeSinceLastChange >= animationValue)
                {
                    SetNextTexture();
                    timeSinceLastChange = 0f;
                }
            }
        }
        private void SetTexture()
        {
            if (vaporTextures != null && vaporTextures.Length > 0)
            {
                targetMaterial.SetTexture("_Volume", vaporTextures[currentTextureIndex]);
            }
        }
        private void SetNextTexture()
        {
            currentTextureIndex = (currentTextureIndex + 1) % vaporTextures.Length;
            SetTexture();
        }
        public void UpdateShaderProperties(Material cachedMaterial, float customIntensity, float customStepDistance, float customASL, float customAGL, float customVV, float customHV, float customDP, float customSP, float customAT, float customET, float customFL)
        {
            if (targetMaterial)
            {
                targetMaterial.SetFloat("_Intensity", customIntensity);
                targetMaterial.SetFloat("_StepDistance", customStepDistance);
                targetMaterial.SetFloat("_ASL", customASL);
                targetMaterial.SetFloat("_AGL", customAGL);
                targetMaterial.SetFloat("_VV", customVV);
                targetMaterial.SetFloat("_HV", customHV);
                targetMaterial.SetFloat("_DP", customDP);
                targetMaterial.SetFloat("_SP", customSP);
                targetMaterial.SetFloat("_AT", customAT);
                targetMaterial.SetFloat("_ET", customET);
                targetMaterial.SetFloat("_FL", customFL);
            }
            if (cachedMaterial)
            {
                cachedMaterial.SetFloat("_Intensity", customIntensity);
                cachedMaterial.SetFloat("_StepDistance", customStepDistance);
                cachedMaterial.SetFloat("_ASL", customASL);
                cachedMaterial.SetFloat("_AGL", customAGL);
                cachedMaterial.SetFloat("_VV", customVV);
                cachedMaterial.SetFloat("_HV", customHV);
                cachedMaterial.SetFloat("_DP", customDP);
                cachedMaterial.SetFloat("_SP", customSP);
                cachedMaterial.SetFloat("_AT", customAT);
                cachedMaterial.SetFloat("_ET", customET);
                cachedMaterial.SetFloat("_FL", customFL);
            }
        }
    }
}