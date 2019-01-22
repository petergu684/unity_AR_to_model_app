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
using UnityEngine.Audio;
using UnityEngine.EventSystems;

namespace Meta.Events
{
    /// <summary>
    /// Plays audio on pointer interactions
    /// </summary>
    public class AudioEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField]
        private AudioClip _downClickSound = null;

        [SerializeField]
        private AudioClip _upClickSound = null;

        [SerializeField]
        private AudioClip _dragUpClickSound = null;

        [SerializeField]
        private AudioClip _dragTickSound = null;

        [SerializeField]
        private AudioClip _dragContinuousSound = null;

        [SerializeField]
        private float _volume = 1f;

        [SerializeField]
        private AudioMixerGroup _audioMixerGroup = null;

        private float _dragSoundAccumulator;
        private AudioSource _oneShotAudioSource;
        private AudioSource _continuousAudioSource;
        private PointerEventData _eventData;

        private void Start()
        {
            _oneShotAudioSource = gameObject.AddComponent<AudioSource>();
            _oneShotAudioSource.spatialBlend = .5f;
            _oneShotAudioSource.volume = _volume;
            _oneShotAudioSource.outputAudioMixerGroup = _audioMixerGroup;
            if (_dragContinuousSound != null)
            {
                _continuousAudioSource = gameObject.AddComponent<AudioSource>();
                _continuousAudioSource.spatialBlend = .5f;
                _continuousAudioSource.loop = true;
                _continuousAudioSource.volume = 0f;
                _continuousAudioSource.clip = _dragContinuousSound;
                _continuousAudioSource.outputAudioMixerGroup = _audioMixerGroup;
                _continuousAudioSource.Play();
            }
        }

        private void Update()
        {
            if (_continuousAudioSource != null)
            {
                if (_eventData != null && _eventData.dragging && _eventData.delta.sqrMagnitude > 10f)
                {
                    _continuousAudioSource.volume += Time.deltaTime*2f;
                }
                else
                {
                    _continuousAudioSource.volume -= Time.deltaTime*2f;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_downClickSound != null)
                _oneShotAudioSource.PlayOneShot(_downClickSound);
            _eventData = eventData;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.dragging && _dragUpClickSound != null)
            {
                _oneShotAudioSource.PlayOneShot(_dragUpClickSound);
            }
            else if (_upClickSound != null)
            {
                _oneShotAudioSource.PlayOneShot(_upClickSound);
            }
            _eventData = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragTickSound != null)
            {
                _dragSoundAccumulator += Mathf.Clamp(eventData.delta.sqrMagnitude, 0f, 30f);
                if (_dragSoundAccumulator > 500f)
                {
                    _dragSoundAccumulator = 0f;
                    if (_dragTickSound != null)
                    {
                        _oneShotAudioSource.PlayOneShot(_dragTickSound);
                    }
                }
            }
        }
    }
}
