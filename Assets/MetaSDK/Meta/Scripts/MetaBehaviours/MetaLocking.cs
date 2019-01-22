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

namespace Meta {
    /// <summary>
    /// Allow for transform locking relative to the user and/or MetaCameras.
    /// </summary>
    public class MetaLocking : MetaBehaviour {

        [SerializeField]
        private bool _hud;

        [SerializeField]
        private bool _hudLockPosition = true;

        [SerializeField]
        private bool _hudLockPositionX = true;

        [SerializeField]
        private bool _hudLockPositionY = true;

        [SerializeField]
        private bool _hudLockPositionZ = true;

        [SerializeField]
        private bool _hudLockRotation = true;

        [SerializeField]
        private bool _hudLockRotationX = true;

        [SerializeField]
        private bool _hudLockRotationY = true;

        [SerializeField]
        private bool _hudLockRotationZ = true;


        /* commented out until user reach distance settings are available again
        [SerializeField]
        private bool _userReachDistance = true;
        /// <summary>
        /// Whether the orbital lock distance should be set to the user reach distance 
        /// (requires Meta.MetaBody.useDefaultOrbitalSettings to be false).
        /// </summary>
        /// <example><b>Example usage:</b>\n<code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>();
        /// mB.orbital = true;                      // Set the object to be locked to orbital
        /// mB.useDefaultOrbitalSettings = false;   // Disable default orbital settings
        /// mB.userReachDistance = false;           // Override the orbital lock distance
        /// mB.lockDistance = 0.5f;                 // Locks the object to be 0.5m away (default: 0.4)
        /// </code></example>
        public bool userReachDistance
        {
            get { return _userReachDistance; }
            set { _userReachDistance = value; }
        }
        */

        [SerializeField]
        private float _lockDistance = 0.4f;

        [SerializeField]
        private bool _orbital;

        [SerializeField]
        private bool _orbitalLockDistance = true;

        [SerializeField]
        private bool _orbitalLookAtCamera = true;

        [SerializeField]
        private bool _orbitalLookAtCameraFlipY = true;


        [SerializeField]
        private bool _useDefaultHUDsettings = true;

        [SerializeField]
        private bool _useDefaultOrbitalSettings = true;


        /// <summary>
        ///     Whether the object is locked to orbital (ie. is always at a fixed distance away
        ///     and always facing the user).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.orbital = true;      // Set the object to be locked to orbital
        /// </code>
        /// </example>
        public bool orbital
        {
            get { return _orbital; }
            set
            {
                _orbital = value;
                if (value)
                {
                    hud = false;
                    if (Application.isPlaying)
                        metaContext.Get<OrbitalLock>().AddOrbitalLockedObject(this);
                }
                else
                {
                    if (Application.isPlaying)
                        metaContext.Get<OrbitalLock>().RemoveOrbitalLockedObject(this);
                }
            }
        }

        /// <summary>
        ///     Whether the default orbital settings are used.
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.orbital = true;                      // Set the object to be locked to orbital
        ///             mB.useDefaultOrbitalSettings = false;   // Disable default orbital settings
        ///             mB.orbitalLockDistance = false;         // Allow object to be moved to any distance
        /// </code>
        /// </example>
        public bool useDefaultOrbitalSettings
        {
            get { return _useDefaultOrbitalSettings; }
            set { _useDefaultOrbitalSettings = value; }
        }

        /// <summary>
        ///     Whether the orbital locked object has their lock distance set (requires
        ///     Meta.MetaBody.useDefaultOrbitalSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.orbital = true;                      // Set the object to be locked to orbital
        ///             mB.useDefaultOrbitalSettings = false;   // Disable default orbital settings
        ///             mB.orbitalLockDistance = false;         // Allow object to be moved to any distance
        /// </code>
        /// </example>
        public bool orbitalLockDistance
        {
            get { return _orbitalLockDistance; }
            set { _orbitalLockDistance = value; }
        }

        /// <summary>
        ///     The distance that the orbital object is locked to (requires Meta.MetaBody.useDefaultOrbitalSettings
        ///     and Meta.MetaBody.userReachDistance to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.orbital = true;                      // Set the object to be locked to orbital
        ///             mB.useDefaultOrbitalSettings = false;   // Disable default orbital settings
        ///             mB.userReachDistance = false;           // Override the orbital lock distance
        ///             mB.lockDistance = 0.5f;                 // Locks the object to be 0.5m away (default: 0.4)
        /// </code>
        /// </example>
        public float lockDistance
        {
            get { return _lockDistance; }
            set { _lockDistance = value>0f?value:_lockDistance; }
        }

