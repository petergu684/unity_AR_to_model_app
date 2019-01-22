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
using Meta.Audio;
using UnityEngine;

namespace Meta.HandInput
{
    /// <summary>
    /// Cursor placed on back of hand will display when it has entered a grabbable collider 
    /// and will provide feedback for when it is grabbing.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class HandCursor: MetaBehaviour
    {
        /// <summary>
        /// Represents an edge of the viewport.
        /// </summary>

        [SerializeField]
        private bool _playAudio = true;

        [SerializeField]
        private Transform _cursorTransform;


        [SerializeField]
        private SpriteRenderer _idleSprite;
        [SerializeField]
        private SpriteRenderer _idleContactSprite;
        [SerializeField]
        private SpriteRenderer _hoverSprite;
        [SerializeField]
        private SpriteRenderer _grabSprite;


        

        [SerializeField]
        private AudioRandomizer _grabAudio;

        [SerializeField]
        private AudioRandomizer _releaseAudio;


        private Hand _hand;
        private AudioSource _audioSource;
        private SpriteRenderer _centerOutOfBoundsSpriteRenderer;
        private CenterHandFeature _centerHandFeature;
        private Transform _centerOutOfBoundsSprite;
        private PalmState _lastPalmState = PalmState.Idle;
        private Vector3 _priorPos;
        private bool _vicinityOn = false;


        public bool PlayAudio
        {
            get { return _playAudio; }
            set { _playAudio = value; }
        }

        public AudioRandomizer GrabAudio
        {
            get { return _grabAudio; }
            set { _grabAudio = value; }
        }

        public AudioRandomizer ReleaseAudio
        {
            get { return _releaseAudio; }
            set { _releaseAudio = value; }
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _hand = GetComponentInParent<Hand>();
            _centerHandFeature = GetComponent<CenterHandFeature>();
            _centerHandFeature.OnEngagedEvent.AddListener(OnGrab);
            _centerHandFeature.OnDisengagedEvent.AddListener(OnRelease);
            _priorPos = ComputePriorPosition();
            _idleSprite.enabled = false;
            _idleContactSprite.enabled = false;
            _hoverSprite.enabled = false;
            _grabSprite.enabled = false;
        }

        private Vector3 ComputePriorPosition()
        {
            const float alpha = 0.75f;
            return Vector3.Lerp(_hand.Data.GrabAnchor, _hand.Palm.Position, alpha);
        }

        private void LateUpdate()
        {
            _cursorTransform.position = GetSmoothHandPosition();
            _cursorTransform.LookAt(Camera.main.transform);
            SetCursorVisualState();
        }

        /// <summary>
        /// Updates the visuals for the cursor which are not dependent upon the grab residual.
        /// </summary>
        private void SetCursorVisualState()
        {

            if (_centerHandFeature.PalmState != _lastPalmState)
            {
                switch (_centerHandFeature.PalmState)
                {

                    case PalmState.Idle:
                        _idleContactSprite.enabled = false;
                        _hoverSprite.enabled = false;
                        _grabSprite.enabled = false;
                        break;
                    case PalmState.Hovering:
                        _idleContactSprite.enabled = false;
                        _hoverSprite.enabled = true;
                        _grabSprite.enabled = false;

                        break;
                    case PalmState.Grabbing:
                        _idleContactSprite.enabled = false;
                        _hoverSprite.enabled = false;
                        _grabSprite.enabled = true;
                        break;
                }
            }
            if((_centerHandFeature.NearObjects.Count != 0) ^ _vicinityOn)
            {
                _vicinityOn = !_vicinityOn;
                if(_vicinityOn && _centerHandFeature.PalmState == PalmState.Idle)
                {
                    _idleContactSprite.enabled = true;
                }
                if (!_vicinityOn && _centerHandFeature.PalmState == PalmState.Idle)
                {
                    _idleContactSprite.enabled = false;
                }
            }

            _lastPalmState = _centerHandFeature.PalmState;
        }



        private void OnGrab(HandFeature handFeature)
        {
            PlayAudioClip(true);
        }

        private void OnRelease(HandFeature handFeature)
        {
            PlayAudioClip(false);
        }

        /// <summary>
        /// Checks if the hand is in the out of bounds region for the field of view.
        /// </summary>
        /// <returns>True, if the hand is is outside the pre-defined boundary regions.</returns>


        private void PlayAudioClip(bool isGrabbing)
        {
            if (PlayAudio)
            {
                if (isGrabbing)
                {
                    _grabAudio.Play(_audioSource);
                }
                else
                {
                    _releaseAudio.Play(_audioSource);
                }
            }
        }
        


        private Vector3 GetSmoothHandPosition()
        {
            const float alpha = 0.8f;
            Vector3 smoothPos = Vector3.Lerp(_priorPos, ComputePriorPosition(), alpha);
            _priorPos = smoothPos;
            return smoothPos;
        }
    }
}
