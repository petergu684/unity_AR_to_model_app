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

internal class SetupRenderTextures : MonoBehaviour
{
    [Range(1.0f, 4.0f)]
    [SerializeField]
    private float UpscaleAmount = 1.0f; // TODO: Check what value we need.
    [SerializeField]
    private AAValueType AAValue = AAValueType.One; // TODO: Check what value we need.
    [SerializeField]
    private Camera[] Cameras = null;
    [SerializeField]
    private GameObject[] Materials = null;

    private Resolution currentResolution;

    void Start()
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        currentResolution = new Resolution();
        currentResolution.width = Screen.width;
        currentResolution.height = Screen.height;
        SetCameraAndMaterialRenderTexture(Cameras, Materials, CreateRenderTexture());
    }

    void Update()
    {
        if (currentResolution.width != Screen.width ||
            currentResolution.height != Screen.height)
        {
            currentResolution.width = Screen.width;
            currentResolution.height = Screen.height;
            SetCameraAndMaterialRenderTexture(Cameras, Materials, CreateRenderTexture());
        }
    }

    private void SetCameraAndMaterialRenderTexture(Camera[] cameras, GameObject[] materials, RenderTexture renderTexture)
    {
        foreach (var mat in materials)
        {
            mat.GetComponent<Renderer>().material.mainTexture = renderTexture;
        }
        foreach (var cam in cameras)
        {
            // TODO: figure this out properly.  We really should be releasing those old render textures! --AHG
            //RenderTexture oldRenderTexture = cam.targetTexture;
            cam.targetTexture = renderTexture;

            // Theoretically correct, but throws exceptions.
            /*if (oldRenderTexture != null)
            {
                oldRenderTexture.ReleaseToSeat();
            }*/
        }
    }

    private RenderTexture CreateRenderTexture()
    {
        RenderTexture renderTexture = new RenderTexture(
            (int)(currentResolution.width * UpscaleAmount),
            (int)(currentResolution.height * UpscaleAmount),
            24,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);
        renderTexture.antiAliasing = (int)AAValue;
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.autoGenerateMips = false;
        renderTexture.anisoLevel = 0;
        renderTexture.Create();

        //Debug.Log(renderTexture.width + "x" + renderTexture.height);

        return renderTexture;
    }

    private enum AAValueType
    {
        One = 1,
        Two = 2,
        Four = 4,
        Eight = 8,
    }
}