        /// <summary>
        ///     Whether the orbital locked object is rotated to always face the camera (user)
        ///     (requires Meta.MetaBody.useDefaultOrbitalSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.orbital = true;                      // Set the object to be locked to orbital
        ///             mB.useDefaultOrbitalSettings = false;   // Disable default orbital settings
        ///             mB.orbitalLookAtCamera = false;         // Disable automatic rotation of object to face the camera
        /// </code>
        /// </example>
        public bool orbitalLookAtCamera
        {
            get { return _orbitalLookAtCamera; }
            set { _orbitalLookAtCamera = value; }
        }

        /// <summary>
        ///     Whether orbital look at camera objects have their Y-value flipped 180 degrees. This
        ///     is useful for objects that appear to be backwards
        ///     (requires Meta.MetaBody.useDefaultOrbitalSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.orbital = true;                      // Set the object to be locked to orbital
        ///             mB.useDefaultOrbitalSettings = false;   // Disable default orbital settings
        ///             mB.orbitalLookAtCameraFlipY = true;     // Flip object along Y axis by 180 degrees
        /// </code>
        /// </example>
        public bool orbitalLookAtCameraFlipY
        {
            get { return _orbitalLookAtCameraFlipY; }
            set { _orbitalLookAtCameraFlipY = value; }
        }

        /// <summary>
        ///     Whether the object is locked to the HUD (ie. is always in a fixed position
        ///     in the user's view, like a heads-up display).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;      // Set the object to be locked to HUD
        /// </code>
        /// </example>
        public bool hud
        {
            get { return _hud; }
            set
            {
                _hud = value;
                if (value)
                {
                    orbital = false;
                    if (Application.isPlaying)
                        metaContext.Get<HudLock>().AddHudLockedObject(this);
                }
                else
                {
                    if (Application.isPlaying)
                        metaContext.Get<HudLock>().RemoveHudLockedObject(this);
                }
            }
        }

        /// <summary>
        ///     Whether the default HUD settings should be used.
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockPosition = false;         // Stops the object's position from being fixed in the HUD
        /// </code>
        /// </example>
        public bool useDefaultHUDSettings
        {
            get { return _useDefaultHUDsettings; }
            set { _useDefaultHUDsettings = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its position locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockPosition = false;         // Stops the object's position from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockPosition
        {
            get { return _hudLockPosition; }
            set { _hudLockPosition = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its X position locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockPositionX = false;        // Stops the object's X position from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockPositionX
        {
            get { return _hudLockPositionX; }
            set { _hudLockPositionX = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its Y position locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockPositionY = false;        // Stops the object's Y position from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockPositionY
        {
            get { return _hudLockPositionY; }
            set { _hudLockPositionY = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its Z position locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockPositionZ = false;        // Stops the object's Z position from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockPositionZ
        {
            get { return _hudLockPositionZ; }
            set { _hudLockPositionZ = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its rotation locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockRotation = false;         // Stops the object's rotation from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockRotation
        {
            get { return _hudLockRotation; }
            set { _hudLockRotation = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its X rotation locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockRotationX = false;        // Stops the object's X rotation from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockRotationX
        {
            get { return _hudLockRotationX; }
            set { _hudLockRotationX = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its Y rotation locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockRotationY = false;        // Stops the object's Y rotation from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockRotationY
        {
            get { return _hudLockRotationY; }
            set { _hudLockRotationY = value; }
        }

        /// <summary>
        ///     Whether the HUD locked object has its Z rotation locked
        ///     (requires Meta.MetaBody.useDefaultHUDSettings to be false).
        /// </summary>
        /// <example>
        ///     <b>Example usage:</b>\n
        ///     <code>
        /// using Meta;
        /// ...
        /// MetaBody mB = gameObject.GetComponent<MetaBody>
        ///             ();
        ///             mB.hud = true;                      // Set the object to be locked to HUD
        ///             mB.useDefaultHUDSettings = false;   // Disable default HUD settings
        ///             mB.hudLockRotationZ = false;        // Stops the object's Z rotation from being fixed in the HUD
        /// </code>
        /// </example>
        public bool hudLockRotationZ
        {
            get { return _hudLockRotationZ; }
            set { _hudLockRotationZ = value; }
        }

        /// <summary>
        ///     Sets the properties to call their setters
        /// </summary>
        private void Start()
        {
            hud = _hud;
            orbital = _orbital;
        }
    }
}
