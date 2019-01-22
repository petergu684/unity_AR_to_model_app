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
    /// <summary>
    /// Wraps Unity-related UI and provides an interface to change the messages of the UI. 
    /// </summary>
    public class MetaSensorUiController
    {
        public const string SensorFailurePrefabName = "Prefabs/SensorFailureUi";
        private MetaSensorMessageController _controller;
        private MetaManager _manager;

        private string _majorMessage = string.Empty;
        private string _minorMessage = string.Empty;

        /// <summary>
        /// Constructs the instance and sets the UI as a sibling of the transform passed.
        /// </summary>
        /// <param name="parent"></param>
        public MetaSensorUiController()
        {
            _controller = CreateMessageUi();
            _manager = GameObject.FindObjectOfType<MetaManager>();
            _controller.FadeToAlphaOverSeconds(0f, 0.01f);
            _controller.gameObject.SetActive(false);
        }

        public bool IsVisible()
        {
            return !string.IsNullOrEmpty(_majorMessage) || !string.IsNullOrEmpty(_minorMessage);
        }

        /// <summary>
        /// Changes the sensor message
        /// </summary>
        /// <param name="message"></param>
        public void ChangeMessage(string message)
        {
            if (message == null)
            {
                return;
            }

            _majorMessage = message;
            UpdateMessage();
        }

        /// <summary>
        /// Changes the minor sensor message
        /// </summary>
        /// <param name="message"></param>
        public void ChangeMinorMessage(string message)
        {
            if (message == null)
            {
                return;
            }

            _minorMessage = message;
            UpdateMessage();
        }


        /// <summary>
        /// Gets the message: a concatenation of the major and minor messages.
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            return UpdateMessage();
        }

        private string UpdateMessage()
        {
            if (string.IsNullOrEmpty(_majorMessage) && string.IsNullOrEmpty(_minorMessage))
            {
                _controller.ChangeMessage(string.Empty);
                SetVisibility(false);
                return string.Empty;
            }

            SetVisibility(true);
            string messageConcat = _majorMessage;
            if (!string.IsNullOrEmpty(_majorMessage) && !string.IsNullOrEmpty(_minorMessage))
            {
                messageConcat += "\n\n";
            }

            messageConcat += _minorMessage;
            _controller.ChangeMessage(messageConcat);
            return messageConcat;
        }

        /// <summary>
        /// Creates the Message UI.
        /// </summary>
        /// <returns></returns>
        private MetaSensorMessageController CreateMessageUi()
        {
            GameObject ui = (GameObject)GameObject.Instantiate(Resources.Load(SensorFailurePrefabName));
            ui.hideFlags = HideFlags.HideAndDontSave;
            return ui.GetComponent<MetaSensorMessageController>();
        }

        /// <summary>
        /// Sets the visibility of the game object with a smooth fade in/out.
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetVisibility(bool isVisible)
        {
            if (isVisible)
            {
                _controller.gameObject.SetActive(true);
                _controller.FadeToAlphaOverSeconds(1f, 0.5f);
            }
            else
            {
                _controller.FadeToAlphaOverSeconds(0f, 0.5f);
                _manager.StartCoroutine(SetVisibleAfterSeconds(false, 0.5f));
            }

        }

        /// <summary>
        /// Sets the visibility of the title
        /// </summary>
        /// <param name="isVisible">Whether the title should be visible.</param>
        public void SetTitleVisibility(bool isVisible)
        {
            _controller.SetTitleVisibility(isVisible);
        }

        private IEnumerator SetVisibleAfterSeconds(bool isVisible, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _controller.gameObject.SetActive(isVisible);
        }

        /// <summary>
        /// Releases resources and cleans the instance.
        /// </summary>
        public void Destroy()
        {
            GameObject.DestroyImmediate(_controller.gameObject);
        }

    }
}


