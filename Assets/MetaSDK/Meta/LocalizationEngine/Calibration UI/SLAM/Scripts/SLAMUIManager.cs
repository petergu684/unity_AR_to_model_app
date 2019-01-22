// Copyright Â© 2018, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in binary form, without modification, is 
// permitted provided that the following conditions are met:
// 
// 1.      Redistributions of the unmodified Software in binary form must reproduce the above 
//         copyright notice, this list of conditions and the following disclaimer in the 
//         documentation and/or other materials provided with the distribution.
// 2.      The name of Meta Company (â€œMetaâ€) may not be used to endorse or promote products derived 
//         from this Software without specific prior written permission from Meta.
// 3.      LIMITATION TO META PLATFORM: Use of the Software is limited to use on or in connection 
//         with Meta-branded devices or Meta-branded software development kits.  For example, a bona 
//         fide recipient of the Software may incorporate an unmodified binary version of the 
//         Software into an application limited to use on or in connection with a Meta-branded 
//         device, while he or she may not incorporate an unmodified binary version of the Software 
//         into an application designed or offered for use on a non-Meta-branded device.
// 
// For the sake of clarity, the Software may not be redistributed under any circumstances in source 
// code form, or in the form of modified binary code â€“ and nothing in this License shall be construed 
// to permit such redistribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using UnityEngine;
using System.Collections;

namespace Meta
{
    using System;

    public class SLAMUIManager : MonoBehaviour
    {
        public SLAMInitializationGazePoint slamInitPrefab;
        public Transform eyeCamera;

        [Header("Head Rotation Settings")]
        //public Camera EyeCamera;
        public float fromAngle = -100;
        public float toAngle = 100;
        public int stepCountHalfCircle = 15;

        [Header("Procedure Settings")]
        public float lookAtTime = .2f;
        public float waitTimeForHelpText = 1f;
        public float waitTimeForHelpArrows = 1f;
        public SLAMParticles particles;

        [Header("Help Animations / Texts")]
        public Animator slamMoveHeadHelp;
        public Animator slamDuringTrackingHelp;
        public Animator slamTrackingDoneHelp;
        public Animator slamTrackingHoldStill;
        public Animator slamLoadingMap;
        public bool useDirectionArrows = true;
        public Animator leftArrows, rightArrows;

        [Header("Blinking Arrow Animation")]
        public bool useArrowAnimation = false;
        public float blinkSpeed = 0.3f;
        public float blinkShift = 0.5f;

        public AnimationCurve fadingCurve;

        [Header("Output")]
        public float timeSinceLastPoint = 0;
        public bool isCalibrating = false;

        // SLAM Particles need these
        [HideInInspector]
        public Transform currentTarget;
        // [HideInInspector]
        public int direction;
        SLAMInitializationGazePoint[] _points;

        private Animator _additionalMessage;
        private SlamLocalizer _slamLocalizer;

        /// <summary>
        /// Generate the points for SLAM initialization that the user has to look at.
        /// TODO: If we go with the current version, only one point will be needed. If we go with the arrow version, all points are needed.
        /// </summary>
        private void Start()
        {
            // build all slam init points for the specified angle and step settings
            // "halfCircleCount * 2" pieces for "fromAngle" to "toAngle"
            // save them in an array
            // then do calibration fromIndex toIndex and use these points (activate/deactivate/show correct graphics/animation/light)

            _slamLocalizer = FindObjectOfType<SlamLocalizer>();

            if (eyeCamera == null)
            {
                eyeCamera = GameObject.Find("StereoCameras").transform;
            }

            if (eyeCamera == null)
            {
                Debug.LogError("Couldn't find stereo cameras!");
            }

            particles.gameObject.SetActive(false);
            leftArrows.gameObject.SetActive(useDirectionArrows);
            rightArrows.gameObject.SetActive(useDirectionArrows);

            var pointCount = useArrowAnimation ? stepCountHalfCircle * 2 : 1;
            pointCount = Mathf.RoundToInt(useArrowAnimation ? (stepCountHalfCircle * 2) : 1);

            _points = new SLAMInitializationGazePoint[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                var angle = Mathf.Lerp(-180, 180, (float)i / (pointCount + 1));

                SLAMInitializationGazePoint gazePoint = Instantiate(slamInitPrefab);
                gazePoint.EyeCamera = eyeCamera;
                gazePoint.transform.parent = transform;
                gazePoint.transform.localPosition = Vector3.zero;
                gazePoint.transform.localRotation = Quaternion.Euler(0, angle, 0);
                gazePoint.requiredLookAtTime = lookAtTime;

                // initialize and hide these points
                // in non-arrow mode, they are invisible anyways
                gazePoint.direction = 1;
                gazePoint.Activate(false);
                gazePoint.number = i;
                gazePoint.slamUI = this;
                _points[i] = gazePoint;
            }
        }

