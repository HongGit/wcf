//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Xml;

    internal class UnrecognizedAssertionsBindingElement : BindingElement
    {
        XmlQualifiedName _wsdlBinding;
        ICollection<XmlElement> _bindingAsserions;
        IDictionary<OperationDescription, ICollection<XmlElement>> _operationAssertions;
        IDictionary<MessageDescription, ICollection<XmlElement>> _messageAssertions;

        internal protected UnrecognizedAssertionsBindingElement(XmlQualifiedName wsdlBinding, ICollection<XmlElement> bindingAsserions)
        {
            Fx.Assert(wsdlBinding != null, "");
            this._wsdlBinding = wsdlBinding;
            this._bindingAsserions = bindingAsserions;
        }

        internal XmlQualifiedName WsdlBinding
        {
            get { return this._wsdlBinding; }
        }

        internal ICollection<XmlElement> BindingAsserions
        {
            get
            {
                if (this._bindingAsserions == null)
                    this._bindingAsserions = new Collection<XmlElement>();
                return this._bindingAsserions;
            }
        }

        internal IDictionary<OperationDescription, ICollection<XmlElement>> OperationAssertions
        {
            get
            {
                if (this._operationAssertions == null)
                    this._operationAssertions = new Dictionary<OperationDescription, ICollection<XmlElement>>();
                return this._operationAssertions;
            }
        }

        internal IDictionary<MessageDescription, ICollection<XmlElement>> MessageAssertions
        {
            get
            {
                if (this._messageAssertions == null)
                    this._messageAssertions = new Dictionary<MessageDescription, ICollection<XmlElement>>();
                return this._messageAssertions;
            }
        }

        internal void Add(OperationDescription operation, ICollection<XmlElement> assertions)
        {
            ICollection<XmlElement> existent;
            if (!OperationAssertions.TryGetValue(operation, out existent))
            {
                OperationAssertions.Add(operation, assertions);
            }
            else
            {
                foreach (XmlElement assertion in assertions)
                    existent.Add(assertion);
            }
        }

        internal void Add(MessageDescription message, ICollection<XmlElement> assertions)
        {
            ICollection<XmlElement> existent;
            if (!MessageAssertions.TryGetValue(message, out existent))
            {
                MessageAssertions.Add(message, assertions);
            }
            else
            {
                foreach (XmlElement assertion in assertions)
                    existent.Add(assertion);
            }
        }

        protected UnrecognizedAssertionsBindingElement(UnrecognizedAssertionsBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this._wsdlBinding = elementToBeCloned._wsdlBinding;
            this._bindingAsserions = elementToBeCloned._bindingAsserions;
            this._operationAssertions = elementToBeCloned._operationAssertions;
            this._messageAssertions = elementToBeCloned._messageAssertions;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.GetInnerProperty<T>();
        }

        public override BindingElement Clone()
        {
            //throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedBindingElementClone, typeof(UnrecognizedAssertionsBindingElement).Name)));
            // do not allow Cloning, return an empty BindingElement
            return new UnrecognizedAssertionsBindingElement(new XmlQualifiedName(_wsdlBinding.Name, _wsdlBinding.Namespace), null);
        }
    }
}

