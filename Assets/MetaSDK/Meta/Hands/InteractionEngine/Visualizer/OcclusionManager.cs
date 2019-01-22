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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Meta.Internal
{
    /// <summary>   Manager for occlusions. </summary>
    /// <seealso cref="T:UnityEngine.MonoBehaviour" />
    public class OcclusionManager : MonoBehaviour
    {
        private readonly string _depthShaderProperty = "_DepthTex";

        private readonly NativeDepthTextureHandle depthPointCloudTexture = new NativeDepthTextureHandle();

        private readonly NativeDepthTextureHandle pmdPointCloudTexture = new NativeDepthTextureHandle();

        private NativeDepthTextureHandle currentNativeTexture;

        public uint height;

        private List<GameObject> meshObjects;

        // offset for each mesh
        [SerializeField]
        public Vector3 meshOffset;

        public float quadHeight;

        public float quadWidth;

        public float quadXGap;

        public float quadYGap;

        public bool usePMD;

        // parameters for creating the mesh
        public uint width;

        // Native plugin rendering events are only called if a plugin is used
        // by some script. This means we have to DllImport at least
        // one function in some active script.
        // For this example, we'll call into plugin's SetTimeFromUnity
        // function and pass the current time so the plugin can animate.
        [DllImport("DepthVisualizerPlugin", EntryPoint = "SetTimeFromUnity")]
        private static extern void SetTimeFromUnity(float t);

        [DllImport("DepthVisualizerPlugin", EntryPoint = "SetUnityStreamingAssetsPath")]
        private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("DepthVisualizerPlugin", EntryPoint = "GetRenderEventFunc")]
        private static extern IntPtr GetRenderEventFunc();

        private IEnumerator Start()
        {
            SetUnityStreamingAssetsPath(Application.streamingAssetsPath);

            int textureHeight, textureWidth;

            //if (Meta.Internal.HandPhysics.CppMetaPhysicsManager.Instance.usePMD)      // Disabled for use with prototyping physics simulation
            if (usePMD)
            {
                currentNativeTexture = pmdPointCloudTexture;
                textureHeight = 224;
                textureWidth = 171;
            }
            else
            {
                currentNativeTexture = depthPointCloudTexture;
                textureHeight = 320;
                textureWidth = 240;
            }
            currentNativeTexture.CreateTextureAndPassToPlugin(textureHeight, textureWidth);

            width = (uint)textureHeight;
            height = (uint)textureWidth;

            // create a list to hold the game objects
            meshObjects = new List<GameObject>();

            // attach the meshes to the game objects.
            CreateGameObjects();

            ApplyTexture(currentNativeTexture);

            yield return StartCoroutine("CallPluginAtEndOfFrames");
        }

        /// <summary>
        ///     Create game objects and attach meshes to them.
        /// </summary>
        /// <param name="numMeshes">The number of meshes there are.</param>
        /// <param name="meshes">The list containing the meshes.</param>
        private void CreateGameObjects()
        {
            // create a list to hold the meshes
            List<Mesh> meshes = new List<Mesh>();

            MeshSplitter.CreateMeshes(width, height, (int)quadWidth, (int)quadHeight, (int)quadXGap, (int)quadYGap, ref meshes);

            for (int i = 0; i < meshes.Count; ++i)
            {
                GameObject newObject = new GameObject("OcclusionMesh" + i);
                newObject.AddComponent<MeshFilter>();
                newObject.AddComponent<MeshRenderer>();
                newObject.GetComponent<MeshFilter>().mesh = meshes[i];

                newObject.transform.SetParent(gameObject.transform, true);
                newObject.transform.localPosition = Vector3.zero;
                newObject.transform.localScale = Vector3.one;

                meshObjects.Add(newObject);
            }
        }

        /// <summary>
        /// </summary>
        private void ApplyTexture(NativeDepthTextureHandle currentNativeTexture)
        {
            GetComponent<Renderer>().material.SetTexture(_depthShaderProperty, currentNativeTexture.tex);
            GetComponent<MeshRenderer>().material.mainTexture = currentNativeTexture.tex;

            for (int i = 0; i < meshObjects.Count; ++i)
            {
                meshObjects[i].GetComponent<MeshRenderer>().material = GetComponent<Renderer>().material;
            }
        }

        private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();

                // Set time for the plugin
                SetTimeFromUnity(Time.timeSinceLevelLoad);

                // Issue a plugin event with arbitrary integer identifier.
                // The plugin can distinguish between different
                // things it needs to do based on this ID.
                // For our simple plugin, it does not matter which ID we pass here.
                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }
    }
}
