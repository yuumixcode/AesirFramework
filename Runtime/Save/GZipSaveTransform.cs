using System.IO;
using System.IO.Compression;

namespace Runestone.AesirModules
{
    /// <summary>
    /// GZip 压缩变换（内置，默认不启用）。经 <see cref="SaveModule.SetTransformChain" /> 注册后
    /// 对存档 payload 施加压缩，加载时自动还原。
    /// </summary>
    public class GZipSaveTransform : ISaveTransform
    {
        /// <inheritdoc />
        public string Id => "gzip";

        /// <inheritdoc />
        public byte[] Encode(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        /// <inheritdoc />
        public byte[] Decode(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}