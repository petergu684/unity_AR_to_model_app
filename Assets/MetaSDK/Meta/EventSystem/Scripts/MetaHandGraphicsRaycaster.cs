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
using System;
using UnityEngine;
using System.Collections.Generic;
using Meta.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Meta.UI
{
    /// <summary>
    /// Performs raycaster for the hands. Many methods from this class
    /// are taken directly from the GraphicsRaycaster source code, if anything
    /// stops working in this class compare it to updates in the GraphicsRaycaster code.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class MetaHandGraphicsRaycaster : BaseRaycaster
    {
        /// <summary>
        /// Camera used for raycasting
        /// </summary>
        [SerializeField]
        private Camera _eventCamera = null;

        //TODO: Since these are private, do they need to be marked as nonserialized?
        [NonSerialized]
        private static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

        //TODO: Since these are private, do they need to be marked as nonserialized?
        [NonSerialized]
        private List<Graphic> m_RaycastResults = new List<Graphic>();

        private List<UIEventTrigger> _uiEventTriggers = new List<UIEventTrigger>();
        private EventSystem _eventSystem;
        private Canvas _canvas;

        /// <summary>
        /// 
        /// </summary>
        public EventSystem EventSystem
        {
            get { return _eventSystem; }
        }

        public override Camera eventCamera
        {
            get { return _eventCamera; }
        }

        private new void Start()
        {
            _canvas = GetComponent<Canvas>();
            _eventSystem = FindObjectOfType<EventSystem>();
        }

        protected override void OnEnable()
        {
            //Necessary to block BaseRaycaster.OnEnable
        }

        protected override void OnDisable()
        {
            //Necessary to block BaseRaycaster.OnEnable
        }

        /// <summary>
        /// Set Camera used to cast events.
        /// </summary>
        public void SetEventCamera(Camera camera)
        {
            _eventCamera = camera;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_uiEventTriggers == null || _uiEventTriggers.Count == 0)
            {
                return;
            }

            Ray ray = _eventCamera.ScreenPointToRay(eventData.position);

            m_RaycastResults.Clear();
            CollectGraphics(_canvas, _eventCamera, eventData.position, m_RaycastResults);

            for (int index = 0; index < m_RaycastResults.Count; index++)
            {
                GameObject go = m_RaycastResults[index].gameObject;
                float distance = 0;

                //Calculate if the GameObject is behind the camera.
                if (_eventCamera == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    distance = 0;
                }
                else
                {
                    Transform trans = go.transform;
                    Vector3 transForward = trans.forward;
                    // http://geomalgorithms.com/a06-_intersect-2.html
                    distance = (Vector3.Dot(transForward, trans.position - ray.origin) /
                                Vector3.Dot(transForward, ray.direction));

                    if (distance < 0)
                    {
                        continue;
                    }
                }

                RaycastResult castResult = new RaycastResult()
                {
                    gameObject = go,
                    module = this,
                    distance = distance,
                    screenPosition = eventData.position,
                    index = resultAppendList.Count,
                    depth = m_RaycastResults[index].depth,
                    sortingLayer = _canvas.sortingLayerID,
                    sortingOrder = _canvas.sortingOrder
                };

                resultAppendList.Add(castResult);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger"></param>
        public void Register(UIEventTrigger trigger)
        {
            if (!_uiEventTriggers.Contains(trigger))
            {
                _uiEventTriggers.Add(trigger);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointerEvent"></param>
        /// <param name="pressed"></param>
        /// <param name="released"></param>
        public void ProcessHandEvent(PointerEventData pointerEvent, bool pressed, bool released)
        {
            GameObject currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                if (pointerEvent.pointerEnter != currentOverGo)
                {
                    // send a pointer enter to the touched element if it isn't the one to select...
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                    pointerEvent.pointerEnter = currentOverGo;
                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                GameObject newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }

                //Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    //Debug.Log("Pressed is equal to last pressed");

                    float diffTime = time - pointerEvent.clickTime;

                    if (diffTime < 0.3f)
                    {
                        ++pointerEvent.clickCount;
                    }
                    else
                    {
                        pointerEvent.clickCount = 1;
                    }

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                {
                    //Debug.Log("Potential pointer drag start");

                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
                }
            }

            // PointerUp notification
            if (released)
            {
                //Debug.Log("Executing pressup on: " + pointerEvent.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointerEvent.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    //Debug.Log("Pointer up on same object as down");
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    //Debug.Log("Pointer drag and drop");
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    //Debug.Log("Pointer drag end while dragging");
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                }

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                if (pointerEvent.pointerDrag != null)
                {
                    //Debug.Log("Pointer drag end even though it shouldn't be?");
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                }

                pointerEvent.pointerDrag = null;

                // send exit events as we need to simulate this on touch up on touch device
                //Maybe we don't need to do this with our hands code? -Jared
                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void ProcessMove(PointerEventData eventData)
        {
            GameObject targetGO = eventData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(eventData, targetGO);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void ProcessDrag(MetaHandEventData eventData)
        {
            bool moving = eventData.IsPointerMoving();

            if (moving && eventData.pointerDrag != null
                && !eventData.dragging
                && ShouldStartDrag(eventData.pressPosition, eventData.position, EventSystem.pixelDragThreshold, eventData.useDragThreshold))
            {
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
                eventData.dragging = true;
            }

            // Drag notification
            if (eventData.dragging && moving && eventData.pointerDrag != null)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (eventData.pointerPress != eventData.pointerDrag)
                {
                    ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

                    eventData.eligibleForClick = false;
                    eventData.pointerPress = null;
                    eventData.rawPointerPress = null;
                }
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
            }
        }

        protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
        {
            // Selection tracking
            GameObject selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);

            // if we have clicked something new, deselect the old thing
            // leave 'selection handling' up to the press event though.
            if (selectHandlerGO != EventSystem.currentSelectedGameObject)
            {
                //Debug.Log("Deselect");
                EventSystem.SetSelectedGameObject(null, pointerEvent);
            }
        }

        // walk up the tree till a common root between the last entered and the current entered is found
        // send exit events up to (but not inluding) the common root. Then send enter events up to
        // (but not including the common root).
        protected void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                for (var i = 0; i < currentPointerData.hovered.Count; ++i)
                {
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);
                }

                currentPointerData.hovered.Clear();

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = newEnterTarget;

                    return;
                }
            }

            // if we have not changed hover target
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
            {
                return;
            }

            GameObject commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (currentPointerData.pointerEnter != null)
            {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                Transform t = currentPointerData.pointerEnter.transform;

                while (t != null)
                {
                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                    {
                        break;
                    }

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }

            // now issue the enter call up to but not including the common root
            currentPointerData.pointerEnter = newEnterTarget;

            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;

                while (t != null && t.gameObject != commonRoot)
                {
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);
                    t = t.parent;
                }
            }
        }

        protected GameObject FindCommonRoot(GameObject g1, GameObject g2)
        {
            if (g1 == null || g2 == null)
            {
                return null;
            }
            Transform t1 = g1.transform;
            while (t1 != null)
            {
                Transform t2 = g2.transform;
                while (t2 != null)
                {
                    if (t1 == t2)
                    {
                        return t1.gameObject;
                    }
                    t2 = t2.parent;
                }
                t1 = t1.parent;
            }
            return null;
        }

        /// <summary>
        /// Perform a raycast into the screen and collect all graphics underneath it.
        /// </summary>
        private void CollectGraphics(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, List<Graphic> results)
        {
            //Necessary for the event system
            IList<Graphic> foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            if (foundGraphics != null)
            {
                for (int i = 0; i < foundGraphics.Count; ++i)
                {
                    Graphic graphic = foundGraphics[i];

                    // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                    if (graphic.depth == -1 || !graphic.raycastTarget)
                    {
                        continue;
                    }

                    if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera))
                    {
                        continue;
                    }

                    if (graphic.Raycast(pointerPosition, eventCamera))
                    {
                        s_SortedGraphics.Add(graphic);
                    }
                }

                s_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));

                for (int i = 0; i < s_SortedGraphics.Count; ++i)
                {
                    results.Add(s_SortedGraphics[i]);
                }

                s_SortedGraphics.Clear();
            }
        }

        private bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
            {
                return true;
            }

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }
    }
}
