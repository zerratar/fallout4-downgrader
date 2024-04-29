// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: gc.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.GC.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CMsgGCRoutingProtoBufHeader : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong dst_gcid_queue
        {
            get => __pbn__dst_gcid_queue.GetValueOrDefault();
            set => __pbn__dst_gcid_queue = value;
        }
        public bool ShouldSerializedst_gcid_queue() => __pbn__dst_gcid_queue != null;
        public void Resetdst_gcid_queue() => __pbn__dst_gcid_queue = null;
        private ulong? __pbn__dst_gcid_queue;

        [global::ProtoBuf.ProtoMember(2)]
        public uint dst_gc_dir_index
        {
            get => __pbn__dst_gc_dir_index.GetValueOrDefault();
            set => __pbn__dst_gc_dir_index = value;
        }
        public bool ShouldSerializedst_gc_dir_index() => __pbn__dst_gc_dir_index != null;
        public void Resetdst_gc_dir_index() => __pbn__dst_gc_dir_index = null;
        private uint? __pbn__dst_gc_dir_index;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CMsgProtoBufHeader : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong client_steam_id
        {
            get => __pbn__client_steam_id.GetValueOrDefault();
            set => __pbn__client_steam_id = value;
        }
        public bool ShouldSerializeclient_steam_id() => __pbn__client_steam_id != null;
        public void Resetclient_steam_id() => __pbn__client_steam_id = null;
        private ulong? __pbn__client_steam_id;

        [global::ProtoBuf.ProtoMember(2)]
        public int client_session_id
        {
            get => __pbn__client_session_id.GetValueOrDefault();
            set => __pbn__client_session_id = value;
        }
        public bool ShouldSerializeclient_session_id() => __pbn__client_session_id != null;
        public void Resetclient_session_id() => __pbn__client_session_id = null;
        private int? __pbn__client_session_id;

        [global::ProtoBuf.ProtoMember(3)]
        public uint source_app_id
        {
            get => __pbn__source_app_id.GetValueOrDefault();
            set => __pbn__source_app_id = value;
        }
        public bool ShouldSerializesource_app_id() => __pbn__source_app_id != null;
        public void Resetsource_app_id() => __pbn__source_app_id = null;
        private uint? __pbn__source_app_id;

        [global::ProtoBuf.ProtoMember(10, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        [global::System.ComponentModel.DefaultValue(typeof(ulong), "18446744073709551615")]
        public ulong job_id_source
        {
            get => __pbn__job_id_source ?? 18446744073709551615ul;
            set => __pbn__job_id_source = value;
        }
        public bool ShouldSerializejob_id_source() => __pbn__job_id_source != null;
        public void Resetjob_id_source() => __pbn__job_id_source = null;
        private ulong? __pbn__job_id_source;

        [global::ProtoBuf.ProtoMember(11, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        [global::System.ComponentModel.DefaultValue(typeof(ulong), "18446744073709551615")]
        public ulong job_id_target
        {
            get => __pbn__job_id_target ?? 18446744073709551615ul;
            set => __pbn__job_id_target = value;
        }
        public bool ShouldSerializejob_id_target() => __pbn__job_id_target != null;
        public void Resetjob_id_target() => __pbn__job_id_target = null;
        private ulong? __pbn__job_id_target;

        [global::ProtoBuf.ProtoMember(12)]
        [global::System.ComponentModel.DefaultValue("")]
        public string target_job_name
        {
            get => __pbn__target_job_name ?? "";
            set => __pbn__target_job_name = value;
        }
        public bool ShouldSerializetarget_job_name() => __pbn__target_job_name != null;
        public void Resettarget_job_name() => __pbn__target_job_name = null;
        private string __pbn__target_job_name;

        [global::ProtoBuf.ProtoMember(24)]
        public int seq_num
        {
            get => __pbn__seq_num.GetValueOrDefault();
            set => __pbn__seq_num = value;
        }
        public bool ShouldSerializeseq_num() => __pbn__seq_num != null;
        public void Resetseq_num() => __pbn__seq_num = null;
        private int? __pbn__seq_num;

        [global::ProtoBuf.ProtoMember(13)]
        [global::System.ComponentModel.DefaultValue(2)]
        public int eresult
        {
            get => __pbn__eresult ?? 2;
            set => __pbn__eresult = value;
        }
        public bool ShouldSerializeeresult() => __pbn__eresult != null;
        public void Reseteresult() => __pbn__eresult = null;
        private int? __pbn__eresult;

        [global::ProtoBuf.ProtoMember(14)]
        [global::System.ComponentModel.DefaultValue("")]
        public string error_message
        {
            get => __pbn__error_message ?? "";
            set => __pbn__error_message = value;
        }
        public bool ShouldSerializeerror_message() => __pbn__error_message != null;
        public void Reseterror_message() => __pbn__error_message = null;
        private string __pbn__error_message;

        [global::ProtoBuf.ProtoMember(16)]
        public uint auth_account_flags
        {
            get => __pbn__auth_account_flags.GetValueOrDefault();
            set => __pbn__auth_account_flags = value;
        }
        public bool ShouldSerializeauth_account_flags() => __pbn__auth_account_flags != null;
        public void Resetauth_account_flags() => __pbn__auth_account_flags = null;
        private uint? __pbn__auth_account_flags;

        [global::ProtoBuf.ProtoMember(22)]
        public uint token_source
        {
            get => __pbn__token_source.GetValueOrDefault();
            set => __pbn__token_source = value;
        }
        public bool ShouldSerializetoken_source() => __pbn__token_source != null;
        public void Resettoken_source() => __pbn__token_source = null;
        private uint? __pbn__token_source;

        [global::ProtoBuf.ProtoMember(23)]
        public bool admin_spoofing_user
        {
            get => __pbn__admin_spoofing_user.GetValueOrDefault();
            set => __pbn__admin_spoofing_user = value;
        }
        public bool ShouldSerializeadmin_spoofing_user() => __pbn__admin_spoofing_user != null;
        public void Resetadmin_spoofing_user() => __pbn__admin_spoofing_user = null;
        private bool? __pbn__admin_spoofing_user;

        [global::ProtoBuf.ProtoMember(17)]
        [global::System.ComponentModel.DefaultValue(1)]
        public int transport_error
        {
            get => __pbn__transport_error ?? 1;
            set => __pbn__transport_error = value;
        }
        public bool ShouldSerializetransport_error() => __pbn__transport_error != null;
        public void Resettransport_error() => __pbn__transport_error = null;
        private int? __pbn__transport_error;

        [global::ProtoBuf.ProtoMember(18)]
        [global::System.ComponentModel.DefaultValue(typeof(ulong), "18446744073709551615")]
        public ulong messageid
        {
            get => __pbn__messageid ?? 18446744073709551615ul;
            set => __pbn__messageid = value;
        }
        public bool ShouldSerializemessageid() => __pbn__messageid != null;
        public void Resetmessageid() => __pbn__messageid = null;
        private ulong? __pbn__messageid;

        [global::ProtoBuf.ProtoMember(19)]
        public uint publisher_group_id
        {
            get => __pbn__publisher_group_id.GetValueOrDefault();
            set => __pbn__publisher_group_id = value;
        }
        public bool ShouldSerializepublisher_group_id() => __pbn__publisher_group_id != null;
        public void Resetpublisher_group_id() => __pbn__publisher_group_id = null;
        private uint? __pbn__publisher_group_id;

        [global::ProtoBuf.ProtoMember(20)]
        public uint sysid
        {
            get => __pbn__sysid.GetValueOrDefault();
            set => __pbn__sysid = value;
        }
        public bool ShouldSerializesysid() => __pbn__sysid != null;
        public void Resetsysid() => __pbn__sysid = null;
        private uint? __pbn__sysid;

        [global::ProtoBuf.ProtoMember(21)]
        public ulong trace_tag
        {
            get => __pbn__trace_tag.GetValueOrDefault();
            set => __pbn__trace_tag = value;
        }
        public bool ShouldSerializetrace_tag() => __pbn__trace_tag != null;
        public void Resettrace_tag() => __pbn__trace_tag = null;
        private ulong? __pbn__trace_tag;

        [global::ProtoBuf.ProtoMember(25)]
        public uint webapi_key_id
        {
            get => __pbn__webapi_key_id.GetValueOrDefault();
            set => __pbn__webapi_key_id = value;
        }
        public bool ShouldSerializewebapi_key_id() => __pbn__webapi_key_id != null;
        public void Resetwebapi_key_id() => __pbn__webapi_key_id = null;
        private uint? __pbn__webapi_key_id;

        [global::ProtoBuf.ProtoMember(26)]
        public bool is_from_external_source
        {
            get => __pbn__is_from_external_source.GetValueOrDefault();
            set => __pbn__is_from_external_source = value;
        }
        public bool ShouldSerializeis_from_external_source() => __pbn__is_from_external_source != null;
        public void Resetis_from_external_source() => __pbn__is_from_external_source = null;
        private bool? __pbn__is_from_external_source;

        [global::ProtoBuf.ProtoMember(27)]
        public global::System.Collections.Generic.List<uint> forward_to_sysid { get; } = new global::System.Collections.Generic.List<uint>();

        [global::ProtoBuf.ProtoMember(28)]
        public uint cm_sysid
        {
            get => __pbn__cm_sysid.GetValueOrDefault();
            set => __pbn__cm_sysid = value;
        }
        public bool ShouldSerializecm_sysid() => __pbn__cm_sysid != null;
        public void Resetcm_sysid() => __pbn__cm_sysid = null;
        private uint? __pbn__cm_sysid;

        [global::ProtoBuf.ProtoMember(31)]
        [global::System.ComponentModel.DefaultValue(0u)]
        public uint launcher_type
        {
            get => __pbn__launcher_type ?? 0u;
            set => __pbn__launcher_type = value;
        }
        public bool ShouldSerializelauncher_type() => __pbn__launcher_type != null;
        public void Resetlauncher_type() => __pbn__launcher_type = null;
        private uint? __pbn__launcher_type;

        [global::ProtoBuf.ProtoMember(32)]
        [global::System.ComponentModel.DefaultValue(0u)]
        public uint realm
        {
            get => __pbn__realm ?? 0u;
            set => __pbn__realm = value;
        }
        public bool ShouldSerializerealm() => __pbn__realm != null;
        public void Resetrealm() => __pbn__realm = null;
        private uint? __pbn__realm;

        [global::ProtoBuf.ProtoMember(33)]
        [global::System.ComponentModel.DefaultValue(-1)]
        public int timeout_ms
        {
            get => __pbn__timeout_ms ?? -1;
            set => __pbn__timeout_ms = value;
        }
        public bool ShouldSerializetimeout_ms() => __pbn__timeout_ms != null;
        public void Resettimeout_ms() => __pbn__timeout_ms = null;
        private int? __pbn__timeout_ms;

        [global::ProtoBuf.ProtoMember(34)]
        [global::System.ComponentModel.DefaultValue("")]
        public string debug_source
        {
            get => __pbn__debug_source ?? "";
            set => __pbn__debug_source = value;
        }
        public bool ShouldSerializedebug_source() => __pbn__debug_source != null;
        public void Resetdebug_source() => __pbn__debug_source = null;
        private string __pbn__debug_source;

        [global::ProtoBuf.ProtoMember(35)]
        public uint debug_source_string_index
        {
            get => __pbn__debug_source_string_index.GetValueOrDefault();
            set => __pbn__debug_source_string_index = value;
        }
        public bool ShouldSerializedebug_source_string_index() => __pbn__debug_source_string_index != null;
        public void Resetdebug_source_string_index() => __pbn__debug_source_string_index = null;
        private uint? __pbn__debug_source_string_index;

        [global::ProtoBuf.ProtoMember(36)]
        public ulong token_id
        {
            get => __pbn__token_id.GetValueOrDefault();
            set => __pbn__token_id = value;
        }
        public bool ShouldSerializetoken_id() => __pbn__token_id != null;
        public void Resettoken_id() => __pbn__token_id = null;
        private ulong? __pbn__token_id;

        [global::ProtoBuf.ProtoMember(37)]
        public CMsgGCRoutingProtoBufHeader routing_gc { get; set; }

        [global::ProtoBuf.ProtoMember(200)]
        [global::System.ComponentModel.DefaultValue(GCProtoBufMsgSrc.GCProtoBufMsgSrc_Unspecified)]
        public GCProtoBufMsgSrc gc_msg_src
        {
            get => __pbn__gc_msg_src ?? GCProtoBufMsgSrc.GCProtoBufMsgSrc_Unspecified;
            set => __pbn__gc_msg_src = value;
        }
        public bool ShouldSerializegc_msg_src() => __pbn__gc_msg_src != null;
        public void Resetgc_msg_src() => __pbn__gc_msg_src = null;
        private GCProtoBufMsgSrc? __pbn__gc_msg_src;

        [global::ProtoBuf.ProtoMember(201)]
        public uint gc_dir_index_source
        {
            get => __pbn__gc_dir_index_source.GetValueOrDefault();
            set => __pbn__gc_dir_index_source = value;
        }
        public bool ShouldSerializegc_dir_index_source() => __pbn__gc_dir_index_source != null;
        public void Resetgc_dir_index_source() => __pbn__gc_dir_index_source = null;
        private uint? __pbn__gc_dir_index_source;

        [global::ProtoBuf.ProtoMember(15)]
        public uint ip
        {
            get => __pbn__ip_addr.Is(15) ? __pbn__ip_addr.UInt32 : default;
            set => __pbn__ip_addr = new global::ProtoBuf.DiscriminatedUnion32Object(15, value);
        }
        public bool ShouldSerializeip() => __pbn__ip_addr.Is(15);
        public void Resetip() => global::ProtoBuf.DiscriminatedUnion32Object.Reset(ref __pbn__ip_addr, 15);

        private global::ProtoBuf.DiscriminatedUnion32Object __pbn__ip_addr;

        [global::ProtoBuf.ProtoMember(29)]
        public byte[] ip_v6
        {
            get => __pbn__ip_addr.Is(29) ? ((byte[])__pbn__ip_addr.Object) : default;
            set => __pbn__ip_addr = new global::ProtoBuf.DiscriminatedUnion32Object(29, value);
        }
        public bool ShouldSerializeip_v6() => __pbn__ip_addr.Is(29);
        public void Resetip_v6() => global::ProtoBuf.DiscriminatedUnion32Object.Reset(ref __pbn__ip_addr, 29);

    }

    [global::ProtoBuf.ProtoContract()]
    public enum GCProtoBufMsgSrc
    {
        GCProtoBufMsgSrc_Unspecified = 0,
        GCProtoBufMsgSrc_FromSystem = 1,
        GCProtoBufMsgSrc_FromSteamID = 2,
        GCProtoBufMsgSrc_FromGC = 3,
        GCProtoBufMsgSrc_ReplySystem = 4,
        GCProtoBufMsgSrc_SpoofedSteamID = 5,
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion