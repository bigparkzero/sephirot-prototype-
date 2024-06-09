using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    PlayerMove _player;
    private void Awake()
    {
        _player = GetComponent<PlayerMove>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
