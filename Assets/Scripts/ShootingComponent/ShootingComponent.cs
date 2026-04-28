using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;

public class ShootingComponent : MonoBehaviour
{
    [Header("----Components----")]
    [SerializeField] LayerMask ignoreLayer;
  
    [Header("----Guns----")]
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform gunPivot;

    int reserveAmmo;

    [Header("----CameraData----")]
    [SerializeField] Transform cameraPosition;

    [Header("----Audio----")]
    [SerializeField] AudioSource audioPlayer;
    [SerializeField] List<AudioClip> shootClips = new List<AudioClip>();
    [SerializeField] float audioShotVol;

    float shootTimer;

    float reloadTimer;

    bool isAutomatic;

    bool isPlayingShooting;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
          
    }

    // Update is called once per frame
    void Update()
    {
        gunPivot.transform.localRotation = cameraPosition.localRotation;
    }



    public void Shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, gunPivot.transform.rotation);
    }


    
  }