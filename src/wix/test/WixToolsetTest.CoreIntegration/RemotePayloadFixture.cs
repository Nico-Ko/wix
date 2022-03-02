// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System.IO;
    using System.Linq;
    using WixBuildTools.TestSupport;
    using WixToolset.Core.TestPackage;
    using Xunit;

    public class RemotePayloadFixture
    {
        [Fact]
        public void CanGetRemotePayload()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var outputFolder = fs.GetFolder();
                var outFile = Path.Combine(outputFolder, "out.xml");

                var result = WixRunner.Execute(new[]
                {
                    "burn", "remotepayload",
                    Path.Combine(folder, ".Data", "burn.exe"),
                    "-downloadurl", "https://www.example.com/files/{0}",
                    "-o", outFile
                });

                result.AssertSuccess();

                var elements = File.ReadAllLines(outFile);
                elements = elements.Select(s => s.Replace("\"", "'")).ToArray();

                WixAssert.CompareLineByLine(new[]
                {
                    @"<ExePackagePayload Name='burn.exe' ProductName='Windows Installer XML Toolset' Description='WiX Toolset Bootstrapper' DownloadUrl='https://www.example.com/files/burn.exe' Hash='F6E722518AC3AB7E31C70099368D5770788C179AA23226110DCF07319B1E1964E246A1E8AE72E2CF23E0138AFC281BAFDE45969204405E114EB20C8195DA7E5E' Size='463360' Version='3.14.1703.0' />",
                }, elements);
            }
        }

        [Fact]
        public void CanGetRemoteMsuPayload()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var outputFolder = fs.GetFolder();
                var outFile = Path.Combine(outputFolder, "out.xml");

                var result = WixRunner.Execute(new[]
                {
                    "burn", "remotepayload",
                    Path.Combine(folder, ".Data", "Windows8.1-KB2937592-x86.msu"),
                    "-o", outFile
                });

                result.AssertSuccess();

                var elements = File.ReadAllLines(outFile);
                elements = elements.Select(s => s.Replace("\"", "'")).ToArray();

                WixAssert.CompareLineByLine(new[]
                {
                    @"<MsuPackagePayload Name='Windows8.1-KB2937592-x86.msu' Hash='904ADEA6AB675ACE16483138BF3F5850FD56ACB6E3A13AFA7263ED49C68CCE6CF84D6AAD6F99AAF175A95EE1A56C787C5AD968019056490B1073E7DBB7B9B7BE' Size='309544' />",
                }, elements);
            }
        }

        [Fact]
        public void CanGetRemotePayloadWithCertificate()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var outputFolder = fs.GetFolder();
                var outFile = Path.Combine(outputFolder, "out.xml");

                var result = WixRunner.Execute(new[]
                {
                    "burn", "remotepayload",
                    "-usecertificate",
                    Path.Combine(folder, ".Data", "burn.exe"),
                    Path.Combine(folder, ".Data", "signed_cab1.cab"),
                    "-o", outFile
                });

                result.AssertSuccess();

                var elements = File.ReadAllLines(outFile);
                elements = elements.Select(s => s.Replace("\"", "'")).ToArray();

                WixAssert.CompareLineByLine(new[]
                {
                    @"<ExePackagePayload Name='burn.exe' ProductName='Windows Installer XML Toolset' Description='WiX Toolset Bootstrapper' Hash='F6E722518AC3AB7E31C70099368D5770788C179AA23226110DCF07319B1E1964E246A1E8AE72E2CF23E0138AFC281BAFDE45969204405E114EB20C8195DA7E5E' Size='463360' Version='3.14.1703.0' />",
                    @"<Payload Name='signed_cab1.cab' CertificatePublicKey='BBD1B48A37503767C71F455624967D406A5D66C3' CertificateThumbprint='DE13B4CE635E3F63AA2394E66F95C460267BC82F' Hash='D8D3842403710E1F6036A62543224855CADF546853933C2B17BA99D789D4347B36717687C022678A9D3DE749DFC1482DAAB92B997B62BB32A8A6828B9D04C414' Size='1585' />",
                }, elements);
            }
        }

        [Fact]
        public void CanGetRemotePayloadWithoutCertificate()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var outputFolder = fs.GetFolder();
                var outFile = Path.Combine(outputFolder, "out.xml");

                var result = WixRunner.Execute(new[]
                {
                    "burn", "remotepayload",
                    Path.Combine(folder, ".Data", "burn.exe"),
                    Path.Combine(folder, ".Data", "signed_cab1.cab"),
                    "-o", outFile
                });

                result.AssertSuccess();

                var elements = File.ReadAllLines(outFile);
                elements = elements.Select(s => s.Replace("\"", "'")).ToArray();

                WixAssert.CompareLineByLine(new[]
                {
                    @"<ExePackagePayload Name='burn.exe' ProductName='Windows Installer XML Toolset' Description='WiX Toolset Bootstrapper' Hash='F6E722518AC3AB7E31C70099368D5770788C179AA23226110DCF07319B1E1964E246A1E8AE72E2CF23E0138AFC281BAFDE45969204405E114EB20C8195DA7E5E' Size='463360' Version='3.14.1703.0' />",
                    @"<Payload Name='signed_cab1.cab' Hash='D8D3842403710E1F6036A62543224855CADF546853933C2B17BA99D789D4347B36717687C022678A9D3DE749DFC1482DAAB92B997B62BB32A8A6828B9D04C414' Size='1585' />",
                }, elements);
            }
        }

        [Fact]
        public void CanGetRemotePayloadsRecursive()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var outputFolder = fs.GetFolder();
                var outFile = Path.Combine(outputFolder, "out.xml");

                var result = WixRunner.Execute(new[]
                {
                    "burn", "remotepayload",
                    "-recurse",
                    "-du", "https://www.example.com/files/{0}",
                    Path.Combine(folder, ".Data", "burn.exe"),
                    Path.Combine(folder, "RemotePayload", "*"),
                    "-basepath", folder,
                    "-bp", Path.Combine(folder, ".Data"),
                    "-o", outFile
                });

                result.AssertSuccess();

                var elements = File.ReadAllLines(outFile);
                elements = elements.Select(s => s.Replace("\"", "'")).ToArray();

                WixAssert.CompareLineByLine(new[]
                {
                    @"<ExePackagePayload Name='burn.exe' ProductName='Windows Installer XML Toolset' Description='WiX Toolset Bootstrapper' DownloadUrl='https://www.example.com/files/burn.exe' Hash='F6E722518AC3AB7E31C70099368D5770788C179AA23226110DCF07319B1E1964E246A1E8AE72E2CF23E0138AFC281BAFDE45969204405E114EB20C8195DA7E5E' Size='463360' Version='3.14.1703.0' />",
                    @"<Payload Name='a.dat' DownloadUrl='https://www.example.com/files/RemotePayload/a.dat' Hash='D13926E5CBE5ED8B46133F9199FAF2FF25B25981C67A31AE2BC3F6C20390FACBFADCD89BD22D3445D95B989C8EACFB1E68DB634BECB5C9624865BA453BCE362A' Size='16' />",
                    @"<Payload Name='b.dat' DownloadUrl='https://www.example.com/files/RemotePayload/subfolder/b.dat' Hash='5F94707BC29ADFE3B9615E6753388707FD0B8F5FD9EEEC2B17E21E72F1635FF7D7A101E7D14F614E111F263CB9AC4D0940BE1247881A7844F226D6C400293D8E' Size='37' />",
                    @"<Payload Name='c.dat' DownloadUrl='https://www.example.com/files/RemotePayload/subfolder/c.dat' Hash='97D6209A5571E05E4F72F9C6BF0987651FA03E63F971F9B53C2B3D798A666D9864F232D4E2D6442E47D9D72B282309B6EEFF4EE017B43B706FA92A0F5EF74734' Size='42' />",
                }, elements);
            }
        }
    }
}