        /// <summary>
        /// Hide all messages, cleanup work.
        /// </summary>
        private void OnDisable()
        {
            if (slamMoveHeadHelp != null && slamMoveHeadHelp.isActiveAndEnabled)
            {
                slamMoveHeadHelp.SetBool("Show", false);
            }
            if (slamDuringTrackingHelp != null && slamDuringTrackingHelp.isActiveAndEnabled)
            {
                slamDuringTrackingHelp.SetBool("Show", false);
            }
            if (slamTrackingDoneHelp != null && slamTrackingDoneHelp.isActiveAndEnabled)
            {
                slamTrackingDoneHelp.SetBool("Show", false);
            }
            if (slamTrackingHoldStill != null && slamTrackingHoldStill.isActiveAndEnabled)
            {
                slamTrackingHoldStill.SetBool("Show", false);
            }
        }

        // Use this for initialization
        /// <param name="initializationType"></param>
        public IEnumerator RunFullCalibration(SLAMInitializationProcess.SlamInitializationType initializationType)
        {
            isCalibrating = true;

            gameObject.SetActive(true);
            timeSinceLastPoint = 100;

            switch (initializationType)
            {
                case SLAMInitializationProcess.SlamInitializationType.LoadingMap:
                    yield return LoadingMapCalibration();
                    break;
                case SLAMInitializationProcess.SlamInitializationType.NewMap:
                    yield return StartCoroutine(NewMapCalibration());
                    break;
                default:
                    throw new Exception(string.Format("Calibration UI does not support {0} initializationType", initializationType));
            }

            // wait for filter initialization
            // show hold still message 
            slamTrackingHoldStill.SetBool("Show", true);
            while (!CheckForFilterInitComplete())
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // fitler initialized, mapping complete
            // fade "hold still" out
            yield return new WaitForSecondsRealtime(1.5f);
            slamTrackingHoldStill.SetBool("Show", false);

            // show done help text
            slamTrackingDoneHelp.SetBool("Show", true);

            StartCoroutine(ParticleEnding(1.5f));

            // fade "done" message out
            yield return new WaitForSecondsRealtime(1.5f);
            slamTrackingDoneHelp.SetBool("Show", false);

            // wait for UI to fade out
            yield return new WaitForSecondsRealtime(1.5f);

            // deactivate this step
            gameObject.SetActive(false);
        }

        private IEnumerator ParticleEnding(float waitTime)
        {
            // end animation with lightband drawing a circle at high speed
            if (particles)
            {
                particles.GoCrazy();
                yield return new WaitForSecondsRealtime(waitTime);
                particles.DoStop();
            }
        }

        bool CheckForSLAMInitComplete()
        {
            // for debugging
            // return false;
            var feedback = _slamLocalizer.SlamFeedback;

            return ((feedback.tracking_ready > 0) && (feedback.camera_ready > 0) && (feedback.scale_quality_percent >= 100));
        }

        bool CheckForFilterInitComplete()
        {
            // for debugging
            // return false;
            var feedback = _slamLocalizer.SlamFeedback;

            return feedback.FilterReady;
        }

        #region HelperCoroutines

