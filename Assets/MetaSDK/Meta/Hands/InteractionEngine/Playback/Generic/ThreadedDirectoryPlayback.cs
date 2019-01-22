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
using System.Threading;
using System.IO;
using System;
using System.Linq;

namespace Meta.Internal.Playback
{
    /// <summary>
    /// Contains context data for each parse thread.
    /// </summary>
    internal class ParseTaskInfo
    {
        public FileInfo file;
        public int frameID;

        public ParseTaskInfo(FileInfo f, int id)
        {
            file = f;
            frameID = id;
        }
    }

    /// <summary>
    /// Handles threaded parsing of playback data.
    /// </summary>
    /// <typeparam name="T">The type of data to be returned.</typeparam>
    internal class ThreadedDirectoryPlayback<T> : GenericDirectoryPlayback<T>
    {
        private Thread _readThread;
        // The id the last frame that was queued. Used to maintain order of frames in queue.
        private int _lastQueuedFrameId = -1;
        private readonly object _totalFrameLock;
        private readonly object _incrementLastQueuedIdLock;
        private const int MaxThreads = 10;
        private const int SleepTimer = 10;

        public ThreadedDirectoryPlayback() : base()
        {
            _totalFrameLock = new object();
            _incrementLastQueuedIdLock = new object();
            ThreadPool.SetMaxThreads(MaxThreads, MaxThreads);
        }

        /// <summary>
        /// Constructor for this object.
        /// </summary>
        /// <param name="playbackFolder">The folder to be used for playback.</param>
        /// <param name="extension">The file extension to read for playback data.</param>
	    public ThreadedDirectoryPlayback(string playbackFolder, string extension) : base(playbackFolder, extension)
        {
            _totalFrameLock = new object();
            _incrementLastQueuedIdLock = new object();
            ThreadPool.SetMaxThreads(MaxThreads, MaxThreads);
        }

        /// <summary>
        /// Constructor for this object.
        /// </summary>
        /// <param name="playbackFolder">The folder to be used for playback.</param>
        /// <param name="extension">The file extension to read for playback data.</param>
        public ThreadedDirectoryPlayback(string playbackFolder, string extension, IFileParser<T> parser) : base(playbackFolder, extension, parser)
        {
            _totalFrameLock = new object();
            _incrementLastQueuedIdLock = new object();
            ThreadPool.SetMaxThreads(MaxThreads, MaxThreads);
        }

        /// <summary>
        /// Threaded implementation for loading filenames and sending each file to be buffered and processed by a worker pool.
        /// </summary>
        /// 
        public sealed override void LoadFrameFiles()
        {
            _readThread = new Thread(QueueFilesForThreads);
            _readThread.Start(); 
        }

        /// <summary>
        /// Gets list of files in the playback folder and create a worker task to parse the file.
        /// </summary>
        private void QueueFilesForThreads()
        {
            DirectoryInfo dir = new DirectoryInfo(_playbackFolder);
            IOrderedEnumerable<FileInfo> files = dir.GetFiles(_extension).OrderBy(
                f => TryGetFileIDLength(f)
            );

            // This may need to be updated if there are invalid frames found, if checking # frames read as the stopping condition.
            lock (_totalFrameLock)
            {
                try
                {
                    _totalFrames = files.Count();
                }
                catch (Exception e)
                {
                    Debug.LogError("A filename could not be parsed as a frame ID (int): " + e.Message);
                }
            }
            Debug.Log(_extension + " files to process in thread: " + _totalFrames);
            int frameId = 0;
            foreach (FileInfo f in files)
            {
                ThreadPool.QueueUserWorkItem(ThreadPoolCallback, new ParseTaskInfo(f, frameId++));
            }
        }

