// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using WsdlNS = System.Web.Services.Description;
using System.Globalization;

namespace System.ServiceModel.Description
{
    //
    // PolicyReader is a complex nested class in the MetadataImporter
    //
    public abstract partial class MetadataImporter
    {
        internal MetadataImporterQuotas _quotas;

        PolicyReader _policyNormalizer = null;
        
        internal delegate void PolicyWarningHandler(XmlElement contextAssertion, string warningMessage);
        
        // Consider, Microsoft, make this public?
        internal event PolicyWarningHandler PolicyWarningOccured;

        internal IEnumerable<IEnumerable<XmlElement>> NormalizePolicy(IEnumerable<XmlElement> policyAssertions)
        {
            if (this._policyNormalizer == null)
            {
                this._policyNormalizer = new PolicyReader(this);
            }

            return this._policyNormalizer.NormalizePolicy(policyAssertions);
        }

        //DevNote: The error handling goal for this class is to NEVER throw an exception.
        //  * Any Ignored Policy should generate a warning
        //  * All policy parsing errors should be logged as warnings in the WSDLImporter.Errors collection.
        sealed class PolicyReader
        {
            int _nodesRead = 0;
            
            readonly MetadataImporter _metadataImporter;

            internal PolicyReader(MetadataImporter metadataImporter)
            {
                this._metadataImporter = metadataImporter;
            }

            static IEnumerable<XmlElement> s_empty = new PolicyHelper.EmptyEnumerable<XmlElement>();
            static IEnumerable<IEnumerable<XmlElement>> s_emptyEmpty = new PolicyHelper.SingleEnumerable<IEnumerable<XmlElement>>(new PolicyHelper.EmptyEnumerable<XmlElement>());

            //
            // the core policy reading logic
            // each step returns a list of lists -- an "and of ors": 
            // each inner list is a policy alternative: it contains the set of assertions that comprise the alternative
            // the outer list represents the choice between alternatives
            //

            IEnumerable<IEnumerable<XmlElement>> ReadNode(XmlNode node, XmlElement contextAssertion, YieldLimiter yieldLimiter)
            {
                if (_nodesRead >= this._metadataImporter._quotas.MaxPolicyNodes)
                {
                    if (_nodesRead == this._metadataImporter._quotas.MaxPolicyNodes)
                    {
                        // add wirning once
                        string warningMsg = SRP.Format(SRP.ExceededMaxPolicyComplexity, node.Name, PolicyHelper.GetFragmentIdentifier((XmlElement)node));
                        _metadataImporter.PolicyWarningOccured.Invoke(contextAssertion, warningMsg);
                        _nodesRead++;
                    }
                    return s_emptyEmpty;
                }
                _nodesRead++;
                IEnumerable<IEnumerable<XmlElement>> nodes = s_emptyEmpty;
                switch (PolicyHelper.GetNodeType(node))
                {
                    case PolicyHelper.NodeType.Policy:
                    case PolicyHelper.NodeType.All:
                        nodes = ReadNode_PolicyOrAll((XmlElement)node, contextAssertion, yieldLimiter);
                        break;
                    case PolicyHelper.NodeType.ExactlyOne:
                        nodes = ReadNode_ExactlyOne((XmlElement)node, contextAssertion, yieldLimiter);
                        break;
                    case PolicyHelper.NodeType.Assertion:
                        nodes = ReadNode_Assertion((XmlElement)node, yieldLimiter);
                        break;
                    case PolicyHelper.NodeType.PolicyReference:
                        nodes = ReadNode_PolicyReference((XmlElement)node, contextAssertion, yieldLimiter);
                        break;
                    case PolicyHelper.NodeType.UnrecognizedWSPolicy:
                        string warningMsg = SRP.Format(SRP.UnrecognizedPolicyElementInNamespace, node.Name, node.NamespaceURI);
                        _metadataImporter.PolicyWarningOccured.Invoke(contextAssertion, warningMsg);
                        break;
                    //consider Microsoft, add more error handling here. default?
                }
                return nodes;
            }

