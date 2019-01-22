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
using System;

namespace Meta.Mouse
{
    /// <summary>
    /// Abstraction of Unity Gameview.
    /// </summary>
    internal class RuntimeGameView
    {
        public event Action PointerEnter;
        public event Action PointerExit;
        public event Action PointerHover;
        public event Action PointerExternal;
        private const int ScreenPadding = 4;
        private readonly IPlatformMouse _platformMouse;
        private readonly IInputWrapper _inputWrapper;
        private bool _priorPointerInGameView;
        private bool _priorPointExternalGameView;
        private Rect _globalGameViewRect = new Rect();

        /// <summary>
        /// The rect of the RuntimeGameView in global, system, coordinates.
        /// </summary>
        public Rect GlobalGameViewRect
        {
            get { return _globalGameViewRect; }
        }

        public RuntimeGameView(IInputWrapper inputWrapper)
        {
            _inputWrapper = inputWrapper;
            _platformMouse = PlatformMouseFactory.GetPlatformMouse();
            _priorPointerInGameView = true;
            //Must be locked initially because CheckVisibility relies on cursor being inside
            //game view intiaily to calculate proper globalGameViewRect.
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Runs logic to determine pointer enter, exit and hover events.
        /// </summary>
        public void ProcessEvents(Vector2 pointerPosition)
        {
            bool currentCursorInGameView = false;
            if (_priorPointerInGameView)
            {
                CalculateGlobalGameViewRect();
                Rect screenRect = _inputWrapper.GetScreenRect();
                screenRect.xMin += ScreenPadding;
                screenRect.xMax -= ScreenPadding;
                screenRect.yMin += ScreenPadding;
                screenRect.yMax -= ScreenPadding;
                currentCursorInGameView = screenRect.Contains(pointerPosition);
            }
            else
            {
                currentCursorInGameView = PointerInGlobalGameView(ScreenPadding * 2);
            }
            if (currentCursorInGameView && !_priorPointerInGameView && _priorPointExternalGameView)
            {
                if (PointerEnter != null)
                {
                    PointerEnter();
                }
                _priorPointerInGameView = true;
                _priorPointExternalGameView = false;
            }
            else if (currentCursorInGameView && _priorPointerInGameView)
            {
                if (PointerHover != null)
                {
                    PointerHover();
                }
            }
            else if (!currentCursorInGameView && _priorPointerInGameView)
            {
                if (PointerExit != null)
                {
                    PointerExit();
                }
                _priorPointerInGameView = false;
            }
            else if (!currentCursorInGameView && !_priorPointerInGameView)
            {
                if (PointerExternal != null)
                {
                    PointerExternal();
                }
                _priorPointExternalGameView = true;
            }
        }

        /// <summary>
        /// Set the system pointer position relative to the RuntimeGameView.
        /// </summary>
        public void SetGlobalPointerPosRelativeGameView(Vector2 position)
        {
            position.x += _globalGameViewRect.x;
            position.y = (_globalGameViewRect.height - position.y) + _globalGameViewRect.y;
            _platformMouse.SetGlobalCursorPos(position);
        }

        /// <summary>
        /// Get the pointer position relative to the RuntimeGameView.
        /// </summary>
        public Vector2 GetGlobalPointerPosRelativeGameView()
        {
            Vector2 pos = new Vector2();
            Vector2 globalPosition = _platformMouse.GetGlobalCursorPos();
            pos.x = globalPosition.x - _globalGameViewRect.x;
            pos.y = _globalGameViewRect.height - (globalPosition.y - _globalGameViewRect.y);
            return pos;
        }

        /// <summary>
        /// Determines whether or not pointer is in RuntimeGameView.
        /// </summary>
        private bool PointerInGlobalGameView(float padding)
        {
            Rect globalScreenRect = _globalGameViewRect;
            globalScreenRect.xMin += padding;
            globalScreenRect.xMax -= padding;
            globalScreenRect.yMin += padding;
            globalScreenRect.yMax -= padding;
            return globalScreenRect.Contains(_platformMouse.GetGlobalCursorPos());
        }

        /// <summary>
        /// Calculates the global game view rect.
        /// </summary>
        /// <remarks>
        /// This works on the assumption that GetGlobalCursorPos and _inputWrapper.GetMousePosition()
        /// are on the exact same point on the screen, just one in global space and the other in local.
        /// Thus _globalScreenRect must be stored as a class variable as it can only be calculated when
        /// the point is inside the scene view. This does mean that if the pointer never enters the scene
        /// view then it cannot properly calculate, this is alleviated by starting the scene with the cursor
        /// locked which will initially force the pointer to the center of the scene view.
        /// </remarks>
        private void CalculateGlobalGameViewRect()
        {
            Vector2 globalPosition = _platformMouse.GetGlobalCursorPos();
            Vector2 localPosition = _inputWrapper.GetMousePosition();
            _globalGameViewRect.x = globalPosition.x - localPosition.x;
            _globalGameViewRect.y = globalPosition.y - (_inputWrapper.GetScreenRect().height - localPosition.y);
            _globalGameViewRect.width = _inputWrapper.GetScreenRect().width;
            _globalGameViewRect.height = _inputWrapper.GetScreenRect().height;
        }
    }
}
