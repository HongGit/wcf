// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml;
using System.Xml.Schema;
using WsdlNS = System.Web.Services.Description;

namespace System.ServiceModel.Channels
{
    // implemented by Indigo Transports
    public class HttpTransportBindingElementImporter : TransportBindingElementImporter
    {
        static void ImportAddress(WsdlEndpointConversionContext context, TransportBindingElement transportBindingElement)
        {
            EndpointAddress address = context.Endpoint.Address = WsdlImporter.WSAddressingHelper.ImportAddress(context.WsdlPort);
            if (address != null)
            {
                context.Endpoint.Address = address;

                // Replace the http BE with https BE only if the uri scheme is https and the transport binding element is a HttpTransportBindingElement but not HttpsTransportBindingElement
                if (address.Uri.Scheme == Uri.UriSchemeHttps && transportBindingElement is HttpTransportBindingElement && !(transportBindingElement is HttpsTransportBindingElement))
                {
                    BindingElementCollection elements = ConvertToCustomBinding(context).Elements;
                    elements.Remove(transportBindingElement);
                    elements.Add(CreateHttpsFromHttp(transportBindingElement as HttpTransportBindingElement));
                }
            }
        }
        static HttpsTransportBindingElement CreateHttpsFromHttp(HttpTransportBindingElement http)
        {
            if (http == null) return new HttpsTransportBindingElement();

            HttpsTransportBindingElement https = HttpsTransportBindingElement.CreateFromHttpBindingElement(http);

            return https;
        }

        static TransportBindingElement CreateTransportBindingElements(string transportUri, PolicyConversionContext policyContext)
        {
            TransportBindingElement transportBindingElement = null;
            // Try and Create TransportBindingElement
            switch (transportUri)
            {
                case TransportPolicyConstants.HttpTransportUri:
                    transportBindingElement = GetHttpTransportBindingElement(policyContext);
                    break;
                case TransportPolicyConstants.WebSocketTransportUri:
                    HttpTransportBindingElement httpTransport = GetHttpTransportBindingElement(policyContext);
                    httpTransport.WebSocketSettings.TransportUsage = WebSocketTransportUsage.Always;
                    httpTransport.WebSocketSettings.SubProtocol = WebSocketTransportSettings.SoapSubProtocol;
                    transportBindingElement = httpTransport;
                    break;
                default:
                    // There may be another registered converter that can handle this transport.
                    break;
            }

            return transportBindingElement;
        }

        static HttpTransportBindingElement GetHttpTransportBindingElement(PolicyConversionContext policyContext)
        {
            if (policyContext != null)
            {
                ICollection<XmlElement> policyCollection = policyContext.GetBindingAssertions();
                // if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyCollection, out sp) && sp.ContainsWsspHttpsTokenAssertion(policyCollection))
                // {
                //     HttpsTransportBindingElement httpsBinding = new HttpsTransportBindingElement();
                //     httpsBinding.MessageSecurityVersion = sp.GetSupportedMessageSecurityVersion(SecurityVersion.WSSecurity11);
                //     return httpsBinding;
                // }
    
                foreach (XmlElement element in policyCollection)
                {
                    if (element.LocalName == WSSecurityPolicy.HttpsTokenName)
                    {
                        if (element.NamespaceURI == @"http://schemas.xmlsoap.org/ws/2005/07/securitypolicy" || element.NamespaceURI == WSSecurityPolicy.MsspNamespace)
                        { 
                            HttpsTransportBindingElement httpsElem = new HttpsTransportBindingElement();
                            httpsElem.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
                            return httpsElem;
                        }
                        //WSSecurityPolicy12 URI
                        else if (element.NamespaceURI == @"http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200702" || element.NamespaceURI == WSSecurityPolicy.MsspNamespace)
                        {
                            HttpsTransportBindingElement httpsElem = new HttpsTransportBindingElement();
                            httpsElem.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
                            return httpsElem;
                        }
                    }                       
                }
            }

            return new HttpTransportBindingElement();
        }
    }
}
