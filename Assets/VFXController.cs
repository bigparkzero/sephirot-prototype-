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
