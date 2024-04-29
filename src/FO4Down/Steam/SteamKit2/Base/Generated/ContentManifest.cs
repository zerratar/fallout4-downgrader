// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: content_manifest.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class ContentManifestPayload : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<FileMapping> mappings { get; } = new global::System.Collections.Generic.List<FileMapping>();

        [global::ProtoBuf.ProtoContract()]
        public partial class FileMapping : global::ProtoBuf.IExtensible
        {
            private global::ProtoBuf.IExtension __pbn__extensionData;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [global::ProtoBuf.ProtoMember(1)]
            [global::System.ComponentModel.DefaultValue("")]
            public string filename
            {
                get => __pbn__filename ?? "";
                set => __pbn__filename = value;
            }
            public bool ShouldSerializefilename() => __pbn__filename != null;
            public void Resetfilename() => __pbn__filename = null;
            private string __pbn__filename;

            [global::ProtoBuf.ProtoMember(2)]
            public ulong size
            {
                get => __pbn__size.GetValueOrDefault();
                set => __pbn__size = value;
            }
            public bool ShouldSerializesize() => __pbn__size != null;
            public void Resetsize() => __pbn__size = null;
            private ulong? __pbn__size;

            [global::ProtoBuf.ProtoMember(3)]
            public uint flags
            {
                get => __pbn__flags.GetValueOrDefault();
                set => __pbn__flags = value;
            }
            public bool ShouldSerializeflags() => __pbn__flags != null;
            public void Resetflags() => __pbn__flags = null;
            private uint? __pbn__flags;

            [global::ProtoBuf.ProtoMember(4)]
            public byte[] sha_filename
            {
                get => __pbn__sha_filename;
                set => __pbn__sha_filename = value;
            }
            public bool ShouldSerializesha_filename() => __pbn__sha_filename != null;
            public void Resetsha_filename() => __pbn__sha_filename = null;
            private byte[] __pbn__sha_filename;

            [global::ProtoBuf.ProtoMember(5)]
            public byte[] sha_content
            {
                get => __pbn__sha_content;
                set => __pbn__sha_content = value;
            }
            public bool ShouldSerializesha_content() => __pbn__sha_content != null;
            public void Resetsha_content() => __pbn__sha_content = null;
            private byte[] __pbn__sha_content;

            [global::ProtoBuf.ProtoMember(6)]
            public global::System.Collections.Generic.List<ChunkData> chunks { get; } = new global::System.Collections.Generic.List<ChunkData>();

            [global::ProtoBuf.ProtoMember(7)]
            [global::System.ComponentModel.DefaultValue("")]
            public string linktarget
            {
                get => __pbn__linktarget ?? "";
                set => __pbn__linktarget = value;
            }
            public bool ShouldSerializelinktarget() => __pbn__linktarget != null;
            public void Resetlinktarget() => __pbn__linktarget = null;
            private string __pbn__linktarget;

            [global::ProtoBuf.ProtoContract()]
            public partial class ChunkData : global::ProtoBuf.IExtensible
            {
                private global::ProtoBuf.IExtension __pbn__extensionData;
                global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                    => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

                [global::ProtoBuf.ProtoMember(1)]
                public byte[] sha
                {
                    get => __pbn__sha;
                    set => __pbn__sha = value;
                }
                public bool ShouldSerializesha() => __pbn__sha != null;
                public void Resetsha() => __pbn__sha = null;
                private byte[] __pbn__sha;

                [global::ProtoBuf.ProtoMember(2, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
                public uint crc
                {
                    get => __pbn__crc.GetValueOrDefault();
                    set => __pbn__crc = value;
                }
                public bool ShouldSerializecrc() => __pbn__crc != null;
                public void Resetcrc() => __pbn__crc = null;
                private uint? __pbn__crc;

                [global::ProtoBuf.ProtoMember(3)]
                public ulong offset
                {
                    get => __pbn__offset.GetValueOrDefault();
                    set => __pbn__offset = value;
                }
                public bool ShouldSerializeoffset() => __pbn__offset != null;
                public void Resetoffset() => __pbn__offset = null;
                private ulong? __pbn__offset;

                [global::ProtoBuf.ProtoMember(4)]
                public uint cb_original
                {
                    get => __pbn__cb_original.GetValueOrDefault();
                    set => __pbn__cb_original = value;
                }
                public bool ShouldSerializecb_original() => __pbn__cb_original != null;
                public void Resetcb_original() => __pbn__cb_original = null;
                private uint? __pbn__cb_original;

                [global::ProtoBuf.ProtoMember(5)]
                public uint cb_compressed
                {
                    get => __pbn__cb_compressed.GetValueOrDefault();
                    set => __pbn__cb_compressed = value;
                }
                public bool ShouldSerializecb_compressed() => __pbn__cb_compressed != null;
                public void Resetcb_compressed() => __pbn__cb_compressed = null;
                private uint? __pbn__cb_compressed;

            }

        }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class ContentManifestMetadata : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint depot_id
        {
            get => __pbn__depot_id.GetValueOrDefault();
            set => __pbn__depot_id = value;
        }
        public bool ShouldSerializedepot_id() => __pbn__depot_id != null;
        public void Resetdepot_id() => __pbn__depot_id = null;
        private uint? __pbn__depot_id;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong gid_manifest
        {
            get => __pbn__gid_manifest.GetValueOrDefault();
            set => __pbn__gid_manifest = value;
        }
        public bool ShouldSerializegid_manifest() => __pbn__gid_manifest != null;
        public void Resetgid_manifest() => __pbn__gid_manifest = null;
        private ulong? __pbn__gid_manifest;

        [global::ProtoBuf.ProtoMember(3)]
        public uint creation_time
        {
            get => __pbn__creation_time.GetValueOrDefault();
            set => __pbn__creation_time = value;
        }
        public bool ShouldSerializecreation_time() => __pbn__creation_time != null;
        public void Resetcreation_time() => __pbn__creation_time = null;
        private uint? __pbn__creation_time;

        [global::ProtoBuf.ProtoMember(4)]
        public bool filenames_encrypted
        {
            get => __pbn__filenames_encrypted.GetValueOrDefault();
            set => __pbn__filenames_encrypted = value;
        }
        public bool ShouldSerializefilenames_encrypted() => __pbn__filenames_encrypted != null;
        public void Resetfilenames_encrypted() => __pbn__filenames_encrypted = null;
        private bool? __pbn__filenames_encrypted;

        [global::ProtoBuf.ProtoMember(5)]
        public ulong cb_disk_original
        {
            get => __pbn__cb_disk_original.GetValueOrDefault();
            set => __pbn__cb_disk_original = value;
        }
        public bool ShouldSerializecb_disk_original() => __pbn__cb_disk_original != null;
        public void Resetcb_disk_original() => __pbn__cb_disk_original = null;
        private ulong? __pbn__cb_disk_original;

        [global::ProtoBuf.ProtoMember(6)]
        public ulong cb_disk_compressed
        {
            get => __pbn__cb_disk_compressed.GetValueOrDefault();
            set => __pbn__cb_disk_compressed = value;
        }
        public bool ShouldSerializecb_disk_compressed() => __pbn__cb_disk_compressed != null;
        public void Resetcb_disk_compressed() => __pbn__cb_disk_compressed = null;
        private ulong? __pbn__cb_disk_compressed;

        [global::ProtoBuf.ProtoMember(7)]
        public uint unique_chunks
        {
            get => __pbn__unique_chunks.GetValueOrDefault();
            set => __pbn__unique_chunks = value;
        }
        public bool ShouldSerializeunique_chunks() => __pbn__unique_chunks != null;
        public void Resetunique_chunks() => __pbn__unique_chunks = null;
        private uint? __pbn__unique_chunks;

        [global::ProtoBuf.ProtoMember(8)]
        public uint crc_encrypted
        {
            get => __pbn__crc_encrypted.GetValueOrDefault();
            set => __pbn__crc_encrypted = value;
        }
        public bool ShouldSerializecrc_encrypted() => __pbn__crc_encrypted != null;
        public void Resetcrc_encrypted() => __pbn__crc_encrypted = null;
        private uint? __pbn__crc_encrypted;

        [global::ProtoBuf.ProtoMember(9)]
        public uint crc_clear
        {
            get => __pbn__crc_clear.GetValueOrDefault();
            set => __pbn__crc_clear = value;
        }
        public bool ShouldSerializecrc_clear() => __pbn__crc_clear != null;
        public void Resetcrc_clear() => __pbn__crc_clear = null;
        private uint? __pbn__crc_clear;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class ContentManifestSignature : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public byte[] signature
        {
            get => __pbn__signature;
            set => __pbn__signature = value;
        }
        public bool ShouldSerializesignature() => __pbn__signature != null;
        public void Resetsignature() => __pbn__signature = null;
        private byte[] __pbn__signature;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class ContentDeltaChunks : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint depot_id
        {
            get => __pbn__depot_id.GetValueOrDefault();
            set => __pbn__depot_id = value;
        }
        public bool ShouldSerializedepot_id() => __pbn__depot_id != null;
        public void Resetdepot_id() => __pbn__depot_id = null;
        private uint? __pbn__depot_id;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong manifest_id_source
        {
            get => __pbn__manifest_id_source.GetValueOrDefault();
            set => __pbn__manifest_id_source = value;
        }
        public bool ShouldSerializemanifest_id_source() => __pbn__manifest_id_source != null;
        public void Resetmanifest_id_source() => __pbn__manifest_id_source = null;
        private ulong? __pbn__manifest_id_source;

        [global::ProtoBuf.ProtoMember(3)]
        public ulong manifest_id_target
        {
            get => __pbn__manifest_id_target.GetValueOrDefault();
            set => __pbn__manifest_id_target = value;
        }
        public bool ShouldSerializemanifest_id_target() => __pbn__manifest_id_target != null;
        public void Resetmanifest_id_target() => __pbn__manifest_id_target = null;
        private ulong? __pbn__manifest_id_target;

        [global::ProtoBuf.ProtoMember(4)]
        public global::System.Collections.Generic.List<DeltaChunk> deltaChunks { get; } = new global::System.Collections.Generic.List<DeltaChunk>();

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue(EContentDeltaChunkDataLocation.k_EContentDeltaChunkDataLocationInProtobuf)]
        public EContentDeltaChunkDataLocation chunk_data_location
        {
            get => __pbn__chunk_data_location ?? EContentDeltaChunkDataLocation.k_EContentDeltaChunkDataLocationInProtobuf;
            set => __pbn__chunk_data_location = value;
        }
        public bool ShouldSerializechunk_data_location() => __pbn__chunk_data_location != null;
        public void Resetchunk_data_location() => __pbn__chunk_data_location = null;
        private EContentDeltaChunkDataLocation? __pbn__chunk_data_location;

        [global::ProtoBuf.ProtoContract()]
        public partial class DeltaChunk : global::ProtoBuf.IExtensible
        {
            private global::ProtoBuf.IExtension __pbn__extensionData;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [global::ProtoBuf.ProtoMember(1)]
            public byte[] sha_source
            {
                get => __pbn__sha_source;
                set => __pbn__sha_source = value;
            }
            public bool ShouldSerializesha_source() => __pbn__sha_source != null;
            public void Resetsha_source() => __pbn__sha_source = null;
            private byte[] __pbn__sha_source;

            [global::ProtoBuf.ProtoMember(2)]
            public byte[] sha_target
            {
                get => __pbn__sha_target;
                set => __pbn__sha_target = value;
            }
            public bool ShouldSerializesha_target() => __pbn__sha_target != null;
            public void Resetsha_target() => __pbn__sha_target = null;
            private byte[] __pbn__sha_target;

            [global::ProtoBuf.ProtoMember(3)]
            public uint size_original
            {
                get => __pbn__size_original.GetValueOrDefault();
                set => __pbn__size_original = value;
            }
            public bool ShouldSerializesize_original() => __pbn__size_original != null;
            public void Resetsize_original() => __pbn__size_original = null;
            private uint? __pbn__size_original;

            [global::ProtoBuf.ProtoMember(4)]
            public uint patch_method
            {
                get => __pbn__patch_method.GetValueOrDefault();
                set => __pbn__patch_method = value;
            }
            public bool ShouldSerializepatch_method() => __pbn__patch_method != null;
            public void Resetpatch_method() => __pbn__patch_method = null;
            private uint? __pbn__patch_method;

            [global::ProtoBuf.ProtoMember(5)]
            public byte[] chunk
            {
                get => __pbn__chunk;
                set => __pbn__chunk = value;
            }
            public bool ShouldSerializechunk() => __pbn__chunk != null;
            public void Resetchunk() => __pbn__chunk = null;
            private byte[] __pbn__chunk;

            [global::ProtoBuf.ProtoMember(6)]
            public uint size_delta
            {
                get => __pbn__size_delta.GetValueOrDefault();
                set => __pbn__size_delta = value;
            }
            public bool ShouldSerializesize_delta() => __pbn__size_delta != null;
            public void Resetsize_delta() => __pbn__size_delta = null;
            private uint? __pbn__size_delta;

        }

    }

    [global::ProtoBuf.ProtoContract()]
    public enum EContentDeltaChunkDataLocation
    {
        k_EContentDeltaChunkDataLocationInProtobuf = 0,
        k_EContentDeltaChunkDataLocationAfterProtobuf = 1,
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion