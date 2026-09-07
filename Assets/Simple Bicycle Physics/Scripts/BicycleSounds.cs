using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; 

namespace SBPScripts
{
    public class BicycleSounds : MonoBehaviour
    {
        public AudioSource pedallingAudioSource, freeWheelAudioSource, bodyHitAudioSource, tyreHitFAudioSource, tyreHitRAudioSource;
        float fHitCoolDown, rHitCoolDown, bHitCoolDown, hopCoolDown;
        
        BicycleController bicycleController;

        void Start()
        {
            bicycleController = GetComponentInParent<BicycleController>(); 

            if (bicycleController == null)
            {
                Debug.LogError("BicycleController tidak ditemukan pada GameObject: " + gameObject.name + ". Skrip BicycleSounds tidak akan berfungsi dengan benar.");
                enabled = false;
                return;
            }

            
            EnsureAudioSource(ref pedallingAudioSource, "PedallingAudioSource");
            EnsureAudioSource(ref freeWheelAudioSource, "FreeWheelAudioSource");
            EnsureAudioSource(ref bodyHitAudioSource, "BodyHitAudioSource");
            EnsureAudioSource(ref tyreHitFAudioSource, "TyreHitFAudioSource");
            EnsureAudioSource(ref tyreHitRAudioSource, "TyreHitRAudioSource");
        }

        
        void EnsureAudioSource(ref AudioSource source, string sourceName)
        {
            if (source == null)
            {
                
                Transform childSource = transform.Find(sourceName); 
                if (childSource != null) source = childSource.GetComponent<AudioSource>();

                if (source == null)
                {
                    Debug.LogWarning($"AudioSource '{sourceName}' belum di-assign pada {gameObject.name} dan tidak ditemukan sebagai child. Assign manual di Inspector.");
                }
            }
            
            if (source != null)
            {
                source.spatialBlend = 1.0f;
            }
        }

        void Update()
        {
            if (bicycleController == null) return; 

    
            if (bicycleController.rb.linearVelocity.magnitude > 2f)
            {
                if (bicycleController.rawCustomAccelerationAxis > 0 && !bicycleController.isAirborne)
                {
                    pedallingAudioSource.pitch = Mathf.Clamp(1 + bicycleController.rb.linearVelocity.magnitude * 0.05f, 1, 3);
                    
                    if (pedallingAudioSource) pedallingAudioSource.volume = 0.5f + bicycleController.customAccelerationAxis;
                }
                else
                {
                    if (pedallingAudioSource) pedallingAudioSource.volume -= Time.deltaTime;
                    if (pedallingAudioSource) pedallingAudioSource.volume = Mathf.Clamp(pedallingAudioSource.volume, 0.2f, 1);
                }

                if (bicycleController.rawCustomAccelerationAxis < 1 || bicycleController.isAirborne) 
                {
                    if (freeWheelAudioSource) freeWheelAudioSource.pitch = Mathf.Clamp(1 + bicycleController.rb.linearVelocity.magnitude * 0.05f, 1, 3);
                    if (freeWheelAudioSource) freeWheelAudioSource.volume += Time.deltaTime * 2;
                }
                else
                {
                    if (freeWheelAudioSource) freeWheelAudioSource.volume -= Time.deltaTime * 2;
                }
            }
            else 
            {
                if (pedallingAudioSource) pedallingAudioSource.volume = bicycleController.rb.linearVelocity.magnitude * 0.25f;
                if (freeWheelAudioSource) freeWheelAudioSource.volume = bicycleController.rb.linearVelocity.magnitude * 0.25f;
            }

            
            if (freeWheelAudioSource) freeWheelAudioSource.volume = Mathf.Clamp(freeWheelAudioSource.volume, 0f, 1f); 

            if (bicycleController.bunnyHopInputState == 1)
            {
                hopCoolDown = 1;
            }
            hopCoolDown -= Time.deltaTime * 5;
            hopCoolDown = Mathf.Clamp01(hopCoolDown);

            // Tyre Hit Front
            if (tyreHitFAudioSource && bicycleController.fWheelRb != null)
            {
                ConfigurableJoint fJoint = bicycleController.fWheelRb.GetComponent<ConfigurableJoint>();
                if (fJoint != null && fJoint.currentForce.magnitude > 500 && fHitCoolDown == 0 && hopCoolDown == 0)
                {
                    tyreHitFAudioSource.Play();
                    tyreHitFAudioSource.pitch = Mathf.Clamp(fJoint.currentForce.magnitude / 1000, 0.75f, 1.5f);
                    tyreHitFAudioSource.volume = Mathf.Clamp(fJoint.currentForce.magnitude / 5000, 0, 0.2f);
                    fHitCoolDown = 1;
                }
            }
            fHitCoolDown -= Time.deltaTime;
            fHitCoolDown = Mathf.Clamp01(fHitCoolDown);

            if (tyreHitRAudioSource && bicycleController.rWheelRb != null)
            {
                ConfigurableJoint rJoint = bicycleController.rWheelRb.GetComponent<ConfigurableJoint>();
                if (rJoint != null && rJoint.currentForce.magnitude > 500 && rHitCoolDown == 0 && hopCoolDown == 0)
                {
                    tyreHitRAudioSource.Play();
                    tyreHitRAudioSource.pitch = Mathf.Clamp(rJoint.currentForce.magnitude / 1000, 0.75f, 1.5f);
                    tyreHitRAudioSource.volume = Mathf.Clamp(rJoint.currentForce.magnitude / 5000, 0, 0.2f);
                    rHitCoolDown = 1;
                }
            }
            rHitCoolDown -= Time.deltaTime;
            rHitCoolDown = Mathf.Clamp01(rHitCoolDown);

            if (bodyHitAudioSource && bicycleController.fWheelRb != null)
            { // Tambah null check
                ConfigurableJoint fJointForBody = bicycleController.fWheelRb.GetComponent<ConfigurableJoint>(); // Asumsi ini benar
                if (fJointForBody != null && fJointForBody.currentForce.magnitude > 1000 && bHitCoolDown == 0)
                {
                    bodyHitAudioSource.Play();
                    bodyHitAudioSource.pitch = fJointForBody.currentForce.magnitude / 1000;
                    bodyHitAudioSource.volume = fJointForBody.currentForce.magnitude / 1000; // Mungkin perlu di-clamp
                    bHitCoolDown = 1;
                }
            }
            bHitCoolDown -= Time.deltaTime; // Kurangi cooldown per detik
            bHitCoolDown = Mathf.Clamp01(bHitCoolDown);
        }
    }
}
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Audio;

