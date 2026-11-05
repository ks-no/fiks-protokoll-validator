using System;
using System.IO;
using System.Threading.Tasks;

namespace KS.FiksProtokollValidator.WebAPI.Utilities
{
    // Reads the entries of one ASiC-E into memory, with limits. The entries are compressed, so the size of a message
    // does not show how much memory its entries take when they are read. Use one instance per message.
    public class AsiceReadLimits
    {
        public const long DefaultMaxEntryBytes = 50L * 1024 * 1024;
        public const long DefaultMaxTotalBytes = 200L * 1024 * 1024;
        public const int DefaultMaxEntries = 100;

        private const int BufferSize = 81920;

        private readonly long _maxEntryBytes;
        private readonly int _maxEntries;
        private long _remainingBytes;
        private int _entries;

        public AsiceReadLimits(long maxEntryBytes = DefaultMaxEntryBytes, long maxTotalBytes = DefaultMaxTotalBytes,
            int maxEntries = DefaultMaxEntries)
        {
            _maxEntryBytes = maxEntryBytes;
            _remainingBytes = maxTotalBytes;
            _maxEntries = maxEntries;
        }

        public async Task<byte[]> ReadEntryAsync(Stream entryStream)
        {
            var limit = StartEntry();
            using var target = new MemoryStream();
            var buffer = new byte[BufferSize];
            int read;
            while ((read = await entryStream.ReadAsync(buffer.AsMemory())) > 0)
            {
                Append(target, buffer, read, limit);
            }

            return Finish(target);
        }

        public byte[] ReadEntry(Stream entryStream)
        {
            var limit = StartEntry();
            using var target = new MemoryStream();
            var buffer = new byte[BufferSize];
            int read;
            while ((read = entryStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                Append(target, buffer, read, limit);
            }

            return Finish(target);
        }

        private long StartEntry()
        {
            _entries++;
            if (_entries > _maxEntries)
            {
                throw new InvalidDataException($"ASiC-E har flere enn {_maxEntries} filer");
            }

            return Math.Min(_maxEntryBytes, _remainingBytes);
        }

        private void Append(MemoryStream target, byte[] buffer, int count, long limit)
        {
            if (target.Length + count > limit)
            {
                throw new InvalidDataException(
                    $"En fil i ASiC-E er større enn {_maxEntryBytes} byte, eller filene i ASiC-E er til sammen for store");
            }

            target.Write(buffer, 0, count);
        }

        private byte[] Finish(MemoryStream target)
        {
            _remainingBytes -= target.Length;
            return target.ToArray();
        }
    }
}
