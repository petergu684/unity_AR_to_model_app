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
using UnityEngine.EventSystems;

namespace Meta.UI
{
    /// <summary>
    /// Visual cursor for a Unity canvas
    /// </summary>
	public class CanvasCursor : MonoBehaviour
	{
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private RingSegment _dot;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private RingSegment _fill;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private RingSegment _fillOutline;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private RingSegment _ring;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private RingSegment _ring2;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private Transform _crosshair;

        /// <summary>
        /// 
        /// </summary>
	    [SerializeField]
	    private CanvasPressIndicator _canvasPressIndicator;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private AnimationCurve _ringPanelProximityScale;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private AnimationCurve _ringPanelProximityAlpha;

        /// <summary>
        /// 
        /// </summary>
        private const float HoverEffectSeconds = 0.05f;
        /// <summary>
        /// 
        /// </summary>
        private const float DotUnhoveredAlpha = 0.6f;
        /// <summary>
        /// 
        /// </summary>
        private const float DotUnhoveredScale = 0.5f;
        /// <summary>
        /// 
        /// </summary>
        private const float FillAlpha = 0.3f;
        /// <summary>
        /// 
        /// </summary>
        private const float FillOutlineAlpha = 0.5f;

        private float _fullDotScale;
        private float _hoverPower;
        private float _hoverPowerVelocity;
	    private bool _isHovering;

        private void Start()
        {
            _fullDotScale = _dot.transform.localScale.x;
            Hide();
        }

        /// <summary>
        /// Handle when the hand hovers over a UI element
        /// </summary>
        /// <param name="pointerEventData"></param>
	    public void HoverBegin(PointerEventData pointerEventData)
	    {
	        if (pointerEventData is MetaHandEventData)
	        {
                _isHovering = true;
                _crosshair.gameObject.SetActive(true);
                MetaHandEventData metaHandEventData = (MetaHandEventData) pointerEventData;
	            _crosshair.transform.position = metaHandEventData.ProjectedPanelPosition;
	            _crosshair.transform.localPosition = new Vector3(_crosshair.transform.localPosition.x, _crosshair.transform.localPosition.y, 0);
            }
	    }

        /// <summary>
        /// Handle when the hand continues hovering over a UI element
        /// </summary>
        /// <param name="pointerEventData"></param>
        public void HoverStay(PointerEventData pointerEventData)
        {
            if (pointerEventData is MetaHandEventData)
            {
                MetaHandEventData metaHandEventData = (MetaHandEventData)pointerEventData;
                _crosshair.transform.position = metaHandEventData.ProjectedPanelPosition;
                _crosshair.transform.localPosition = new Vector3(_crosshair.transform.localPosition.x, _crosshair.transform.localPosition.y, 0);
            }
        }

        /// <summary>
        /// Handle when the hand stops hovering over a UI element
        /// </summary>
        /// <param name="pointerEventData"></param>
        public void HoverEnd(PointerEventData pointerEventData)
	    {
	        _isHovering = false;
            _crosshair.gameObject.SetActive(false);
        }

        /// <summary>
        /// Show the cursor
        /// </summary>
	    public void Show()
	    {
            _dot.gameObject.SetActive(true);
            _fill.gameObject.SetActive(true);
            _fillOutline.gameObject.SetActive(true);
            _ring.gameObject.SetActive(true);
            _ring2.gameObject.SetActive(true);
            _crosshair.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the cursor
        /// </summary>
	    public void Hide()
	    {
            _dot.gameObject.SetActive(false);
            _fill.gameObject.SetActive(false);
            _fillOutline.gameObject.SetActive(false);
            _ring.gameObject.SetActive(false);
            _ring2.gameObject.SetActive(false);
            _crosshair.gameObject.SetActive(false);
        }

        /// <summary>
        /// Update the cursor's position and visual state
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="pressState"></param>
        public void UpdateCursor(MetaHandEventData eventData, PressState pressState)
        {
            float ringScale = _ringPanelProximityScale.Evaluate(eventData.DownDistance);
            _ring.transform.position = eventData.ProjectedPanelPosition;
            _ring.transform.localPosition = new Vector3( _ring.transform.localPosition.x, _ring.transform.localPosition.y, 0);
            _ring.transform.localScale = Vector3.one * ringScale;
            _ring.Alpha = _ringPanelProximityAlpha.Evaluate(eventData.DownDistance);

            _ring2.transform.position = eventData.ProjectedPanelPosition;
            _ring2.transform.localPosition = new Vector3(_ring2.transform.localPosition.x, _ring2.transform.localPosition.y, 0);
            _ring2.transform.localScale = Vector3.one * (ringScale + _ring2.Thickness);
            _ring2.Alpha = _ring.Alpha;

            _fill.transform.position = eventData.ProjectedPanelPosition;
            _fill.transform.localPosition = new Vector3(_fill.transform.localPosition.x, _fill.transform.localPosition.y, 0);
            _fill.Alpha = eventData.DownDistance < 0 ? _ring.Alpha * FillAlpha : FillAlpha;
            _fillOutline.transform.position = eventData.ProjectedPanelPosition;
            _fillOutline.Alpha = eventData.DownDistance < 0 ? _ring.Alpha * FillOutlineAlpha : FillOutlineAlpha;
            
            _hoverPower = Mathf.SmoothDamp(_hoverPower, (_isHovering ? 1 : 0), ref _hoverPowerVelocity, HoverEffectSeconds);
            _dot.transform.position = eventData.ProjectedPanelPosition;
            _dot.transform.localPosition = new Vector3(_dot.transform.localPosition.x, _dot.transform.localPosition.y, 0);
            _dot.transform.localScale = Vector3.one * (_fullDotScale * Mathf.Lerp(DotUnhoveredScale, 1, _hoverPower));
            float dotAlpha = Mathf.Lerp(DotUnhoveredAlpha, 1, _hoverPower);
            _dot.Alpha = eventData.DownDistance < 0 ? _ring.Alpha * dotAlpha : dotAlpha;

            if (_isHovering && pressState == PressState.Pressed)
            {
                _canvasPressIndicator.PlayAnimation(eventData.ProjectedPanelPosition);
            }
        }
    }
}
