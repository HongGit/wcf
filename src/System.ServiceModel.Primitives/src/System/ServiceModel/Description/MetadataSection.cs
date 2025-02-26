//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.Xml;
    using WsdlNS = System.Web.Services.Description;
    using XsdNS = System.Xml.Schema;
    using System.Reflection;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = MetadataStrings.MetadataExchangeStrings.MetadataSection, Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
    public class MetadataSection
    {
        Collection<XmlAttribute> _attributes = new Collection<XmlAttribute>();
        string _dialect;
        string _identifier;
        object _metadata;
        string _sourceUrl;
        static XmlDocument s_xmlDocument = new XmlDocument();

        public MetadataSection()
            : this(null, null, null)
        {
        }

        public MetadataSection(string dialect, string identifier, object metadata)
        {
            this._dialect = dialect;
            this._identifier = identifier;
            this._metadata = metadata;
        }

        static public string ServiceDescriptionDialect { get { return System.Web.Services.Description.ServiceDescription.Namespace; } }
        static public string XmlSchemaDialect { get { return System.Xml.Schema.XmlSchema.Namespace; } }
        static public string PolicyDialect { get { return MetadataStrings.WSPolicy.NamespaceUri; } }
        static public string MetadataExchangeDialect { get { return MetadataStrings.MetadataExchangeStrings.Namespace; } }

        [XmlAnyAttribute]
        public Collection<XmlAttribute> Attributes
        {
            get { return _attributes; }
        }

        [XmlAttribute]
        public string Dialect
        {
            get { return this._dialect; }
            set { this._dialect = value; }
        }

        [XmlAttribute]
        public string Identifier
        {
            get { return this._identifier; }
            set { this._identifier = value; }
        }

        [XmlAnyElement]
        [XmlElement(MetadataStrings.XmlSchema.Schema, typeof(XsdNS.XmlSchema), Namespace = XsdNS.XmlSchema.Namespace)]
        //typeof(WsdlNS.ServiceDescription) produces an XmlSerializer which can't export / import the Extensions in the ServiceDescription.  
        //We use change this to typeof(string) and then fix the generated serializer to use the Read/Write 
        //methods provided by WsdlNS.ServiceDesciption which use a pregenerated serializer which can export / import the Extensions.
        [XmlElement(MetadataStrings.ServiceDescription.Definitions, typeof(WsdlNS.ServiceDescription), Namespace = WsdlNS.ServiceDescription.Namespace)]
        [XmlElement(MetadataStrings.MetadataExchangeStrings.MetadataReference, typeof(MetadataReference), Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
        [XmlElement(MetadataStrings.MetadataExchangeStrings.Location, typeof(MetadataLocation), Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
        [XmlElement(MetadataStrings.MetadataExchangeStrings.Metadata, typeof(MetadataSet), Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
        public object Metadata
        {
            get { return this._metadata; }
            set { this._metadata = value; }
        }

        internal string SourceUrl
        {
            get { return _sourceUrl; }
            set { _sourceUrl = value; }
        }

        public static MetadataSection CreateFromPolicy(XmlElement policy, string identifier)
        {
            if (policy == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policy");
            }

            if (!IsPolicyElement(policy))
            {
                //TODO
//                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("policy",
//#pragma warning suppress 56506 // chiomaos, policy cannot be null at this point since it has been validated above.
// SR.GetString(SR.SFxBadMetadataMustBePolicy, MetadataStrings.WSPolicy.NamespaceUri, MetadataStrings.WSPolicy.Elements.Policy, policy.NamespaceURI, policy.LocalName));
            }

            MetadataSection section = new MetadataSection();

            section.Dialect = policy.NamespaceURI;
            section.Identifier = identifier;
            section.Metadata = policy;

            return section;
        }
        public static MetadataSection CreateFromSchema(XsdNS.XmlSchema schema)
        {
            if (schema == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schema");
            }

            MetadataSection section = new MetadataSection();

            section.Dialect = MetadataSection.XmlSchemaDialect;
            section.Identifier = schema.TargetNamespace;
            section.Metadata = schema;

            return section;
        }
        public static MetadataSection CreateFromServiceDescription(WsdlNS.ServiceDescription serviceDescription)
        {
            if (serviceDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceDescription");
            }

            MetadataSection section = new MetadataSection();

            section.Dialect = MetadataSection.ServiceDescriptionDialect;
            section.Identifier = serviceDescription.TargetNamespace;
            section.Metadata = serviceDescription;

            return section;
        }

        internal static bool IsPolicyElement(XmlElement policy)
        {
            return (policy.NamespaceURI == MetadataStrings.WSPolicy.NamespaceUri
                || policy.NamespaceURI == MetadataStrings.WSPolicy.NamespaceUri15)
                && policy.LocalName == MetadataStrings.WSPolicy.Elements.Policy;
        }

    }
}
