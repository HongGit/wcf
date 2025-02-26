// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml.Schema;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using WsdlNS = System.Web.Services.Description;

namespace System.ServiceModel.Channels
{
    // implemented by Indigo Transports

    public class TcpTransportBindingElementImporter : TransportBindingElementImporter
    {
        static TransportBindingElement CreateTransportBindingElements(string transportUri, PolicyConversionContext policyContext)
        {
            TransportBindingElement transportBindingElement = null;
            // Try and Create TransportBindingElement
            switch (transportUri)
            {
                case TransportPolicyConstants.TcpTransportUri:
                    transportBindingElement = new TcpTransportBindingElement();
                    break;
                default:
                    // There may be another registered converter that can handle this transport.
                    break;
            }

            return transportBindingElement;
        }
    }

}
