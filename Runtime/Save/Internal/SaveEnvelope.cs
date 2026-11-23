using System;
using System.IO;
using System.Text;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 存档信封：固定二进制头 + payload。信封头<b>不经过用户序列化器</b>（否则加载时连头都读不出），
    /// 承载 magic、格式版本、序列化器标识、变换链标识、数据版本与 payload 校验和。
    /// </summary>
    /// <remarks>
    /// 字节布局（小端）：
    /// <code>
    /// 偏移   大小        字段
    /// 0      4          magic "ASAV"
    /// 4      1          信封格式版本（当前 = 1）
    /// 5      1          serializerId 字节长 N（UTF8，≤255）
    /// 6      N          serializerId
    /// 6+N    1          变换数 M
    /// …      M×(1+len)  各 transformId（1 字节长 + UTF8，按施加顺序）
    /// …      4          dataVersion（int32）
    /// …      4          payload 字节长（int32）
    /// …      4          payload CRC32（uint32，对变换后字节计算）
    /// …      其余       payload
    /// </code>
    /// 加载校验链：magic → 格式版本 → 头部长度自洽（payload 长 = 文件实际剩余）→ CRC32。
    /// 任一失败即判该份存档损坏。格式演进时 bump <see cref="FormatVersion" />，旧版本明确拒绝而非猜测解析。
    /// </remarks>
    internal static class SaveEnvelope
    {
        /// <summary>当前信封格式版本。</summary>
        public const byte FormatVersion = 1;

        static readonly byte[] Magic = { (byte)'A', (byte)'S', (byte)'A', (byte)'V' };

        /// <summary>封装信封：头 + payload 拼接为完整存档字节。</summary>
        /// <param name="serializerId">序列化器标识（UTF8 字节长 ≤255）。</param>
        /// <param name="transformIds">变换链标识（按施加顺序，数量与各 Id 字节长 ≤255）。</param>
        /// <param name="dataVersion">数据版本号。</param>
        /// <param name="payload">变换后的序列化字节。</param>
        public static byte[] Wrap(string serializerId, string[] transformIds, int dataVersion, byte[] payload)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(Magic, 0, Magic.Length);
                stream.WriteByte(FormatVersion);
                WriteShortString(stream, serializerId);
                stream.WriteByte((byte)transformIds.Length);
                for (var i = 0; i < transformIds.Length; i++)
                {
                    WriteShortString(stream, transformIds[i]);
                }

                WriteInt32(stream, dataVersion);
                WriteInt32(stream, payload.Length);
                WriteUInt32(stream, Crc32.Compute(payload));
                stream.Write(payload, 0, payload.Length);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// 解封装并走完整校验链；任一校验失败返回 false 并给出损坏描述（不抛异常）。
        /// </summary>
        /// <param name="file">完整存档字节。</param>
        /// <param name="data">校验通过时的信封数据。</param>
        /// <param name="error">校验失败时的损坏描述。</param>
        public static bool TryUnwrap(byte[] file, out EnvelopeData data, out string error)
        {
            data = default;
            error = null;

            if (file == null || file.Length == 0)
            {
                error = "存档字节为空";
                return false;
            }

            try
            {
                using (var stream = new MemoryStream(file))
                using (var reader = new BinaryReader(stream))
                {
                    var magic = reader.ReadBytes(Magic.Length);
                    if (magic.Length != Magic.Length ||
                        magic[0] != Magic[0] || magic[1] != Magic[1] ||
                        magic[2] != Magic[2] || magic[3] != Magic[3])
                    {
                        error = "存档 magic 不匹配（非本模块存档或文件头损坏）";
                        return false;
                    }

                    var version = reader.ReadByte();
                    if (version != FormatVersion)
                    {
                        error = $"存档信封格式版本 {version} 不受支持（当前 {FormatVersion}）";
                        return false;
                    }

                    var serializerId = ReadShortString(reader);
                    var transformCount = reader.ReadByte();
                    var transformIds = new string[transformCount];
                    for (var i = 0; i < transformCount; i++)
                    {
                        transformIds[i] = ReadShortString(reader);
                    }

                    var dataVersion = reader.ReadInt32();
                    var payloadLength = reader.ReadInt32();
                    var checksum = reader.ReadUInt32();

                    if (payloadLength < 0 || payloadLength != stream.Length - stream.Position)
                    {
                        error = $"存档长度不自洽（声明 payload {payloadLength} 字节，实际剩余 {stream.Length - stream.Position} 字节）";
                        return false;
                    }

                    var payload = reader.ReadBytes(payloadLength);
                    if (Crc32.Compute(payload) != checksum)
                    {
                        error = "存档 payload 校验和失败（内容被截断或位翻转）";
                        return false;
                    }

                    data = new EnvelopeData(serializerId, transformIds, dataVersion, payload);
                    return true;
                }
            }
            catch (EndOfStreamException)
            {
                error = "存档文件头截断";
                return false;
            }
        }

        static void WriteShortString(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > byte.MaxValue)
            {
                throw new ArgumentException($"标识 '{value}' 的 UTF8 字节长超过 255，无法写入信封");
            }

            stream.WriteByte((byte)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        static string ReadShortString(BinaryReader reader)
        {
            var length = reader.ReadByte();
            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new EndOfStreamException();
            }

            return Encoding.UTF8.GetString(bytes);
        }

        static void WriteInt32(Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        static void WriteUInt32(Stream stream, uint value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }
    }

    /// <summary>信封解封装结果（校验通过后有效）。</summary>
    internal readonly struct EnvelopeData
    {
        /// <summary>存档记录的序列化器标识。</summary>
        public string SerializerId { get; }

        /// <summary>存档记录的变换链标识（按施加顺序）。</summary>
        public string[] TransformIds { get; }

        /// <summary>存档记录的数据版本号。</summary>
        public int DataVersion { get; }

        /// <summary>变换后的序列化字节。</summary>
        public byte[] Payload { get; }

        public EnvelopeData(string serializerId, string[] transformIds, int dataVersion, byte[] payload)
        {
            SerializerId = serializerId;
            TransformIds = transformIds;
            DataVersion = dataVersion;
            Payload = payload;
        }
    }
}