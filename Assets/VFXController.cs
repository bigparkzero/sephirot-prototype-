/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    public VisualEffect[] visualEffect;
    private void Start()
    {
     

    }
    public void PlayVFX(int a)
    {
        visualEffect[a].Play();
      
    }

    public void StopVFX(int a)
    {
        visualEffect[a].Stop();
    }
   
}
*/
using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    public VisualEffect visualEffect;

    void Start()
    {
        if (visualEffect == null)
        {
            visualEffect = GetComponent<VisualEffect>();
        }
    }

    // 애니메이션 이벤트를 통해 호출되는 함수
    public void PlayVFX(string jsonParams)
    {
        if (!string.IsNullOrEmpty(jsonParams))
        {
            VFXParams parameters = JsonUtility.FromJson<VFXParams>(jsonParams);
            Vector3 position = new Vector3(parameters.positionX, parameters.positionY, parameters.positionZ);
            Vector3 rotation = new Vector3(parameters.rotationX, parameters.rotationY, parameters.rotationZ);

            // 이펙트의 위치와 회전을 설정
            visualEffect.transform.position = position + transform.position;
            visualEffect.transform.eulerAngles = rotation + transform.eulerAngles;
            visualEffect.Play();
        }
    }

    // 데이터 직렬화 구조체
    [System.Serializable]
    public struct VFXParams
    {
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationX;
        public float rotationY;
        public float rotationZ;
    }

    // 이펙트를 중지하는 함수
    public void StopVFX()
    {
        if (visualEffect != null)
        {
            visualEffect.Stop();
        }
    }
}
