using System;
using System.IO;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Utilities;
using Xunit;

namespace KS.FiksProtokollValidator.Tests.UnitTest.WebAPI.Utilities
{
    public class AsiceReadLimitsTests
    {
        private static byte[] RandomBytes(int length)
        {
            var bytes = new byte[length];
            new Random(42).NextBytes(bytes);
            return bytes;
        }

        [Fact]
        public void ReadEntry_AtEntryLimit_ReturnsAllBytes()
        {
            var data = RandomBytes(200_000);

            var bytes = new AsiceReadLimits(maxEntryBytes: data.Length).ReadEntry(new MemoryStream(data));

            Assert.Equal(data, bytes);
        }

        [Fact]
        public async Task ReadEntryAsync_AtEntryLimit_ReturnsAllBytes()
        {
            var data = RandomBytes(200_000);

            var bytes = await new AsiceReadLimits(maxEntryBytes: data.Length).ReadEntryAsync(new MemoryStream(data));

            Assert.Equal(data, bytes);
        }

        [Fact]
        public void ReadEntry_OverEntryLimit_Throws()
        {
            var limits = new AsiceReadLimits(maxEntryBytes: 999);

            Assert.Throws<InvalidDataException>(() => limits.ReadEntry(new MemoryStream(new byte[1000])));
        }

        [Fact]
        public async Task ReadEntryAsync_OverEntryLimit_Throws()
        {
            var limits = new AsiceReadLimits(maxEntryBytes: 999);

            await Assert.ThrowsAsync<InvalidDataException>(() => limits.ReadEntryAsync(new MemoryStream(new byte[1000])));
        }

        [Fact]
        public async Task ReadEntryAsync_OverTotalLimit_Throws()
        {
            var limits = new AsiceReadLimits(maxEntryBytes: 1000, maxTotalBytes: 1500);
            await limits.ReadEntryAsync(new MemoryStream(new byte[1000]));

            await Assert.ThrowsAsync<InvalidDataException>(() => limits.ReadEntryAsync(new MemoryStream(new byte[1000])));
        }

        [Fact]
        public void ReadEntry_MoreEntriesThanLimit_Throws()
        {
            var limits = new AsiceReadLimits(maxEntries: 2);
            limits.ReadEntry(new MemoryStream(new byte[1]));
            limits.ReadEntry(new MemoryStream(new byte[1]));

            Assert.Throws<InvalidDataException>(() => limits.ReadEntry(new MemoryStream(new byte[1])));
        }

        [Fact]
        public void ReadEntry_EmptyEntry_ReturnsEmptyArray()
        {
            Assert.Empty(new AsiceReadLimits().ReadEntry(new MemoryStream()));
        }
    }
}
