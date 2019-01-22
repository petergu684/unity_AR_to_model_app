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
using System.Collections.Generic;
using UnityEngine;

namespace Meta.EditorUtils
{
    /// <summary>
    /// Handles in a better way the colors of the GUI
    /// </summary>
    public class ColorStack
    {
        private Stack<Color> _mainColorStack;
        private Stack<Color> _backgroundColorStack;
        private Stack<Color> _contentColorStack;
        private bool _defaults = false;

        /// <summary>
        /// Create a new instance of this object
        /// </summary>
        public ColorStack()
        {
            _mainColorStack = new Stack<Color>();
            _backgroundColorStack = new Stack<Color>();
            _contentColorStack = new Stack<Color>();
        }

        /// <summary>
        /// Collect the default values of the GUI.
        /// This happens onle once, further calls will do nothing.
        /// </summary>
        public void CollectDefaults()
        {
            if (_defaults)
            {
                return;
            }

            _mainColorStack.Push(GUI.color);
            _backgroundColorStack.Push(GUI.backgroundColor);
            _contentColorStack.Push(GUI.contentColor);
            _defaults = true;
        }

        #region Content Color
        /// <summary>
        /// Push the given color to the Content Stack.
        /// This will update the GUI.contentColor to the given value
        /// </summary>
        /// <param name="color">Color of Content</param>
        public void PushContent(Color color)
        {
            _contentColorStack.Push(color);
            GUI.contentColor = color;
        }

        /// <summary>
        /// Pop the color of the Content Stack.
        /// This will update the GUI.contentColor to the previous value.
        /// </summary>
        /// <returns>Color Popped</returns>
        public Color PopContent()
        {
            if (_contentColorStack.Count <= 0)
            {
                return Color.white;
            }
            var color = _contentColorStack.Pop();
            GUI.contentColor = _contentColorStack.Peek();
            return color;
        }
        #endregion

        #region Background Color
        /// <summary>
        /// Push the given color to the Background Stack.
        /// This will update the GUI.backgroundColor to the given value
        /// </summary>
        /// <param name="color">Color of Background</param>
        public void PushBackground(Color color)
        {
            _backgroundColorStack.Push(color);
            GUI.backgroundColor = color;
        }

        /// <summary>
        /// Pop the color of the Background Stack.
        /// This will update the GUI.backgroundColor to the previous value.
        /// </summary>
        /// <returns>Color Popped</returns>
        public Color PopBackground()
        {
            if (_backgroundColorStack.Count <= 0)
            {
                return Color.white;
            }
            var color = _backgroundColorStack.Pop();
            GUI.backgroundColor = _backgroundColorStack.Peek();
            return color;
        }
        #endregion

        #region Main Stack
        /// <summary>
        /// Push the given color to the Main Stack.
        /// This will update the GUI.color to the given value
        /// </summary>
        /// <param name="color"></param>
        public void Push(Color color)
        {
            _mainColorStack.Push(color);
            GUI.color = color;
        }

        /// <summary>
        /// Pop the color of the Main Stack.
        /// This will update the GUI.color to the previous value.
        /// </summary>
        /// <returns>Color Popped</returns>
        public Color Pop()
        {
            if (_mainColorStack.Count <= 0)
            {
                return Color.white;
            }
            var color = _mainColorStack.Pop();
            GUI.color = _mainColorStack.Peek();
            return color;
        }
        #endregion

        /// <summary>
        /// Gets the current main color
        /// </summary>
        public Color CurrentColor
        {
            get { return _mainColorStack.Peek(); }
        }

        /// <summary>
        /// Gets the current Background color
        /// </summary>
        public Color CurrentBackgroundColor
        {
            get { return _backgroundColorStack.Peek(); }
        }

        /// <summary>
        /// Gets the current Content color
        /// </summary>
        public Color CurrentContentColor
        {
            get { return _contentColorStack.Peek(); }
        }
    }
}
