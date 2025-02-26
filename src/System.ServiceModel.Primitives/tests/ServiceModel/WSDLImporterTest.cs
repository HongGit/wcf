// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using Infrastructure.Common;
using Xunit;
using System.Threading.Tasks;

public class WsdlImporterTest
{
    [WcfFact]
    public static void WSDLImporter_Import_ValidWsdl()
    {
        WsdlImporter _wsdlImporter = new WsdlImporter();
        try
        {
            // Arrange
            var wsdlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "valid.wsdl");

            // Act
            var result = _wsdlImporter.Import(wsdlFilePath);

            // Assert
            Assert.IsTrue(result);
        }
        finally
        {
            _wsdlImporter.Dispose();
        }
    }

}
