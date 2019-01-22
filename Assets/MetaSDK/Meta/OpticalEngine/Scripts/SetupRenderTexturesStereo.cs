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

namespace Meta
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class SetupRenderTexturesStereo : MonoBehaviour
    {
        [Header("Rendering")]
        //[SerializeField]
        //[Range(1.0f, 2.0f)]
        //private float UpscaleAmount = 2.0f; // TODO: Check what value we need. 2.0f seems good.
        [SerializeField]
        private AAValueType AAValue = AAValueType.Four; // TODO: Check what value we need. Four seems good.
        [SerializeField]
        private Camera[] LeftCameras = null;
        [SerializeField]
        private Camera[] RightCameras = null;

        [Header("Unwarping")]
        public Shader UnwarpingShader;
        public AssignMaps MapAssigner;

        // internal settings
        private float _fov = 90f;
        private float _tilt = 15f;
        private int RenderTextureWidth = 1440;
        private int RenderTextureHeight = 1440;

        void Start()
        {
            // Disable if we don't support image effects
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                return;
            }

            _fov = GetComponent<Camera>().fieldOfView;

            // Disable the image effect if the shader can't
            // run on the users graphics card
            if (!UnwarpingShader || !UnwarpingShader.isSupported)
                enabled = false;

            CheckUnwarpingSetup();
        }

        private void SetCameraAndMaterialRenderTexture(Camera[] cameras, Material material, RenderTexture renderTexture)
        {
            float x_max = Mathf.Tan(_fov * Mathf.PI / 360.0f);
            float y_max = ((float)RenderTextureWidth / (float)RenderTextureHeight) * x_max;
            float rTilt = Mathf.Tan(_tilt * Mathf.Deg2Rad);

            material.mainTexture = renderTexture;

            material.SetFloat("x_max", x_max);
            material.SetFloat("y_max", y_max);

            material.SetFloat("_TiltSecant", Mathf.Sqrt(rTilt * rTilt + 1));
            material.SetFloat("_Tilt", rTilt);

            material.SetInt("_FlipVertical", FlipImageVertically ? 1 : -1);

            foreach (var cam in cameras)
            {
                cam.targetTexture = renderTexture;
                cam.fieldOfView = _fov;
                cam.transform.localEulerAngles = new Vector3(_tilt, 0, 0);
            }
        }

        private RenderTexture CreateRenderTexture()
        {
            RenderTexture renderTexture = new RenderTexture(
                2048,//(int)(RenderTextureWidth * UpscaleAmount),
                2048,//(int)(RenderTextureHeight * UpscaleAmount),
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default);
            renderTexture.antiAliasing = (int)AAValue;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.autoGenerateMips = false;
            renderTexture.anisoLevel = 0;
            renderTexture.Create();
            return renderTexture;
        }

        private enum AAValueType
        {
            One = 1,
            Two = 2,
            Four = 4,
            Eight = 8,
        }

        /// <summary>
        /// These checks are done every frame, to ensure that all maps are set up correctly. They might have changed for various serialization reasons.
        /// </summary>
        void CheckUnwarpingSetup()
        {
            // check that all textures and references are correctly set up.
            if (LeftCameras[0].targetTexture == null) LeftCameras[0].targetTexture = CreateRenderTexture();
            if (RightCameras[0].targetTexture == null) RightCameras[0].targetTexture = CreateRenderTexture();

            // check if our materials are set up properly
            if (leftMaterial.mainTexture == null) leftMaterial.mainTexture = LeftCameras[0].targetTexture;
            if (rightMaterial.mainTexture == null) rightMaterial.mainTexture = RightCameras[0].targetTexture;

            // apply material and camera values
            SetCameraAndMaterialRenderTexture(LeftCameras, leftMaterial, LeftCameras[0].targetTexture);
            SetCameraAndMaterialRenderTexture(RightCameras, rightMaterial, RightCameras[0].targetTexture);

            // load distortion maps if available
            MapAssigner.AssignTo(leftMaterial, rightMaterial);
        }

        Material _leftMaterial;
        protected Material leftMaterial
        {
            get
            {
                if (_leftMaterial == null)
                {
                    _leftMaterial = new Material(UnwarpingShader);
                    _leftMaterial.SetFloat("_IsLeft", 1);
                    _leftMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                return _leftMaterial;
            }
        }

        Material _rightMaterial;
        protected Material rightMaterial
        {
            get
            {
                if (_rightMaterial == null)
                {
                    _rightMaterial = new Material(UnwarpingShader);
                    _rightMaterial.SetFloat("_IsLeft", 0);
                    _rightMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                return _rightMaterial;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // check whether RenderTextures are correctly assigned
            CheckUnwarpingSetup();

            // draw quads directly to the source texture
            RenderTexture.active = source;

            // draw two fullscreen quads, currently they move themselves to the correct location on screen via shader
            // TODO change the vertices here, remove vertex movement from shader
            // TODO check that there is no RenderTexture flipping involved anywhere here
            DrawFullScreenQuad(leftMaterial);
            DrawFullScreenQuad(rightMaterial);

            // output to the destination (screen or RT)
            Graphics.Blit(source, destination);
        }

        public static void DrawFullScreenQuad(Material material)
        {
            GL.PushMatrix();
            GL.LoadOrtho();

            int i = 0;
            while (i < material.passCount) // TODO: Remove the text coords, and hard code left and right vertices.
            {
                material.SetPass(i);
                GL.Begin(GL.QUADS);
                //GL.Color(Color.white);
                GL.TexCoord2(0, 0);
                GL.Vertex3(-1, -1, 0.1f);

                GL.TexCoord2(1, 0);
                GL.Vertex3(1, -1, 0.1f);

                GL.TexCoord2(1, 1);
                GL.Vertex3(1, 1, 0.1f);

                GL.TexCoord2(0, 1);
                GL.Vertex3(-1, 1, 0.1f);
                GL.End();
                ++i;
            }

            GL.PopMatrix();
        }

        /// <summary>
        /// Clean up materials.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_leftMaterial)
            {
                DestroyImmediate(_leftMaterial);
            }
            if (_rightMaterial)
            {
                DestroyImmediate(_rightMaterial);
            }
        }

        /// <summary>
        /// Flip the image vertically
        /// </summary>
        public bool FlipImageVertically
        {
            get;
            set;
        }
    }
}