        /// <summary>
        /// Worker task for reading a playback data file.
        /// </summary>
        /// <param name="context">Parse task info needed by this thread.</param>
        private void ThreadPoolCallback(object context)
        {
            ParseTaskInfo taskInfo = (ParseTaskInfo) context;
            IFileParser<T> ownParser = (IFileParser<T>) Activator.CreateInstance(_parser.GetType());
            try
            {
                // Since not every frame might be captured, use the filename as the actual frame ID and frameID to maintain queue order.
                int fileNameId = int.Parse(Path.GetFileNameWithoutExtension(taskInfo.file.Name));
                T frame = ownParser.ParseFile(taskInfo.file, fileNameId);
                while (taskInfo.frameID != _lastQueuedFrameId + 1)
                {
                    Thread.Sleep(SleepTimer);
                }
                AddToPlayback(frame);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message + " " + e.StackTrace);
                DecrementTotalFrames();
                while (taskInfo.frameID != _lastQueuedFrameId + 1)
                {
                    Thread.Sleep(SleepTimer);
                }
                IncrementLastQueuedFrame();
            }
        }

        /// <summary>
        /// Adds the frame to the queue using explicit synchronisation.
        /// </summary>
        /// <param name="data">The data to be stored.</param>
        protected sealed override void AddToPlayback(T data)
        {
            lock (_frames)
            {
                _frames.Add(data);
                _lastQueuedFrameId++;
            }
        }

        /// <summary>
        /// Decreases total frame count. Called when a thread finds an invalid frame.
        /// </summary>
        private void DecrementTotalFrames()
        {
            lock (_totalFrameLock)
            {
                _totalFrames--;
            }
        }

        /// <summary>
        /// Increments the next frame counter. Used to queue frames in ID order.
        /// </summary>
        private void IncrementLastQueuedFrame()
        {
            lock (_incrementLastQueuedIdLock)
            {
                _lastQueuedFrameId++;
            }
        }

        #region Playback Controls

        public sealed override int GetTotalFrameCount()
        {
            lock (_totalFrameLock)
            {
                return base.GetTotalFrameCount();
            }
        }

        /// <summary>
        /// Indicates if all frames in the playback directory have been seen.
        /// </summary>
        /// <returns>True, if all valid frames have been played (dequeued).</returns>
        public sealed override bool IsFinished()
        {
            lock (_frames)
            {
                return base.IsFinished();
            }
        }

        /// <summary>
        /// Indicates if all frames in the playback directory are loaded for playback.
        /// </summary>
        /// <returns>True, if all files are parsed and in the queue. If the number of files exceeds the buffer size, then the buffer size is used for comparison instead of the total frame count.</returns>
        public sealed override bool AreFramesLoaded()
        {
            lock (_frames)
            {
                return (_frames.Count != 0) && (_frames.Count == _totalFrames);
            }
        }

        /// <summary>
        /// Indicates if there is another frame left for playback.
        /// </summary>
        /// <returns>True, if there is an unseen frame remaining. Else, returns false.</returns>
        public sealed override bool HasNextFrame()
        {
            lock (_frames)
            {
                return base.HasNextFrame();
            }
        }

        /// <summary>
        /// Gets the next unseen frame for playback.
        /// </summary>
        /// <returns>The next unseen frame in ID order.</returns>
        public sealed override T NextFrame()
        {
            lock (_frames)
            {
                return base.NextFrame();
            }
        }

        public sealed override bool HasPrevFrame()
        {
            lock (_frames)
            {
                return base.HasPrevFrame();
            }
        }

        public sealed override T PreviousFrame()
        {
            lock (_frames)
            {
                return base.PreviousFrame();
            }
        }

        public sealed override void Reset()
        {
            lock (_frames)
            {
                base.Reset();
            }
        }

        public sealed override void UseNewPlaybackSourcePath(string directory, string extension)
        {
            _lastQueuedFrameId = -1;
            if (_readThread != null)
            {
                _readThread.Abort();
            }
            base.UseNewPlaybackSourcePath(directory, extension);
        }

        #endregion
    }
}
