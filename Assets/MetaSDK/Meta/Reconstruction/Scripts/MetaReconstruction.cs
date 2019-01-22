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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Meta.Reconstruction;

namespace Meta
{
    /// <summary>
    /// Encapsulates the Meta 3D reconstruction functionality.
    /// Adds game object to the scene containing the 3D reconstruction.
    /// </summary>
    internal class MetaReconstruction : MetaBehaviourInternal, IMetaReconstruction
    {
        private enum ReconstructionState
        {
            None,
            Initializing,
            Reinitializing,
            Scanning,
            Loading
        }

        #region Events
        [Tooltip("Occurs after the reconstruction process is initialized.")]
        [HideInInspector]
        [SerializeField]
        private UnityEvent _reconstructionStarted = null;

        [Tooltip("Occurs when the reconstruction process is paused.")]
        [HideInInspector]
        [SerializeField]
        private UnityEvent _reconstructionPaused = null;

        [Tooltip("Occurs when the reconstruction process is resumed.")]
        [HideInInspector]
        [SerializeField]
        private UnityEvent _reconstructionResumed = null;

        [Tooltip("Occurs after the reconstruction is reset.")]
        [HideInInspector]
        [SerializeField]
        private UnityEvent _reconstructionReset = null;

        [Tooltip("Occurs after all the meshes are saved on disk.")]
        [HideInInspector]
        [SerializeField]
        private UnityEvent _reconstructionSaved = null;

        [Tooltip("Occurs after all saved meshes are loaded on the scene. Returns the parent GameObject of all the reconstruction meshes.")]
        [HideInInspector]
        [SerializeField]
        private GameObjectEvent _reconstructionLoaded = null;
        #endregion

        #region Variables
        [Tooltip("Material to use for the reconstruction, only actively set when we are creating the reconstruction initially.")]
        [HideInInspector]
        [SerializeField]
        private Material _scanningMaterial;

        [Tooltip("Material used to occlude the reconstruction meshes.")]
        [HideInInspector]
        [SerializeField]
        private Material _occlusionMaterial;

        // state of the reconstruction system
        private ReconstructionState _currentState = ReconstructionState.None;

        // pause reconstruction
        private bool _pause = true;
        private bool _loadFinished = false;
        private bool _saveFinished = false;

        // Parent of the reconstruction meshes
        private GameObject _reconstruction;

        // Parent of the loaded reconstruction meshes
        private GameObject _loadedReconstruction;

        // Current mesh from interop
        private Mesh _currentMesh;

        private IMeshGenerator _meshGenerator;
        private IModelFileManipulator _modelFileManipulator;

        // Path for the folder that the meshes are saved
        private IEnvironmentProfileRepository _environmentProfileRepository;
        #endregion

        #region Properties
        /// <summary>
        /// Occurs after the reconstruction process is initialized.
        /// </summary>
        public UnityEvent ReconstructionStarted
        {
            get { return _reconstructionStarted; }
        }

        /// <summary>
        /// Occurs when the reconstruction process is paused.
        /// </summary>
        public UnityEvent ReconstructionPaused
        {
            get { return _reconstructionPaused; }
        }

        /// <summary>
        /// Occurs when the reconstruction process is resumed.
        /// </summary>
        public UnityEvent ReconstructionResumed
        {
            get { return _reconstructionResumed; }
        }

        /// <summary>
        /// Occurs after the reconstruction is reset.
        /// </summary>
        public UnityEvent ReconstructionReset
        {
            get { return _reconstructionReset; }
        }

        /// <summary>
        /// Occurs after all the meshes are saved on disk.
        /// </summary>
        public UnityEvent ReconstructionSaved
        {
            get { return _reconstructionSaved; }
        }

        /// <summary>
        /// Occurs after all saved meshes are loaded on the scene. Returns the parent GameObject of all the reconstruction meshes.
        /// </summary>
        public GameObjectEvent ReconstructionLoaded
        {
            get { return _reconstructionLoaded; }
        }

        /// <summary>
        /// Returns the parent GameObject of all the reconstruction meshes.
        /// </summary>
        public GameObject ReconstructionRoot
        {
            get { return _reconstruction; }
        }

        /// <summary>
        /// Returns the parent GameObject of all the loaded reconstruction meshes.
        /// </summary>
        public GameObject LoadedReconstructionRoot
        {
            get { return _loadedReconstruction; }
        }

        /// <summary>
        /// Gets or sets the material to use for the reconstruction, only actively set when we are creating the reconstruction initially.
        /// </summary>
        public Material ScanningMaterial
        {
            get { return _scanningMaterial; }
            set
            {
                _scanningMaterial = value;
                _meshGenerator.Material = _scanningMaterial;
            }
        }

