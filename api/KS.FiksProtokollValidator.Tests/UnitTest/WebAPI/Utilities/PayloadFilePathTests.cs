using System.IO;
using KS.FiksProtokollValidator.WebAPI.Utilities;
using Xunit;

namespace KS.FiksProtokollValidator.Tests.UnitTest.WebAPI.Utilities
{
    public class PayloadFilePathTests
    {
        private static readonly string BaseDirectory = Path.Combine(Path.GetTempPath(), "validator");

        [Theory]
        [InlineData("Samples/Fiks.Plan.V2/payload.xml")]
        [InlineData("TestCases/no.ks.fiks.arkiv.v1/NyJournalpostN1/arkivmelding.xml")]
        [InlineData("TestCases/no.ks.fiks.arkiv.v1/NyJournalpostN1/Attachments/vedlegg.pdf")]
        public void Resolve_FileInsideAllowedDirectory_ReturnsFullPath(string relativeFilePath)
        {
            var fullPath = PayloadFilePath.Resolve(BaseDirectory, relativeFilePath);

            Assert.Equal(Path.GetFullPath(Path.Combine(BaseDirectory, relativeFilePath)), fullPath);
        }

        [Theory]
        [InlineData("Samples/../appsettings.json")]
        [InlineData("TestCases/../../outside.txt")]
        [InlineData("../outside.txt")]
        [InlineData("appsettings.json")]
        [InlineData("SamplesOther/payload.xml")]
        [InlineData("/etc/hosts")]
        [InlineData("")]
        [InlineData(null)]
        public void Resolve_FileOutsideAllowedDirectories_ReturnsNull(string relativeFilePath)
        {
            Assert.Null(PayloadFilePath.Resolve(BaseDirectory, relativeFilePath));
        }

        [Theory]
        [InlineData("vedlegg.pdf")]
        [InlineData("rekvisisjon med mellomrom.pdf")]
        public void IsFileName_PlainFileName_ReturnsTrue(string fileName)
        {
            Assert.True(PayloadFilePath.IsFileName(fileName));
        }

        [Theory]
        [InlineData("../vedlegg.pdf")]
        [InlineData("../../arkivmelding.xml")]
        [InlineData("Attachments/vedlegg.pdf")]
        [InlineData("/etc/hosts")]
        [InlineData("..")]
        [InlineData(".")]
        [InlineData("")]
        [InlineData(null)]
        public void IsFileName_PathOrSpecialName_ReturnsFalse(string fileName)
        {
            Assert.False(PayloadFilePath.IsFileName(fileName));
        }
    }
}