            IEnumerable<IEnumerable<XmlElement>> ReadNode_PolicyReference(XmlElement element, XmlElement contextAssertion, YieldLimiter yieldLimiter)
            {
                string idRef = element.GetAttribute(MetadataStrings.WSPolicy.Attributes.URI);
                if (idRef == null)
                {
                    string warningMsg = SRP.Format(SRP.PolicyReferenceMissingURI, MetadataStrings.WSPolicy.Attributes.URI);
                    _metadataImporter.PolicyWarningOccured.Invoke(contextAssertion, warningMsg);
                    return s_emptyEmpty;
                }
                else if (idRef == string.Empty)
                {
                    string warningMsg = SRP.Format(SRP.PolicyReferenceInvalidId);
                    _metadataImporter.PolicyWarningOccured.Invoke(contextAssertion, warningMsg);
                    return s_emptyEmpty;
                }

                XmlElement policy = _metadataImporter.ResolvePolicyReference(idRef, contextAssertion);
                if (policy == null)
                {
                    string warningMsg = SRP.Format(SRP.UnableToFindPolicyWithId, idRef);
                    _metadataImporter.PolicyWarningOccured.Invoke(contextAssertion, warningMsg);
                    return s_emptyEmpty;
                }

                //
                // Since we looked up a reference, the context assertion changes.
                //
                return ReadNode_PolicyOrAll(policy, policy, yieldLimiter);

            }

            IEnumerable<IEnumerable<XmlElement>> ReadNode_Assertion(XmlElement element, YieldLimiter yieldLimiter)
            {
                if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    yield return s_empty;
                else
                    yield return new PolicyHelper.SingleEnumerable<XmlElement>(element);
            }

            IEnumerable<IEnumerable<XmlElement>> ReadNode_ExactlyOne(XmlElement element, XmlElement contextAssertion, YieldLimiter yieldLimiter)
            {
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        foreach (IEnumerable<XmlElement> alternative in ReadNode(child, contextAssertion, yieldLimiter))
                        {
                            if (yieldLimiter.IncrementAndLogIfExceededLimit())
                            {
                                yield break;
                            }
                            else
                            {
                                yield return alternative;
                            }
                        }
                    }
                }
            }