        /// <summary>
        /// Gets or sets the material used to occlude the reconstruction meshes
        /// </summary>
        public Material OcclusionMaterial
        {
            get { return _occlusionMaterial; }
            set { _occlusionMaterial = value; }
        }
        #endregion

        #region Unity Events

        private void Awake()
        {
            if (!metaContext.ContainsModule<IMetaReconstruction>())
            {
                metaContext.Add<IMetaReconstruction>(this);
            }
        }

        private void Start()
        {
            _environmentProfileRepository = metaContext.Get<IEnvironmentProfileRepository>();
            Validate();

            _meshGenerator = metaContext.Get<IMeshGenerator>();
            _meshGenerator.Material = _scanningMaterial;
            _modelFileManipulator = metaContext.Get<IModelFileManipulator>();
            SetState(ReconstructionState.Initializing);
        }

        private void Update()
        {
            if (_currentState == ReconstructionState.Scanning && !_pause)
            {
                ReconstructMesh();
            }
        }
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Change the material of the reconstruction meshes
        /// </summary>
        /// <param name="material">Material</param>
        public void ChangeReconstructionMaterial(Material material)
        {
            SetMaterialsInChildren(_reconstruction, material);
        }

        /// <summary>
        /// Change the material of the reconstruction meshes loaded from file
        /// </summary>
        /// <param name="material">Material</param>
        public void ChangeLoadedReconstructionMaterial(Material material)
        {
            SetMaterialsInChildren(_loadedReconstruction, material);
        }

        /// <summary>
        /// Initializes the reconstruction process.
        /// </summary>
        public void InitReconstruction()
        {
            switch (_currentState)
            {
                case ReconstructionState.Initializing:
                    InitReconstructionForFirstTime();
                    break;
                case ReconstructionState.Reinitializing:
                    Reinitialize();
                    break;
                default:
                    Debug.LogWarning("The reconstruction process is already initialized.");
                    break;
            }
        }

        /// <summary>
        /// Restart the reconstruction process.
        /// </summary>
        public void RestartReconstruction()
        {
            //TODO RECONSTRUCTION
            //Reset is not currently supported. It was crashing Unity the last time we tested it, if we pause it and reset in the same frame.
        }

