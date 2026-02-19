using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.Remoting.Messaging;
using Leap;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Claw : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI debugTextField;
    Rigidbody body;
    public LeapProvider leapProvider; // Assign your LeapProvider here
    Frame curFrame;
    bool _isGrabbing = false;
    [SerializeField] float grab = 0.8f;
    [SerializeField] float release = 0.4f;
    [SerializeField] Transform centerPoint;
    [SerializeField] float CamMoveSpeed = 1;
    [SerializeField] float CamTurnSpeed = 90;
    [SerializeField] Vector3 centerOffset = Vector3.zero;
   
    Vector3 camHome = Vector3.zero;


    Vector3 prevLeftHandTrack = Vector3.zero;
    [SerializeField] float leftHandTrackSpeed = 10;
    bool validLeftHandSpeed = false;
    Vector3 leftHandGrabPos = Vector3.zero;
    bool _isLeftGrabbing = false;

    Vector3 cameraGrabPos = Vector3.zero;

    Vector3 prevRightHandTrack = Vector3.zero;
    [SerializeField] float camGrabSpeed = 3.0f;
    [SerializeField] float rightHandTrackSpeed = 10;
    bool validRighHandSpeed = false;

    float releaseObjTime = 0;

    [SerializeField] Transform viewCam;

    Animator animator;

    /// OBJECTS ///

    GameObject item; // what I'm touching
    GrabObject holdItem; //what I alr grabbed

    



    void Start()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        PlatformController.singleton.Init("COM5", 115200);
        viewCam = centerPoint;
        camHome = Camera.main.transform.localPosition;

    }

    private void OnEnable()
    {
        if (leapProvider != null)
        {
            leapProvider.OnUpdateFrame += OnUpdateFrame;
        }
    }

    private void OnDisable()
    {
        if (leapProvider != null)
        {
            leapProvider.OnUpdateFrame -= OnUpdateFrame;
        }
    }

    void OnUpdateFrame(Frame frame)
    {
        curFrame = frame;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (viewCam.gameObject.name == "CenterPoint")
            {
                viewCam = Camera.main.transform;
            }
            else
            {
                Camera.main.transform.localPosition = camHome;
                viewCam = centerPoint.transform;
            }
        }

        if (curFrame != null && curFrame.Hands.Count > 0)
        {
            Hand leftHand = curFrame.GetHand(Chirality.Left);
            Hand rightHand = curFrame.GetHand(Chirality.Right);

            if (leftHand != null)
            {
                Vector3 leftHandDelta = (leftHand.PalmPosition - prevLeftHandTrack) * 1000;
                prevLeftHandTrack = leftHand.PalmPosition;

                float moveLimit = camGrabSpeed;

                if (holdItem != null)
                {
                    moveLimit = holdItem.speedFall;
                }

               /* //// left hand (if pinch too fast it falls
                if (leftHand.PinchStrength >= grab)
                {
                    if (validLeftHandSpeed && Mathf.Abs(leftHandDelta.magnitude) > moveLimit)
                    {
                        _isGrabbing = false;
                        releaseObjTime = Time.time;
                        OnRelease();
                    }
                } */

                if (!_isLeftGrabbing)
                {

                    if (leftHand.PinchStrength >= grab)
                    {
                        _isLeftGrabbing = true;
                        leftHandGrabPos = leftHand.PalmPosition;
                        cameraGrabPos = viewCam.position;
                    }
                }
                else
                {
                    if (leftHand.PinchStrength <= release)
                    {
                        _isLeftGrabbing = false;

                    }

                    float leftMoveLimit = camGrabSpeed;

                    if (holdItem != null)
                    {
                        leftMoveLimit = holdItem.camSpeed;
                    }

                    Vector3 mount = leftHand.PalmPosition - leftHandGrabPos;
                    viewCam.position = cameraGrabPos - mount * leftMoveLimit;
                }

                validLeftHandSpeed = true;
            }
            else
            { validLeftHandSpeed = false; }



            /*float leftHandRoll = Mathf.Clamp(Mathf.DeltaAngle(leftHand.Rotation.eulerAngles.z, 0), -60, 60);
            float leftHandPitch = Mathf.Clamp(Mathf.DeltaAngle(leftHand.Rotation.eulerAngles.x, 0), -60, 60);

            if (Mathf.Abs(leftHandRoll) > 15) //death zone
            {
                float amount = leftHandRoll / 60.0f * CamMoveSpeed * Time.deltaTime;
                Camera.main.transform.Translate(amount, 0, 0, Space.World);
                centerPoint.transform.Translate(amount, 0, 0, Space.World);

                //float rAmount = leftHandRoll / 60.0f * CamTurnSpeed * Time.deltaTime;
                //Camera.main.transform.Rotate(0, rAmount, 0, Space.World);
                //centerPoint.transform.Rotate(0, rAmount, 0, Space.World);
            }
            if (Mathf.Abs(leftHandPitch) > 20)
            {
                float amount = leftHandPitch / 60.0f * CamMoveSpeed * Time.deltaTime;
                Camera.main.transform.Translate(0, 0, amount, Space.World);
                centerPoint.transform.Translate(0, 0, amount, Space.World);
            }*/

            if (rightHand != null)
            {
                transform.position = centerPoint.position + centerOffset + rightHand.PalmPosition;


                Vector3 rightHandDelta = (rightHand.PalmPosition - prevRightHandTrack) * 1000;
                prevRightHandTrack = rightHand.PalmPosition;

                float hx = Mathf.Clamp(rightHand.PalmPosition.x / 0.3f, -1, 1);
                float hz = Mathf.Clamp(rightHand.PalmPosition.z / 0.3f, -1, 1);
                float hy = Mathf.Clamp(rightHand.PalmPosition.y / 0.3f, 0.2f, 1.5f);

                PlatformController.singleton.Heave = MapRange(hy, 0.2f, 1.5f, -5, 8);
                /*
                //PlatformController.singleton.Sway = MapRange(hx, -1,1, -24, 23);
                //PlatformController.singleton.Surge = MapRange(hz, -1,1, -22, 22);*/

                float swayMin = -18;
                float swayMax = 18;
                float surgeMin = -18;
                float surgeMax = 18;

                //if (PlatformController.singleton.Heave >= 0)
                //{
                //    // Platform up +5
                //    swayMin = -22f;
                //    swayMax = 22f;

                //    surgeMin = -24f;
                //    surgeMax = 23f;
                //}
                //else
                //{
                //    // Platform down -5
                //    swayMin = -11f;
                //    swayMax = 11f;

                //    surgeMin = -17f;
                //    surgeMax = 12f;
                //}

                PlatformController.singleton.Sway = MapRange(hx, -1, 1, swayMin, swayMax);
                PlatformController.singleton.Surge = MapRange(hz, -1, 1, surgeMin, surgeMax);


                if (holdItem != null)
                {
                    if (holdItem.speedFall > 0)
                    {
                        PlatformController.singleton.Pitch = Random.Range(-holdItem.weight, holdItem.weight);
                        PlatformController.singleton.Roll = Random.Range(-holdItem.weight, holdItem.weight);
                    }

                }
                else
                {
                    PlatformController.singleton.Pitch = 0; PlatformController.singleton.Roll = 0;
                }



                float strength = rightHand.PinchStrength;

                animator.SetFloat("Close", strength);

                float moveLimit = 0;

                if (holdItem != null)
                {
                    moveLimit = holdItem.speedFall;
                }
                if (!_isGrabbing) // if not currently grabbing
                {
                    // are we trying to grab?
                    if (strength > grab && Time.time > releaseObjTime + 1)
                    {
                        _isGrabbing = true;
                        OnGrab();
                    }
                }
                else if (_isGrabbing) // if  currently grabbing
                {
                    // should we release due to grab strength (hand tracker)?
                    if (strength <= release)
                    {
                        _isGrabbing = false;
                        releaseObjTime = Time.time;
                        OnRelease();
                    }
                    // should we release due to moving too fast?
                    else if (validRighHandSpeed && Mathf.Abs(rightHandDelta.magnitude) > moveLimit)
                    {
                        _isGrabbing = false;
                        releaseObjTime = Time.time;
                        OnRelease();
                    }
                }
                validRighHandSpeed = true;
            }
            else
            {
                validRighHandSpeed = false;
            }


        }

        /*Vector3 lookDir = transform.position - Camera.main.transform.position; //direction camera to look
        lookDir.x = 0;

        Camera.main.transform.rotation = Quaternion.LookRotation(lookDir); //turns rotation */

    }

    public void OnGrab()
    {
        if (item != null && holdItem == null)
        {
            GrabObject obj = item.GetComponent<GrabObject>();
            ///objects that can be grabbed for tests

            if ((obj != null))
            {

                holdItem = obj;

                Rigidbody rb = holdItem.GetComponent<Rigidbody>();
                rb.isKinematic = true;

                Collider col = holdItem.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                holdItem.transform.SetParent(transform);

                print("Grabbed: " + holdItem.name);
            }
        }
    }

    public void OnRelease()
    {
        if (holdItem != null)
        {
            Rigidbody rb = holdItem.GetComponent<Rigidbody>();
            rb.isKinematic = false;

            Collider col = holdItem.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            holdItem.transform.SetParent(null);
            holdItem = null;

            print("Release");

        }

    }

    ///Detects Object///

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            item = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == item)
        {
            item = null;
        }
    }

    public static float MapRange(float val, float oldMin, float oldMax, float newMin, float newMax)
    {
        float slope = (newMax - newMin) / (oldMax - oldMin);
        float newVal = newMin + slope * (val - oldMin);
        return Mathf.Clamp(newVal, Mathf.Min(newMin, newMax), Mathf.Max(newMin, newMax));
    }

}