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
using System.IO;

namespace Meta.Mouse
{
    internal class PlaybackInputWrapper : IInputWrapper
    {
        private int _framesRead;
        private int _totalFrames;
        private float _mouseSensitivity;
        private Vector2 _mouseScrollDelta = new Vector2();
        private float _mouseXAxis;
        private float _mouseYAxis;
        private bool _getMouseButton;
        private bool _getMouseButtonUp;
        private bool _getMouseButtonDown;
        private float _screenHeight;
        private float _screenWidth;
        private BinaryReader _reader;

        public int framesRead
        {
            get { return _framesRead; }
        }

        public int numFrames
        {
            get { return _totalFrames; }
        }

        public float MouseSensitivity
        {
            get { return _mouseSensitivity; }
        }

        public PlaybackInputWrapper(BinaryReader reader)
        {
            _reader = reader;
            _totalFrames = _reader.ReadInt32();
            _mouseSensitivity = _reader.ReadSingle();
            ReadFrame();
        }

        public void ReadFrame()
        {
            _screenWidth = _reader.ReadSingle();
            _screenHeight = _reader.ReadSingle();
            Vector2 screenSizeRatio = new Vector2(Screen.width / _screenWidth, Screen.height / _screenHeight);
            _mouseXAxis = _reader.ReadSingle() * screenSizeRatio.x;
            _mouseYAxis = _reader.ReadSingle() * screenSizeRatio.y;
            _getMouseButton = _reader.ReadBoolean();
            _getMouseButtonUp = _reader.ReadBoolean();
            _getMouseButtonDown = _reader.ReadBoolean();
            _framesRead++;
        }

        public CursorLockMode LockState { get; set; }
        public bool Visible { get; set; }

        public Vector3 GetMousePosition()
        {
            // TODO: this should almost CERTAINLY be 
            // return _mousePosition;
            //#warning returning _mouseScrollDelta instead of _mousePosition
            return _mouseScrollDelta;
        }

        public Vector2 GetMouseScrollDelta()
        {
            return Input.mouseScrollDelta;
        }

        public float GetAxis(string axisName)
        {
            if (axisName == "Mouse X")
            {
                return _mouseXAxis;
            }
            else if (axisName == "Mouse Y")
            {
                return _mouseYAxis;
            }
            return 0f;
        }

        public bool GetMouseButton(int button)
        {
            return _getMouseButton;
        }

        public bool GetMouseButtonUp(int button)
        {
            return _getMouseButtonUp;
        }

        public bool GetMouseButtonDown(int button)
        {
            return _getMouseButtonDown;
        }

        public Rect GetScreenRect()
        {
            return new Rect(0, 0, _screenWidth, _screenHeight);
        }

        public void CloseFile()
        {
            _reader.Close();
        }
    }
}