        /// <summary>
        /// Toggles on and off the reconstruction process.
        /// </summary>
        public void PauseResumeReconstruction()
        {
            if (_currentState == ReconstructionState.Scanning)
            {
                ReconstructionInterop.PauseReconstruction();
                _pause = !_pause;

                if (_pause)
                {
                    _reconstructionPaused.Invoke();
                    UpdateReconstructionMeshes();
                }
                else
                {
                    _reconstructionResumed.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("The reconstruction process is not currently initialized.");
            }
        }

        /// <summary>
        /// Resets the reconstruction mesh.
        /// </summary>
        public void ResetReconstruction()
        {
            if (_currentState == ReconstructionState.Scanning && _pause)
            {
                _meshGenerator.ResetMeshes();
                ReconstructionInterop.ResetReconstruction();
                _reconstructionReset.Invoke();
            }
            else
            {
                Debug.LogWarning("Reconstruction can only be reset if it is initialized and paused.");
            }
        }

        /// <summary>
        /// Delete meshes related to the given map.
        /// </summary>
        /// <param name="profileName">The slam map name</param>
        /// <param name="saveChangesInProfile">Whether to save changes in profile or not</param>
        public void DeleteReconstructionMeshFiles(string environmentProfileName, bool saveChangesInProfile = true)
        {
            Validate();
            IEnvironmentProfile environmentProfile = GetEnvironmentByName(environmentProfileName);
            for (int i = 0; i < environmentProfile.Meshes.Count; i++)
            {
                string meshPath = environmentProfile.Meshes[i];
                if (File.Exists(meshPath))
                {
                    File.Delete(meshPath);
                }
            }

            _environmentProfileRepository.SetMeshes(environmentProfile.Id, new List<string>());
            if (saveChangesInProfile)
            {
                StartCoroutine(SaveReconstructionChangesInProfile());
            }
        }

        /// <summary>
        /// Loads the reconstruction for the given map or the one currently active.
        /// <param name="profileName">The slam map name</param>
        /// </summary>
        public void LoadReconstruction(string environmentProfileName = null)
        {
            SetState(ReconstructionState.Loading);
            StartCoroutine(LoadReconstructionCoroutine(environmentProfileName));
        }

        /// <summary>
        /// Save the current scanned reconstruction in .obj files
        /// <param name="environmentProfileName">The environment profile name</param>
        /// <param name="saveChangesInProfile">Whether to save changes in profile or not</param>
        /// </summary>
        public void SaveReconstruction(string environmentProfileName = null, bool saveChangesInProfile = true)
        {
            StartCoroutine(SaveReconstructionCoroutine(environmentProfileName, saveChangesInProfile));
        }

        /// <summary>
        /// Stops the reconstruction process.
        /// </summary>
        public void StopReconstruction()
        {
            if (!_pause)
            {
                PauseResumeReconstruction();
            }
        }

        /// <summary>
        /// Cleans the current environment meshes.
        /// </summary>
        public void CleanMeshes()
        {
            switch (_currentState)
            {
                case ReconstructionState.Scanning:
                    StartCoroutine(CleanScannedCoroutine());
                    break;
                case ReconstructionState.Loading:
                    CleanLoadedCoroutine();
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void Scan()
        {
            SetState(ReconstructionState.Scanning);
        }

        private void SetState(ReconstructionState state)
        {
            _currentState = state;
        }

        private void SetMaterialsInChildren(GameObject reconstructionRoot, Material material)
        {
            foreach (MeshRenderer meshRenderer in reconstructionRoot.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = material;
            }
        }

        private void InitReconstructionForFirstTime()
        {
            _reconstruction = new GameObject("Reconstruction");
            _reconstruction.transform.position = Vector3.zero;
            _reconstruction.transform.rotation = Quaternion.identity;
            _meshGenerator.Parent = _reconstruction.transform;

            ReconstructionInterop.ConnectReconstruction();
            ReconstructionInterop.StartReconstruction();

            Scan();
            _pause = false;
            _reconstructionStarted.Invoke();
        }

        private void Reinitialize()
        {
            if (_pause)
            {
                _reconstruction = new GameObject("Reconstruction");
                _reconstruction.transform.position = Vector3.zero;
                _reconstruction.transform.rotation = Quaternion.identity;
                _meshGenerator.Parent = _reconstruction.transform;

                Scan();
                PauseResumeReconstruction();
                _reconstructionStarted.Invoke();
            }
            else
            {
                Debug.LogWarning("The reconstruction process is already running.");
            }
        }

        private IEnumerator CleanScannedCoroutine()
        {
            StopReconstruction();

            // Waiting 1 sec to avoid crashes when we reset in the same frame we stop.
            yield return new WaitForSeconds(1f);
            ResetReconstruction();

            if (_reconstruction != null)
            {
                Destroy(_reconstruction);
                _reconstruction = null;
            }

            if (_loadedReconstruction != null)
            {
                Destroy(_loadedReconstruction);
                _loadedReconstruction = null;
            }

            SetState(ReconstructionState.Reinitializing);
        }

        private void CleanLoadedCoroutine()
        {
            StopReconstruction();
            
            if (_loadedReconstruction != null)
            {
                Destroy(_loadedReconstruction);
                _loadedReconstruction = null;
            }

            SetState(ReconstructionState.Initializing);
        }

        private IEnumerator LoadReconstructionCoroutine(string environmentProfileName)
        {
            Validate();

            IEnvironmentProfile environmentProfile = GetEnvironmentByName(environmentProfileName);
            GameObject loadedReconstruction = new GameObject("LoadedReconstruction");

            List<MeshData> meshesData = new List<MeshData>();
            List<Thread> threads = new List<Thread>();
            _loadFinished = false;

            for (int i = 0; i < environmentProfile.Meshes.Count; i++)
            {
                string meshPath = environmentProfile.Meshes[i];
                Thread thread = new Thread(
                    () =>
                    {
                        lock (meshesData)
                        {
                            meshesData.Add(_modelFileManipulator.LoadMeshFromFile(meshPath));
                        }
                    });
                thread.Start();
                threads.Add(thread);
                yield return null;
            }

            Thread invokeOnFinished = new Thread(() => ScanThreads(threads, out _loadFinished));
            invokeOnFinished.Start();

            // wait for the end of the loading threads
            yield return new WaitWhile(() => !_loadFinished);

            foreach (MeshData meshData in meshesData)
            {
                GameObject newMesh = new GameObject(meshData.Name);
                MeshFilter meshFilter = newMesh.AddComponent<MeshFilter>();
                meshFilter.mesh.vertices = meshData.Vertices.ToArray();
                meshFilter.mesh.triangles = meshData.Triangles.ToArray();
                newMesh.AddComponent<MeshRenderer>().material = _scanningMaterial;

                Rigidbody rigidbody = newMesh.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                newMesh.AddComponent<MeshCollider>();
                newMesh.transform.SetParent(loadedReconstruction.transform);
                meshFilter.mesh.RecalculateNormals();
            }

            // Remove object from the scene if there is no saved mesh for this environment profile
            if (loadedReconstruction.transform.childCount == 0)
            {
                Destroy(loadedReconstruction);
            }

            _loadedReconstruction = loadedReconstruction;
            _reconstructionLoaded.Invoke(loadedReconstruction);
        }

        private IEnumerator SaveReconstructionCoroutine(string environmentProfileName, bool saveChangesInProfile)
        {
            Validate();

            if (_currentState == ReconstructionState.Scanning && _pause)
            {
                IEnvironmentProfile environmentProfile = GetEnvironmentByName(environmentProfileName);

                int index = 0;
                string path = _environmentProfileRepository.GetPath(environmentProfile.Id);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                List<string> meshesName = new List<string>();
                List<Thread> threads = new List<Thread>();
                _saveFinished = false;

                // Delete old reconstruction meshes
                DeleteReconstructionMeshFiles(environmentProfileName, false);

                foreach (MeshFilter meshFilter in _reconstruction.GetComponentsInChildren<MeshFilter>())
                {
                    Mesh mesh = meshFilter.mesh;
                    Vector3[] vertices = mesh.vertices;
                    int[] triangles = mesh.triangles;

                    string meshName = Path.Combine(path, (++index).ToString());
                    meshesName.Add(meshName + ".obj");

                    Thread thread = new Thread(() => _modelFileManipulator.SaveMeshToFile(meshName, vertices, triangles));
                    thread.Start();
                    threads.Add(thread);
                    yield return null;
                }

                Thread invokeOnFinished = new Thread(() => ScanThreads(threads, out _saveFinished));
                invokeOnFinished.Start();
                yield return new WaitWhile(() => !_saveFinished);

                _environmentProfileRepository.SetMeshes(environmentProfile.Id, meshesName);
                if (saveChangesInProfile)
                {
                    yield return StartCoroutine(SaveReconstructionChangesInProfile());
                    _reconstructionSaved.Invoke();
                }
                else
                {
                    // Invoked only after the meshes are completly saved on disk
                    _reconstructionSaved.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("Trying to save a mesh of a non initialized or ongoing reconstruction.");
            }
        }

        private IEnumerator SaveReconstructionChangesInProfile()
        {
            bool ready = false;
            Thread saveThread = new Thread(() =>
            {
                _environmentProfileRepository.Save();
                ready = true;
            });
            saveThread.Start();
            yield return new WaitUntil(() => ready);
        }

        private void ReconstructMesh()
        {
            // Convert buffers from interop to mesh
            // Get verts and tris
            IntPtr verts;
            IntPtr inds;
            int num_verts;
            int num_tris;
            ReconstructionInterop.GetReconstructionMesh(
                out verts, out num_verts, out inds, out num_tris);

            if (num_verts <= 0 && num_tris <= 0)
            {
                return;
            }

            // Copy data into managed buffers
            double[] result = new double[num_verts];
            Marshal.Copy(verts, result, 0, num_verts);

            int[] triangles = new int[num_tris];
            Marshal.Copy(inds, triangles, 0, num_tris);

            _meshGenerator.UpdateMeshes(result, triangles);
        }

        private void UpdateReconstructionMeshes()
        {
            foreach (MeshRenderer meshRenderer in _reconstruction.GetComponentsInChildren<MeshRenderer>())
            {
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                MeshCollider meshCollider = meshRenderer.GetComponent<MeshCollider>();
                
                // Update the collider
                if (meshCollider == null)
                {
                    meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
                }
                meshCollider.sharedMesh = meshFilter.mesh;
                
                // Update normals
                meshFilter.mesh.RecalculateNormals();
            }
        }

        private void ScanThreads(List<Thread> threads, out bool flag)
        {
            while (true)
            {
                bool isAlive = false;
                foreach (Thread thread in threads)
                {
                    if (thread.IsAlive)
                    {
                        isAlive = true;
                        break;
                    }
                }
                // if there is no thread alive
                if (!isAlive)
                {
                    flag = true;
                    return;
                }
            }
        }

        private IEnvironmentProfile GetEnvironmentByName(string name)
        {
            IEnvironmentProfile environmentProfile = string.IsNullOrEmpty(name) ? _environmentProfileRepository.SelectedEnvironment : _environmentProfileRepository.FindByName(name);
            if (environmentProfile == null)
            {
                throw new KeyNotFoundException("There is no environment specified by name " + name);
            }

            return environmentProfile;
        }

        private void Validate()
        {
            if (_environmentProfileRepository == null)
            {
                throw new NullReferenceException("EnvironmentProfileRepository is null");
            }
        }

        #endregion
    }
}