// namespace SBPScripts
// {


//     public class BicycleSounds : MonoBehaviour
//     {
//         public AudioSource pedallingAudioSource, freeWheelAudioSource, bodyHitAudioSource, tyreHitFAudioSource, tyreHitRAudioSource;
//         float fHitCoolDown,rHitCoolDown,bHitCoolDown, hopCoolDown;
//         AudioMixer mixer;
//         BicycleController bicycleController;
//         // Start is called before the first frame update
//         void Start()
//         {
//             pedallingAudioSource = pedallingAudioSource.GetComponent<AudioSource>();
//             freeWheelAudioSource = freeWheelAudioSource.GetComponent<AudioSource>();
//             tyreHitFAudioSource = tyreHitFAudioSource.GetComponent<AudioSource>();
//             bicycleController = FindObjectOfType<BicycleController>();
//         }

        
//         // Update is called once per frame
//         void Update()
//         {
//             if (bicycleController.rb.linearVelocity.magnitude > 2f)
//             {
//                 if (bicycleController.rawCustomAccelerationAxis > 0 && !bicycleController.isAirborne)
//                 {
//                     pedallingAudioSource.pitch = Mathf.Clamp(1 + bicycleController.rb.linearVelocity.magnitude * 0.05f,1,3);
//                     pedallingAudioSource.volume = 0.5f + bicycleController.customAccelerationAxis;
//                 }

//                 else
//                 {
//                     pedallingAudioSource.volume -= Time.deltaTime;
//                     pedallingAudioSource.volume = Mathf.Clamp(pedallingAudioSource.volume, 0.2f, 1);
//                 }
//                 if (bicycleController.rawCustomAccelerationAxis < 1 || bicycleController.isAirborne)
//                 {
//                     freeWheelAudioSource.pitch = Mathf.Clamp(1 + bicycleController.rb.linearVelocity.magnitude * 0.05f, 1,3);
//                     freeWheelAudioSource.volume += Time.deltaTime * 2;
//                 }
//                 else
//                 {
//                     freeWheelAudioSource.volume -= Time.deltaTime * 2;
//                 }
//             }
//             else
//             {
//                 pedallingAudioSource.volume = bicycleController.rb.linearVelocity.magnitude * 0.25f;
//                 freeWheelAudioSource.volume = bicycleController.rb.linearVelocity.magnitude * 0.25f;
//             }

//             if(bicycleController.bunnyHopInputState == 1)
//             {
//                 hopCoolDown = 1;
//             }
//             hopCoolDown -=Time.deltaTime*5;
//             hopCoolDown = Mathf.Clamp01(hopCoolDown);

//             if(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude > 500 && fHitCoolDown == 0 && hopCoolDown == 0)
//             {
//                 tyreHitFAudioSource.Play();
//                 tyreHitFAudioSource.pitch = Mathf.Clamp(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000, 0.75f,1.5f);
//                 tyreHitFAudioSource.volume = Mathf.Clamp(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 5000, 0 ,0.2f);
//                 fHitCoolDown = 1;
//             }
//             fHitCoolDown -=Time.deltaTime;
//             fHitCoolDown = Mathf.Clamp01(fHitCoolDown);

//             if(bicycleController.rWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude > 500 && rHitCoolDown == 0 && hopCoolDown == 0)
//             {
//                 tyreHitRAudioSource.Play();
//                 tyreHitRAudioSource.pitch = Mathf.Clamp(bicycleController.rWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000, 0.75f,1.5f);
//                 tyreHitRAudioSource.volume = Mathf.Clamp(bicycleController.rWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 5000,0,0.2f);
//                 rHitCoolDown = 1;
//             }
//             rHitCoolDown -=Time.deltaTime;
//             rHitCoolDown = Mathf.Clamp01(rHitCoolDown);

//             if(bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude > 1000 && bHitCoolDown == 0)
//             {
//                 bodyHitAudioSource.Play();
//                 bodyHitAudioSource.pitch = bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000;
//                 bodyHitAudioSource.volume = bicycleController.fWheelRb.GetComponent<ConfigurableJoint>().currentForce.magnitude / 1000;
//                 bHitCoolDown = 1;
//             }
//             bHitCoolDown -=Time.deltaTime;
//             bHitCoolDown = Mathf.Clamp01(bHitCoolDown);
            
            





//         }
//     }
// }