        private IEnumerator NewMapCalibration()
        {
            // helper coroutines -
            // displays text whenever user gets stuck (movement is not smooth)
            // display direction arrows whenever the user gets stuck
            StartCoroutine(ShowDuringTrackingHelp());

            if (useDirectionArrows)
            {
                StartCoroutine(ShowArrows());
            }

            // wait for first SLAM initialization
            // yield return StartCoroutine(WaitForHeadMovement());

            // activate lightband particles
            if (particles)
            {
                particles.DoStart();
            }

            // calculate based on angles
            int first = stepCountHalfCircle - Mathf.RoundToInt(Mathf.Abs(fromAngle) / 180 * stepCountHalfCircle);
            int midStart = stepCountHalfCircle + 0; // offset the start position a bit to the right so it is not activated in the beginning
            int end = stepCountHalfCircle + Mathf.RoundToInt(Mathf.Abs(toAngle) / 180 * stepCountHalfCircle);

            // start with turning your head from the middle to the left 
            yield return StartCoroutine(CalibrateDirectional(midStart, first));

            // while SLAM is not finished, make the user look left and right and vice versa
            // at any point, the SLAM subsystem might be "done", and then we need some early-out. Or we need to repeat the process!
            while (!CheckForSLAMInitComplete())
            {
                // turn your head from left to right 
                yield return StartCoroutine(CalibrateDirectional(first, end));

                // turn your head from right to left 
                yield return StartCoroutine(CalibrateDirectional(end, first));
            }

            currentTarget = null;
            isCalibrating = false;

            // hide direction arrows
            if (useDirectionArrows && leftArrows && rightArrows)
            {
                leftArrows.SetBool("IsOn", false);
                rightArrows.SetBool("IsOn", false);
            }

            // hide help text
            yield return new WaitForSecondsRealtime(1.0f);
            slamMoveHeadHelp.SetBool("Show", false);
            slamDuringTrackingHelp.SetBool("Show", false);
        }

        private IEnumerator LoadingMapCalibration()
        {
            slamLoadingMap.SetBool("Show", true);

            while (!CheckForSLAMInitComplete())
            {
                yield return 0;
            }

            slamLoadingMap.SetBool("Show", false);
        }

        private IEnumerator CalibrateDirectional(int fromIndex, int toIndex)
        {
            // check for early-out if slam calibration is fully done already
            if (CheckForSLAMInitComplete())
            {
                yield break;
            }

            if (useArrowAnimation)
            {
                yield return StartCoroutine(CalibrateDirectional_Arrows(fromIndex, toIndex));
            }
            else
            {
                yield return StartCoroutine(CalibrateDirectional_SinglePoint(fromIndex, toIndex));
            }
        }

        /// <summary>
        /// Activates a single (invisible) point at a time and moves that around to have the user look at it.
        /// </summary>
        IEnumerator CalibrateDirectional_SinglePoint(int fromIndex, int toIndex)
        {
            timeSinceLastPoint = 0;
            direction = fromIndex > toIndex ? -1 : 1;
            leftArrows.SetBool("IsOn", direction > 0);
            rightArrows.SetBool("IsOn", direction < 0);

            int i = fromIndex;
            while (i != toIndex)
            {
                SLAMInitializationGazePoint pCurrent = _points[0];

                // set point rotation
                var angle = Mathf.Lerp(fromAngle, toAngle, (float)i / (stepCountHalfCircle * 2 + 1));
                pCurrent.transform.localRotation = Quaternion.Euler(0, angle, 0);

                // set up point for gazing
                pCurrent.direction = -direction;
                pCurrent.allowGazing = true;
                pCurrent.Init();
                pCurrent.Activate(true);

                currentTarget = pCurrent.r.transform;

                // wait until point has been gazed at for long enough
                while (pCurrent.isActive)
                {
                    // check for early-out if slam calibration is fully done already
                    if (CheckForSLAMInitComplete())
                    {
                        timeSinceLastPoint = 0;
                        yield break;
                    }

                    timeSinceLastPoint += Time.unscaledDeltaTime;
                    yield return null;
                }

                timeSinceLastPoint = 0;
                i += direction;
            }
        }

