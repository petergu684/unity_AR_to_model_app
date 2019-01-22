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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.SlamUI
{
    /// <summary>
    /// Controller for messages content and animations
    /// </summary>
    public class SlamUIMessages : BaseSlamUIMessages
    {
        [Tooltip("Text field for the message title")]
        [SerializeField]
        private Text _messageTitle;

        [Tooltip("Text field for the message content")]
        [SerializeField]
        private Text _messageContent;

        [Tooltip("Animation time in seconds to fade a message")]
        [SerializeField]
        private float _fadeTime = 1f;

        [Tooltip("Current message being displayed")]
        [SerializeField]
        private SLAMUIMessageType _currentMessage;

        private SlamMessage _slamMessage;
        private Dictionary<SLAMUIMessageType, SlamMessage> _slamUImessages;

        private Color _initialTitleColor;
        private Color _initialContentColor;

        /// <summary>
        /// Current message being displayed
        /// </summary>
        public override SLAMUIMessageType CurrentMessage
        {
            get { return _currentMessage; }
            set
            {
                if (_currentMessage != value)
                {
                    _currentMessage = value;
                    StartCoroutine(ChangeMessage());
                }
            }
        }

        private void Awake()
        {
            InitMessages();

            _initialTitleColor = _messageTitle.color;
            _initialContentColor = _messageContent.color;
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(ChangeMessage());
            }
        }

        private void InitMessages()
        {
            _slamUImessages = new Dictionary<SLAMUIMessageType, SlamMessage>();

            _slamUImessages.Add(SLAMUIMessageType.None,
                new SlamMessage("", ""));
            _slamUImessages.Add(SLAMUIMessageType.WaitingForSensors, 
                new SlamMessage("Waiting for sensors ...", ""));
            _slamUImessages.Add(SLAMUIMessageType.WaitingForTracking,
                new SlamMessage("Initializing environment mapping", "Move your head side to side"));
            _slamUImessages.Add(SLAMUIMessageType.TurnAround,
                new SlamMessage("Turn your head", "Keep the white circle\ninside the blue marker"));
            _slamUImessages.Add(SLAMUIMessageType.HoldStill, 
                new SlamMessage("Hold your head still", ""));
            _slamUImessages.Add(SLAMUIMessageType.MappingSuccess, 
                new SlamMessage("Environment mapping complete", "", Color.green));
            _slamUImessages.Add(SLAMUIMessageType.MappingFail, 
                new SlamMessage("Unable to map the environment", "Visit metavision.com/mapping for details.\nRetrying ...", Color.red));

            _slamUImessages.Add(SLAMUIMessageType.Relocalization, 
                new SlamMessage("Relocalizing...", "Move your head side to side"));
            _slamUImessages.Add(SLAMUIMessageType.ReconstructionInstructions, 
                new SlamMessage("Reconstruction", "Move your head side to side"));
        }

        private IEnumerator ChangeMessage()
        {
            if (_slamUImessages != null)
            {
                if (_slamUImessages.TryGetValue(_currentMessage, out _slamMessage))
                {
                    // fade out
                    yield return Fade(1, 0, _fadeTime);

                    if (_messageTitle != null && _messageContent != null)
                    {
                        // set color
                        _messageTitle.color = (_slamMessage.TitleColor != null) ? _slamMessage.TitleColor.Value : _initialTitleColor;
                        _messageContent.color = (_slamMessage.ContentColor != null) ? _slamMessage.ContentColor.Value : _initialContentColor;

                        // set message
                        _messageTitle.text = _slamMessage.Title;
                        _messageContent.text = _slamMessage.Content;
                    }
                    
                    // fade in
                    yield return Fade(0, 1, _fadeTime);
                }
                else
                {
                    throw new System.Exception("Trying to access a SLAMUIMessageType key that was not inserted in the dictionary _slamUImessages.");
                }
            }
        }

        private IEnumerator Fade(float start, float end, float time)
        {
            float initialTime = Time.time;
            while (Time.time - initialTime <= time)
            {
                if (_messageTitle != null && _messageContent != null)
                {
                    float alpha = Mathf.Lerp(start, end, (Time.time - initialTime) / time);
                    _messageTitle.color = new Color(_messageTitle.color.r, _messageTitle.color.g, _messageTitle.color.b, alpha);
                    _messageContent.color = new Color(_messageContent.color.r, _messageContent.color.g, _messageContent.color.b, alpha);
                    yield return null;
                }
            }
        }
    }
}