            IEnumerable<IEnumerable<XmlElement>> ReadNode_PolicyOrAll(XmlElement element, XmlElement contextAssertion, YieldLimiter yieldLimiter)
            {
                IEnumerable<IEnumerable<XmlElement>> target = s_emptyEmpty;
                
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        IEnumerable<IEnumerable<XmlElement>> childPolicy = ReadNode(child, contextAssertion, yieldLimiter);
                        target = PolicyHelper.CrossProduct<XmlElement>(target, childPolicy, yieldLimiter);
                    }
                }
                return target;
            }

            internal IEnumerable<IEnumerable<XmlElement>> NormalizePolicy(IEnumerable<XmlElement> policyAssertions)
            {
                IEnumerable<IEnumerable<XmlElement>> target = s_emptyEmpty;
                YieldLimiter yieldLimiter = new YieldLimiter(this._metadataImporter._quotas.MaxYields, this._metadataImporter);
                foreach (XmlElement child in policyAssertions)
                {
                    IEnumerable<IEnumerable<XmlElement>> childPolicy = ReadNode(child, child, yieldLimiter);
                    target = PolicyHelper.CrossProduct<XmlElement>(target, childPolicy, yieldLimiter);
                }

                return target;
            }
        }

        internal class YieldLimiter
        {
            int _maxYields;
            int _yieldsHit;
            readonly MetadataImporter _metadataImporter;

            internal YieldLimiter(int maxYields, MetadataImporter metadataImporter)
            {
                this._metadataImporter = metadataImporter;
                this._yieldsHit = 0;
                this._maxYields = maxYields;
            }

            internal bool IncrementAndLogIfExceededLimit()
            {
                if (++_yieldsHit > _maxYields)
                {
                    string warningMsg = SRP.Format(SRP.ExceededMaxPolicySize);
                    _metadataImporter.PolicyWarningOccured.Invoke(null, warningMsg);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal static class PolicyHelper
        {
            

            internal static string GetFragmentIdentifier(XmlElement element)
            {
                string id = element.GetAttribute(MetadataStrings.Wsu.Attributes.Id, MetadataStrings.Wsu.NamespaceUri);

                if (id == null)
                {
                    id = element.GetAttribute(MetadataStrings.Xml.Attributes.Id, MetadataStrings.Xml.NamespaceUri);
                }

                if (string.IsNullOrEmpty(id))
                    return string.Empty;
                else
                    return string.Format(CultureInfo.InvariantCulture, "#{0}", id);
            }

            internal static bool IsPolicyURIs(XmlAttribute attribute)
            {
                return ((attribute.NamespaceURI == MetadataStrings.WSPolicy.NamespaceUri
                    || attribute.NamespaceURI == MetadataStrings.WSPolicy.NamespaceUri15)
                            && attribute.LocalName == MetadataStrings.WSPolicy.Attributes.PolicyURIs);
            }

            internal static NodeType GetNodeType(XmlNode node)
            {
                XmlElement currentElement = node as XmlElement;

                if (currentElement == null)
                    return PolicyHelper.NodeType.NonElement;

                if (currentElement.NamespaceURI != MetadataStrings.WSPolicy.NamespaceUri
                    && currentElement.NamespaceURI != MetadataStrings.WSPolicy.NamespaceUri15)
                    return NodeType.Assertion;
                else if (currentElement.LocalName == MetadataStrings.WSPolicy.Elements.Policy)
                    return NodeType.Policy;
                else if (currentElement.LocalName == MetadataStrings.WSPolicy.Elements.All)
                    return NodeType.All;
                else if (currentElement.LocalName == MetadataStrings.WSPolicy.Elements.ExactlyOne)
                    return NodeType.ExactlyOne;
                else if (currentElement.LocalName == MetadataStrings.WSPolicy.Elements.PolicyReference)
                    return NodeType.PolicyReference;
                else
                    return PolicyHelper.NodeType.UnrecognizedWSPolicy;
            }

            // 
            // some helpers for dealing with ands of ors
            //

            internal static IEnumerable<IEnumerable<T>> CrossProduct<T>(IEnumerable<IEnumerable<T>> xs, IEnumerable<IEnumerable<T>> ys, YieldLimiter yieldLimiter)
            {
                foreach (IEnumerable<T> x in AtLeastOne<T>(xs, yieldLimiter))
                {
                    foreach (IEnumerable<T> y in AtLeastOne<T>(ys, yieldLimiter))
                    {
                        if (yieldLimiter.IncrementAndLogIfExceededLimit())
                        {
                            yield break;
                        }
                        else
                        {
                            yield return Merge<T>(x, y, yieldLimiter);
                        }
                    }
                }
            }

            static IEnumerable<IEnumerable<T>> AtLeastOne<T>(IEnumerable<IEnumerable<T>> xs, YieldLimiter yieldLimiter)
            {
                bool gotOne = false;
                foreach (IEnumerable<T> x in xs)
                {
                    gotOne = true;

                    if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    {
                        yield break;
                    }
                    else
                    {
                        yield return x;
                    }
                }
                if (!gotOne)
                {
                    if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    {
                        yield break;
                    }
                    else
                    {
                        yield return new EmptyEnumerable<T>();
                    }
                }
            }

            static IEnumerable<T> Merge<T>(IEnumerable<T> e1, IEnumerable<T> e2, YieldLimiter yieldLimiter)
            {
                foreach (T t1 in e1)
                {
                    if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    {
                        yield break;
                    }
                    else
                    {
                        yield return t1;
                    }
                    
                }
                foreach (T t2 in e2)
                {
                    if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    {
                        yield break;
                    }
                    else
                    {
                        yield return t2;
                    }
                }
            }

            //
            // some helper enumerators
            //

            internal class EmptyEnumerable<T> : IEnumerable<T>, IEnumerator<T>
            {
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return this;
                }

                object IEnumerator.Current
                {
                    get { return this.Current; }
                }

                public T Current
                {
                    get
                    {
//#pragma warning suppress 56503 // Microsoft, IEnumerator guidelines, Current throws exception before calling MoveNext
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SRP.Format(SRP.NoValue0)));
                    }
                }

                public bool MoveNext()
                {
                    return false;
                }

                public void Dispose()
                {
                }

                void IEnumerator.Reset()
                {
                }
            }

            internal class SingleEnumerable<T> : IEnumerable<T>
            {
                T _value;

                internal SingleEnumerable(T value)
                {
                    this._value = value;
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                public IEnumerator<T> GetEnumerator()
                {
                    yield return _value;
                }
            }

            //
            // the NodeType enum
            //
            internal enum NodeType
            {
                NonElement,
                Policy,
                All,
                ExactlyOne,
                Assertion,
                PolicyReference,
                UnrecognizedWSPolicy,
            }


        }

    }
}
