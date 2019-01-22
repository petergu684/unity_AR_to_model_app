﻿// Copyright Â© 2018, Meta Company.  All rights reserved.
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
// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace meta.types
{

using global::System;
using global::FlatBuffers;

public struct BufferHeader : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static BufferHeader GetRootAsBufferHeader(ByteBuffer _bb) { return GetRootAsBufferHeader(_bb, new BufferHeader()); }
  public static BufferHeader GetRootAsBufferHeader(ByteBuffer _bb, BufferHeader obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public BufferHeader __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public double Timestamp { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  public ulong FrameId { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetUlong(o + __p.bb_pos) : (ulong)0; } }

  public static Offset<BufferHeader> CreateBufferHeader(FlatBufferBuilder builder,
      double timestamp = 0.0,
      ulong frame_id = 0) {
    builder.StartObject(2);
    BufferHeader.AddFrameId(builder, frame_id);
    BufferHeader.AddTimestamp(builder, timestamp);
    return BufferHeader.EndBufferHeader(builder);
  }

  public static void StartBufferHeader(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddTimestamp(FlatBufferBuilder builder, double timestamp) { builder.AddDouble(0, timestamp, 0.0); }
  public static void AddFrameId(FlatBufferBuilder builder, ulong frameId) { builder.AddUlong(1, frameId, 0); }
  public static Offset<BufferHeader> EndBufferHeader(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<BufferHeader>(o);
  }
  public static void FinishBufferHeaderBuffer(FlatBufferBuilder builder, Offset<BufferHeader> offset) { builder.Finish(offset.Value); }
};


}
