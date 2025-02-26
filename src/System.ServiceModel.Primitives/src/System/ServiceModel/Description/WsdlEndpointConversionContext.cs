//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.IO;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using WsdlNS = System.Web.Services.Description;

    public class WsdlEndpointConversionContext
    {

        readonly ServiceEndpoint _endpoint;
        readonly WsdlNS.Binding _wsdlBinding;
        readonly WsdlNS.Port _wsdlPort;
        readonly WsdlContractConversionContext _contractContext;

        readonly Dictionary<OperationDescription, WsdlNS.OperationBinding> _wsdlOperationBindings;
        readonly Dictionary<WsdlNS.OperationBinding, OperationDescription> _operationDescriptionBindings;
        readonly Dictionary<MessageDescription, WsdlNS.MessageBinding> _wsdlMessageBindings;
        readonly Dictionary<FaultDescription, WsdlNS.FaultBinding> _wsdlFaultBindings;
        readonly Dictionary<WsdlNS.MessageBinding, MessageDescription> _messageDescriptionBindings;
        readonly Dictionary<WsdlNS.FaultBinding, FaultDescription> _faultDescriptionBindings;
        
        internal WsdlEndpointConversionContext(WsdlContractConversionContext contractContext, ServiceEndpoint endpoint, WsdlNS.Binding wsdlBinding, WsdlNS.Port wsdlport)
        {

            this._endpoint = endpoint;
            this._wsdlBinding = wsdlBinding;
            this._wsdlPort = wsdlport;
            this._contractContext = contractContext;

            this._wsdlOperationBindings = new Dictionary<OperationDescription, WsdlNS.OperationBinding>();
            this._operationDescriptionBindings = new Dictionary<WsdlNS.OperationBinding, OperationDescription>();
            this._wsdlMessageBindings = new Dictionary<MessageDescription, WsdlNS.MessageBinding>();
            this._messageDescriptionBindings = new Dictionary<WsdlNS.MessageBinding, MessageDescription>();
            this._wsdlFaultBindings = new Dictionary<FaultDescription, WsdlNS.FaultBinding>();
            this._faultDescriptionBindings = new Dictionary<WsdlNS.FaultBinding, FaultDescription>();
        }

        internal WsdlEndpointConversionContext(WsdlEndpointConversionContext bindingContext, ServiceEndpoint endpoint, WsdlNS.Port wsdlport)
        {

            this._endpoint = endpoint;
            this._wsdlBinding = bindingContext.WsdlBinding;
            this._wsdlPort = wsdlport;
            this._contractContext = bindingContext._contractContext;

            this._wsdlOperationBindings = bindingContext._wsdlOperationBindings;
            this._operationDescriptionBindings = bindingContext._operationDescriptionBindings;
            this._wsdlMessageBindings = bindingContext._wsdlMessageBindings;
            this._messageDescriptionBindings = bindingContext._messageDescriptionBindings;
            this._wsdlFaultBindings = bindingContext._wsdlFaultBindings;
            this._faultDescriptionBindings = bindingContext._faultDescriptionBindings;
        }

        //internal IEnumerable<IWsdlExportExtension> ExportExtensions
        //{
        //    get
        //    {
        //        foreach (IWsdlExportExtension extension in _endpoint.Behaviors.FindAll<IWsdlExportExtension>())
        //        {
        //            yield return extension;
        //        }

        //        foreach (IWsdlExportExtension extension in _endpoint.Binding.CreateBindingElements().FindAll<IWsdlExportExtension>())
        //        {
        //            yield return extension;
        //        }

        //        foreach (IWsdlExportExtension extension in _endpoint.Contract.Behaviors.FindAll<IWsdlExportExtension>())
        //        {
        //            yield return extension;
        //        }

        //        foreach (OperationDescription operation in _endpoint.Contract.Operations)
        //        {
        //            if (!WsdlExporter.OperationIsExportable(operation))
        //            {
        //                continue;
        //            }

        //            // In 3.0SP1, the DCSOB and XSOB were moved from before to after the custom behaviors.  For
        //            // IWsdlExportExtension compat, run them in the pre-SP1 order.
        //            // TEF QFE 367607
        //            Collection<IWsdlExportExtension> extensions = operation.Behaviors.FindAll<IWsdlExportExtension>();
        //            for (int i = 0; i < extensions.Count;)
        //            {
        //                if (WsdlExporter.IsBuiltInOperationBehavior(extensions[i]))
        //                {
        //                    yield return extensions[i];
        //                    extensions.RemoveAt(i);
        //                }
        //                else
        //                {
        //                    i++;
        //                }
        //            }
        //            foreach (IWsdlExportExtension extension in extensions)
        //            {
        //                yield return extension;
        //            }
        //        }
        //    }
        //}

        public ServiceEndpoint Endpoint { get { return _endpoint; } }
        public WsdlNS.Binding WsdlBinding { get { return _wsdlBinding; } }
        public WsdlNS.Port WsdlPort { get { return _wsdlPort; } }
        public WsdlContractConversionContext ContractConversionContext { get { return _contractContext; } }

        public WsdlNS.OperationBinding GetOperationBinding(OperationDescription operation)
        {
            return this._wsdlOperationBindings[operation];
        }

        public WsdlNS.MessageBinding GetMessageBinding(MessageDescription message)
        {
            return this._wsdlMessageBindings[message];
        }

        public WsdlNS.FaultBinding GetFaultBinding(FaultDescription fault)
        {
            return this._wsdlFaultBindings[fault];
        }

        public OperationDescription GetOperationDescription(WsdlNS.OperationBinding operationBinding)
        {
            return this._operationDescriptionBindings[operationBinding];
        }

        public MessageDescription GetMessageDescription(WsdlNS.MessageBinding messageBinding)
        {
            return this._messageDescriptionBindings[messageBinding];
        }

        public FaultDescription GetFaultDescription(WsdlNS.FaultBinding faultBinding)
        {
            return this._faultDescriptionBindings[faultBinding];
        }

        // --------------------------------------------------------------------------------------------------

        internal void AddOperationBinding(OperationDescription operationDescription, WsdlNS.OperationBinding wsdlOperationBinding)
        {
            this._wsdlOperationBindings.Add(operationDescription, wsdlOperationBinding);
            this._operationDescriptionBindings.Add(wsdlOperationBinding, operationDescription);
        }

        internal void AddMessageBinding(MessageDescription messageDescription, WsdlNS.MessageBinding wsdlMessageBinding)
        {
            this._wsdlMessageBindings.Add(messageDescription, wsdlMessageBinding);
            this._messageDescriptionBindings.Add(wsdlMessageBinding, messageDescription);
        }

        internal void AddFaultBinding(FaultDescription faultDescription, WsdlNS.FaultBinding wsdlFaultBinding)
        {
            this._wsdlFaultBindings.Add(faultDescription, wsdlFaultBinding);
            this._faultDescriptionBindings.Add(wsdlFaultBinding, faultDescription);
        }
    }

}
