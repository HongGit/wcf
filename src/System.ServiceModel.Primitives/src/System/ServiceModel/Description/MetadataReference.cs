//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Collections.ObjectModel;
    using WsdlNS = System.Web.Services.Description;
    using System.ServiceModel.Channels;

    [XmlRoot(ElementName = MetadataStrings.MetadataExchangeStrings.MetadataReference, Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
    public class MetadataReference : IXmlSerializable
    {
        EndpointAddress _address;
        AddressingVersion _addressVersion;
        Collection<XmlAttribute> _attributes = new Collection<XmlAttribute>();
        static XmlDocument s_document = new XmlDocument();
        
        public MetadataReference()
        {
        }

        public MetadataReference(EndpointAddress address, AddressingVersion addressVersion)
        {
            this._address = address;
            this._addressVersion = addressVersion;
        }

        public EndpointAddress Address
        {
            get { return this._address; }
            set { this._address = value; }
        }

        public AddressingVersion AddressVersion
        {
            get { return this._addressVersion; }
            set { this._addressVersion = value; }
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            this._address = EndpointAddress.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader), out this._addressVersion);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (_address != null)
            {
                _address.WriteContentsTo(this._addressVersion, writer);
            }
        }
    }
}