        /// <summary>
        /// Shows multiple points at the same time and animates their blinking.
        /// </summary>
        private IEnumerator CalibrateDirectional_Arrows(int fromIndex, int toIndex)
        {
            timeSinceLastPoint = 0;
            direction = fromIndex > toIndex ? -1 : 1;

            if (useDirectionArrows && leftArrows && rightArrows)
            {
                leftArrows.SetBool("IsOn", direction > 0);
                rightArrows.SetBool("IsOn", direction < 0);
            }

            int i = fromIndex;
            while (i != toIndex)
            {
                Debug.Log("waiting for point " + i);
                SLAMInitializationGazePoint pCurrent = _points[i];

                pCurrent.direction = -direction;
                pCurrent.allowGazing = true;
                pCurrent.Init();
                pCurrent.Activate(true);

                currentTarget = pCurrent.r.transform;

                // wait until this point is detected (looked at for a minimum time set in the point prefab)
                while (pCurrent.isActive)
                {
                    Debug.DrawLine(Vector3.zero, pCurrent.r.transform.position, Color.green);

                    // check for early-out if slam calibration is fully done already
                    if (CheckForSLAMInitComplete())
                    {
                        timeSinceLastPoint = 0;
                        yield break;
                    }

                    timeSinceLastPoint += Time.unscaledDeltaTime;
                    yield return null;
                }

                timeSinceLastPoint = 0;
                i += direction;
            }

            // wait for points to fade out
        }

        /// <summary>
        /// User has to move his head to get SLAM from "sensors initialized" to "tracking initialized".
        /// </summary>
        private IEnumerator WaitForHeadMovement()
        {
            // wait for event from SLAM system that tracking has started.
            // what to do if no event comes?

            // for testing: wait for some total angular movement (here 20°)
            var startVector = eyeCamera.transform.forward;
            var angle = 0f;
            while (angle < 10)
            {
                angle += Vector3.Angle(eyeCamera.transform.forward, startVector) * Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Show help during tracking at the beginning and if the user hasn't gotten rid of any target points for some seconds.
        /// </summary>
        private IEnumerator ShowDuringTrackingHelp()
        {
            slamMoveHeadHelp.SetBool("Show", true);
            yield return new WaitForSecondsRealtime(4);

            slamMoveHeadHelp.SetBool("Show", false);

            while (isCalibrating)
            {
                // user needs help if he didnt calibrate a point in the last couple seconds
                var userNeedsHelp = timeSinceLastPoint > waitTimeForHelpText;
                // also show help for the first couple seconds
                // userNeedsHelp |= (Time.time - startTime < 3);

                slamDuringTrackingHelp.SetBool("Show", userNeedsHelp);
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        /// <summary>
        /// Show arrow animation pointing the user into the right direction when he gets stuck.
        /// </summary>
        private IEnumerator ShowArrows()
        {
            while (isCalibrating)
            {
                var userNeedsHelp = timeSinceLastPoint > waitTimeForHelpArrows;

                if (currentTarget == null)
                {
                    yield return null;
                }
                else
                {
                    // see whether we are left or right of the next target point
                    // show arrows accordingly
                    Vector3 eyeRightVector = Vector3.Cross(eyeCamera.transform.forward, -Vector3.up).normalized;
                    Vector3 objectForwardVector = (currentTarget.position - eyeCamera.transform.position).normalized;

                    // get dot product of these
                    float dir = Vector3.Dot(objectForwardVector, eyeRightVector);

                    leftArrows.SetBool("IsOn", userNeedsHelp && (dir < 0));
                    rightArrows.SetBool("IsOn", userNeedsHelp && (dir > 0));

                    yield return null;
                }
            }

            leftArrows.SetBool("IsOn", false);
            rightArrows.SetBool("IsOn", false);
        }

        /// <summary>
        /// Make sure that all sensors needed for this procedure are up and running.
        /// TODO: Not only wait for IMU
        /// </summary>
        private IEnumerator WaitForSensorInitialization()
        {
            yield break;
        }
        #endregion
    }
}
