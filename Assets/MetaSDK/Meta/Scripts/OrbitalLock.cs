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
using System.Collections.Generic;

namespace Meta
{

    /// <summary>
    /// The OrbitalLock class makes GameObjects look at the MainCamera and
    /// locks GameObjects at a constant distance away from the MainCamera
    /// </summary>
    internal class OrbitalLock : IEventReceiver
    {

        /// <summary>
        /// The camera's position in the last frame
        /// </summary>
        private Vector3? _oldCameraPos = null;

        private static readonly float epsilon = 0.001f;

        /// <summary>
        /// The lock distance when useDefaultOrbitalSettings = true
        /// </summary>
        // private float _defaultLockDistance = 0.4f;  // TODO: actually use this distance, or remove the useDefaultOrbitalSettings flag
        /// <summary>
        /// List of orbit locked MetaBodies
        /// </summary>
        private List<MetaLocking> _orbitLockedObjects = new List<MetaLocking>();

        /// <summary>
        /// Adds MetaBodies to the list of lockables
        /// </summary>
        internal void AddOrbitalLockedObject(MetaLocking orbitLockedObject)
        {
            if (!_orbitLockedObjects.Contains(orbitLockedObject))
            {
                _orbitLockedObjects.Add(orbitLockedObject);
            }
        }

        /// <summary>
        /// MetaBodies from the list of lockables
        /// </summary>
        internal void RemoveOrbitalLockedObject(MetaLocking orbitLockedObject)
        {
            if (_orbitLockedObjects.Contains(orbitLockedObject))
            {
                _orbitLockedObjects.Remove(orbitLockedObject);
            }
        }

        /// <summary>
        /// Adds the IEventReceiver functions to the delegates in order to be called from MetaManager
        /// </summary>
        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }

        // Update is called once per frame
        private void Update()
        {
            UpdateOrbitalLocks();
        }

        /// <summary>
        /// Updates the position and rotations of the orbital locked objects
        /// so that they are at the lock distance away from the camera and look at the camera
        /// </summary>
        private void UpdateOrbitalLocks()
        {
            var cameraPos = Camera.main.transform.position;
            foreach (MetaLocking metaLocking in _orbitLockedObjects)
            {
                if (metaLocking != null)
                {
                    if (metaLocking.useDefaultOrbitalSettings || metaLocking.orbitalLockDistance)
                    {
                        //at least one frame must have passed to have the old camera position.
                        if (_oldCameraPos.HasValue)
                        {
                            // This is the primary method of updating the target object's location relative to
                            // the meta2.  
                            var delta = (cameraPos - _oldCameraPos.Value);
                            metaLocking.transform.position += delta;
                        }

                        //The difference between the true distance of the orbit gameobject and the meta2 camera and the desired distance.
                        var magnitudeDifference = Mathf.Abs((metaLocking.transform.position - cameraPos).magnitude - metaLocking.lockDistance);
                        if (magnitudeDifference > epsilon)
                        {
                            // Only modify the target object's position relative to the camera along a vector with precision
                            // issues if the target object is not the desired distance from the camera. 
                            // This usually happens if the user moves the target object, and should not happen frequently
                            // when the camera moves. 
                            // This FeatureType is designed to prevent the position of the target object from changing relative
                            //  to the meta2 when the meta2 is moved.
                            Vector3 lookVector = (metaLocking.transform.position - cameraPos).normalized;
                            metaLocking.transform.position = (lookVector * metaLocking.lockDistance + cameraPos);
                        }
                    }
                    if (metaLocking.useDefaultOrbitalSettings || metaLocking.orbitalLookAtCamera)
                    {
                        Vector3 lookVector = Camera.main.transform.position - metaLocking.transform.position;
                        Quaternion lookRotation = Quaternion.LookRotation(lookVector);
                        if (metaLocking.useDefaultOrbitalSettings || metaLocking.orbitalLookAtCameraFlipY)
                        {
                            lookRotation *= Quaternion.Euler(new Vector3(0, 180, 0));
                        }
                        if (metaLocking.transform.rotation != lookRotation)
                        {
                            metaLocking.transform.rotation = lookRotation;
                        }
                    }
                }
            }

            _oldCameraPos = cameraPos;
        }
    }
}
