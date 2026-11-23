namespace Runestone.AesirModules
{
    /// <summary>
    /// CRC32 校验和（IEEE 多项式，表驱动）。用于存档信封的 payload 完整性校验——
    /// 检测意外损坏（截断 / 位翻转），<b>不防对抗篡改</b>（防篡改需服务端校验，超出模块范围）。
    /// </summary>
    internal static class Crc32
    {
        static readonly uint[] Table = BuildTable();

        /// <summary>计算字节数组的 CRC32 校验和。</summary>
        public static uint Compute(byte[] data)
        {
            var crc = 0xFFFFFFFFu;
            for (var i = 0; i < data.Length; i++)
            {
                crc = (crc >> 8) ^ Table[(crc ^ data[i]) & 0xFF];
            }

            return crc ^ 0xFFFFFFFFu;
        }

        static uint[] BuildTable()
        {
            // IEEE 802.3 反射多项式 0xEDB88320（与 zlib/gzip 的 CRC32 同口径）
            const uint polynomial = 0xEDB88320u;
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                var entry = i;
                for (var bit = 0; bit < 8; bit++)
                {
                    entry = (entry & 1u) != 0 ? (entry >> 1) ^ polynomial : entry >> 1;
                }

                table[i] = entry;
            }

            return table;
        }
    }
}