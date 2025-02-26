// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
//using System.ServiceModel.Configuration;
using System.Xml;

namespace System.ServiceModel.Description
{
    public abstract partial class MetadataImporter
    {
        readonly KeyedByTypeCollection<IPolicyImportExtension> _policyExtensions;
        readonly Dictionary<XmlQualifiedName, ContractDescription> _knownContracts = new();
        readonly Collection<MetadataConversionError> _errors = new Collection<MetadataConversionError>();
        readonly Dictionary<object, object> _state = new();

        //prevent inheritance until we are ready to allow it.
        internal MetadataImporter()
            : this (null, MetadataImporterQuotas.Defaults)
        {
        }

        internal MetadataImporter(IEnumerable<IPolicyImportExtension> policyImportExtensions)
            : this (policyImportExtensions, MetadataImporterQuotas.Defaults)
        {
        }

        internal MetadataImporter(IEnumerable<IPolicyImportExtension> policyImportExtensions,
            MetadataImporterQuotas quotas)
        {
            if (quotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("quotas");
            }

            if (policyImportExtensions == null)
            {
                //policyImportExtensions = LoadPolicyExtensionsFromConfig();
            }

            _quotas = quotas;
            _policyExtensions = new KeyedByTypeCollection<IPolicyImportExtension>(policyImportExtensions);
        }

        public KeyedByTypeCollection<IPolicyImportExtension> PolicyImportExtensions
        {
            get { return _policyExtensions; }
        }

        public Collection<MetadataConversionError> Errors
        {
            get { return _errors; }
        }

        public Dictionary<object, object> State
        {
            get { return _state; }
        }

        public Dictionary<XmlQualifiedName, ContractDescription> KnownContracts
        {
            get { return _knownContracts; }
        }

        // Abstract Building Methods
        public abstract Collection<ContractDescription> ImportAllContracts();
        public abstract ServiceEndpointCollection ImportAllEndpoints();

        internal virtual XmlElement ResolvePolicyReference(string policyReference, XmlElement contextAssertion)
        {
            return null;
        }
        internal BindingElementCollection ImportPolicy(ServiceEndpoint endpoint, Collection<Collection<XmlElement>> policyAlternatives)
        {
            foreach (Collection<XmlElement> selectedPolicy in policyAlternatives)
            {
                BindingOnlyPolicyConversionContext policyConversionContext = new BindingOnlyPolicyConversionContext(endpoint, selectedPolicy);

                if (TryImportPolicy(policyConversionContext))
                {
                    return policyConversionContext.BindingElements;
                }
            }
            return null;
        }

        internal bool TryImportPolicy(PolicyConversionContext policyContext)
        {
            foreach (IPolicyImportExtension policyImporter in _policyExtensions)
                try
                {
                    policyImporter.ImportPolicy(this, policyContext);
                }
//#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateExtensionException(policyImporter, e));
                }

            if (policyContext.GetBindingAssertions().Count != 0)
                return false;

            foreach (OperationDescription operation in policyContext.Contract.Operations)
            {
                if (policyContext.GetOperationBindingAssertions(operation).Count != 0)
                    return false;

                foreach (MessageDescription message in operation.Messages)
                    if (policyContext.GetMessageBindingAssertions(message).Count != 0)
                        return false;
            }

            return true;
        }

        //[Fx.Tag.SecurityNote(Critical = "uses ClientSection.UnsafeGetSection to get config in PT",
        //    Safe = "does not leak config object, just picks up extensions")]
        //[SecuritySafeCritical]
        //static Collection<IPolicyImportExtension> LoadPolicyExtensionsFromConfig()
        //{
        //    return ClientSection.UnsafeGetSection().Metadata.LoadPolicyImportExtensions();
        //}

        Exception CreateExtensionException(IPolicyImportExtension importer, Exception e)
        {
            string errorMessage = SRP.Format(SRP.PolicyExtensionImportError, importer.GetType(), e.Message);
            return new InvalidOperationException(errorMessage, e);
        }

        internal class BindingOnlyPolicyConversionContext : PolicyConversionContext
        {
            static readonly PolicyAssertionCollection s_noPolicy = new PolicyAssertionCollection();
            readonly BindingElementCollection _bindingElements = new BindingElementCollection();
            readonly PolicyAssertionCollection _bindingPolicy;

            internal BindingOnlyPolicyConversionContext(ServiceEndpoint endpoint, IEnumerable<XmlElement> bindingPolicy)
                : base(endpoint)
            {
                _bindingPolicy = new PolicyAssertionCollection(bindingPolicy);
            }

            public override BindingElementCollection BindingElements { get { return this._bindingElements; } }

            public override PolicyAssertionCollection GetBindingAssertions()
            {
                return _bindingPolicy;
            }

            public override PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation)
            {
                return s_noPolicy;
            }

            public override PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message)
            {
                return s_noPolicy;
            }

            public override PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription fault)
            {
                return s_noPolicy;
            }
        }
    }

}
