
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    public VisualEffect visualEffect;
    public List<ParticleSystem> Particle;

    void Start()
    {
        if (visualEffect == null)
        {
            visualEffect = GetComponent<VisualEffect>();
        }
    }
    public void PlayVFX(string jsonParams)
    {
        if (!string.IsNullOrEmpty(jsonParams))
        {
            VFXParams parameters = JsonUtility.FromJson<VFXParams>(jsonParams);
            Vector3 position = new Vector3(parameters.positionX, parameters.positionY, parameters.positionZ);
            Vector3 rotation = new Vector3(parameters.rotationX, parameters.rotationY, parameters.rotationZ);

            //visualEffect.transform.localScale = new Vector3(parameters.scale, parameters.scale,parameters.scale);
            visualEffect.transform.position = position + transform.position;
            visualEffect.transform.eulerAngles = rotation + transform.eulerAngles;
            visualEffect.Play();
        }
    }

    public void PlayParticle(int particleID, string jsonParams)
    {
        VFXParams parameters = JsonUtility.FromJson<VFXParams>(jsonParams);
        Vector3 position = new Vector3(parameters.positionX, parameters.positionY, parameters.positionZ);
        Vector3 rotation = new Vector3(parameters.rotationX, parameters.rotationY, parameters.rotationZ);

        Particle[particleID].transform.localScale = new Vector3(parameters.scale, parameters.scale, parameters.scale);
        Particle[particleID].transform.position = position + transform.position;
        Particle[particleID].transform.eulerAngles = rotation + transform.eulerAngles;
        Particle[particleID].Play();
    }

    [System.Serializable]
    public struct VFXParams
    {
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationX;
        public float rotationY;
        public float rotationZ;
        public float scale;
    }

    public void StopVFX()
    {
        if (visualEffect != null)
        {
            visualEffect.Stop();
        }
    }
}
