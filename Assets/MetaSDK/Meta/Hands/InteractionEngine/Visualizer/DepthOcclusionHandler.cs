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
using System;

namespace Meta.Internal
{
    /// <summary>   A depth occlusion handler. </summary>
    /// <seealso cref="T:Meta.IEventReceiver" />
    internal class DepthOcclusionHandler : IEventReceiver
    {
        /// <summary>   The X coordinate focal length of the depth camera. </summary>
        private readonly string _depthFocalLengthXShaderProperty = "_DepthFocalLengthX";

        /// <summary>   The y coordinate focal length of the depth camera. </summary>
        private readonly string _depthFocalLengthYShaderProperty = "_DepthFocalLengthY";

        /// <summary>   The height of the depth data. </summary>
        private readonly string _depthHeightShaderProperty = "_DepthHeight";

        /// <summary>   The depth occlusion gameObject. </summary>
        private readonly GameObject _depthOcclusionGO;

        /// <summary>   The X coordinate principal point of the depth camera. </summary>
        private readonly string _depthPrincipalPointXShaderProperty = "_DepthPrincipalPointX";

        /// <summary>   The Y coordinate principal point of the depth camera. </summary>
        private readonly string _depthPrincipalPointYShaderProperty = "_DepthPrincipalPointY";

        /// <summary>   The depth shader property. </summary>
        private readonly string _depthTextureShaderProperty = "_DepthTex";

        /// <summary>   The width of the depth data. </summary>
        private readonly string _depthWidthShaderProperty = "_DepthWidth";

        /// <summary>   The current native texture handle. </summary>
        private readonly NativeDepthTextureHandle currentNativeTexture = new NativeDepthTextureHandle();

        private bool _meshesDisabled;

        /// <summary>   The meta data describing the sensor. </summary>
        private SensorMetaData _sensorMetaData;

        /// <summary>   true if texture created. </summary>
        private bool _textureCreated;

        // parameters for creating the mesh
        private uint height;

        public bool initlialzed;

        /// <summary>   The mesh objects. </summary>
        private List<GameObject> meshObjects;

        // offset for each mesh
        [SerializeField]
        public Vector3 meshOffset;

        /// <summary>   Height of the quad. </summary>
        private float quadHeight = 0;

        /// <summary>   Width of the quad. </summary>
        private float quadWidth = 0;

        /// <summary>   The quad x coordinate gap. </summary>
        private float quadXGap = 0;

        /// <summary>   The quad y coordinate gap. </summary>
        private float quadYGap = 0;

        private uint width;

        public Texture2D DepthTexture2D
        {
            get { return currentNativeTexture.tex; }
        }

        /// <summary>
        ///     Initializes a new instance of the DepthOcclusionHandler class.
        /// </summary>
        /// <param name="depthOcclusionGO"> The depth occlusion gameObject. </param>
        public DepthOcclusionHandler(GameObject depthOcclusionGO)
        {
            _depthOcclusionGO = depthOcclusionGO;
            _sensorMetaData = new SensorMetaData();
        }

        private DepthOcclusionHandler() {}

        /// <summary>   Initialises this object. </summary>
        /// <param name="eventHandlers">    The event handlers. </param>
        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnUpdate(Update);
        }

        /// <summary>  MetaBehavior Update. </summary>
        private void Update()
        {
            if (initlialzed && !_textureCreated)
            {
                currentNativeTexture.CreateTextureAndPassToPlugin((int)height, (int)width);
                InitOclussionManager();
                _textureCreated = true;
            }
            else if (!initlialzed)
            {
                if (HandKernelInterop.GetSensorMetaData(ref _sensorMetaData))
                {
                    height = (uint)_sensorMetaData.width;
                    width = (uint)_sensorMetaData.height;
                    initlialzed = true;
                }
            }
        }

        /// <summary>   Initialises the oclussion manager. </summary>
        private void InitOclussionManager()
        {
            // create a list to hold the game objects
            meshObjects = new List<GameObject>();

            // create a list to hold the meshes
            List<Mesh> meshes = new List<Mesh>();

            MeshSplitter.CreateMeshes(width, height, (int)quadWidth, (int)quadHeight, (int)quadXGap, (int)quadYGap, ref meshes);

            for (int i = 0; i < meshes.Count; ++i)
            {
                GameObject newObject = new GameObject("OcclusionMesh" + i);
                newObject.AddComponent<MeshFilter>();
                newObject.AddComponent<MeshRenderer>();
                newObject.GetComponent<MeshFilter>().mesh = meshes[i];

                newObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, Vector3.one);

                newObject.transform.SetParent(_depthOcclusionGO.transform, false);
                newObject.transform.localPosition = Vector3.zero;
                newObject.transform.localScale = Vector3.one;

                meshObjects.Add(newObject);
            }

            ApplyTexture(currentNativeTexture);
        }

        /// <summary>   Applies the texture described by currentNativeTexture. </summary>
        /// <param name="currentNativeTexture"> The current native texture handle. </param>
        private void ApplyTexture(NativeDepthTextureHandle currentNativeTexture)
        {
            _depthOcclusionGO.GetComponent<Renderer>().material.SetTexture(_depthTextureShaderProperty, currentNativeTexture.tex);
            _depthOcclusionGO.GetComponent<Renderer>().material.SetFloat(_depthHeightShaderProperty, _sensorMetaData.height);
            _depthOcclusionGO.GetComponent<Renderer>().material.SetFloat(_depthWidthShaderProperty, _sensorMetaData.width);
            _depthOcclusionGO.GetComponent<Renderer>().material.SetFloat(_depthFocalLengthXShaderProperty, _sensorMetaData.focalLengthX);
            _depthOcclusionGO.GetComponent<Renderer>().material.SetFloat(_depthFocalLengthYShaderProperty, _sensorMetaData.focalLengthY);
            _depthOcclusionGO.GetComponent<Renderer>().material.SetFloat(_depthPrincipalPointXShaderProperty, _sensorMetaData.principalPointX);
            _depthOcclusionGO.GetComponent<Renderer>().material.SetFloat(_depthPrincipalPointYShaderProperty, _sensorMetaData.principalPointY);
            _depthOcclusionGO.GetComponent<MeshRenderer>().material.mainTexture = currentNativeTexture.tex;
            for (int i = 0; i < meshObjects.Count; ++i)
            {
                meshObjects[i].GetComponent<MeshRenderer>().material = _depthOcclusionGO.GetComponent<Renderer>().material;
                meshObjects[i].GetComponent<MeshRenderer>().enabled = false;
            }
            _meshesDisabled = true;
        }

        /// <summary>   Call plugin at end of frames. </summary>
        /// <returns>   An IEnumerator. </returns>
        internal IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();
                if (initlialzed && _textureCreated)
                {
                    // Set time for the plugin
                    HandKernelInterop.SetTimeFromUnity(Time.timeSinceLevelLoad);

                    // Issue a plugin event with arbitrary integer identifier.
                    // The plugin can distinguish between different
                    // things it needs to do based on this ID.
                    // For our simple plugin, it does not matter which ID we pass here.
                    GL.IssuePluginEvent(HandKernelInterop.GetRenderEventFunc(), 1);

                    if (_meshesDisabled)
                    {
                        EnableMeshObjects();
                        _meshesDisabled = false;
                    }
                }
            }
        }

        private void EnableMeshObjects()
        {
            for (int i = 0; i < meshObjects.Count; ++i)
            {
                meshObjects[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
}
