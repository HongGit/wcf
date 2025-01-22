//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UpdateNetPipeServiceRefDefault
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "99.99.99")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="UpdateNetPipeServiceRefDefault.IStreamedService")]
    public interface IStreamedService
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamedService/UploadStream", ReplyAction="http://tempuri.org/IStreamedService/UploadStreamResponse")]
        System.Threading.Tasks.Task<System.DateTime> UploadStreamAsync(System.IO.Stream data);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStreamedService/GetDisconnectResult", ReplyAction="http://tempuri.org/IStreamedService/GetDisconnectResultResponse")]
        System.Threading.Tasks.Task<string> GetDisconnectResultAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "99.99.99")]
    public interface IStreamedServiceChannel : UpdateNetPipeServiceRefDefault.IStreamedService, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "99.99.99")]
    public partial class StreamedServiceClient : System.ServiceModel.ClientBase<UpdateNetPipeServiceRefDefault.IStreamedService>, UpdateNetPipeServiceRefDefault.IStreamedService
    {
        
        /// <summary>
        /// Implement this partial method to configure the service endpoint.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint to configure</param>
        /// <param name="clientCredentials">The client credentials</param>
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public StreamedServiceClient() : 
                base(StreamedServiceClient.GetDefaultBinding(), StreamedServiceClient.GetDefaultEndpointAddress())
        {
            this.Endpoint.Name = EndpointConfiguration.NetNamedPipeBinding_IStreamedService.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public StreamedServiceClient(EndpointConfiguration endpointConfiguration) : 
                base(StreamedServiceClient.GetBindingForEndpoint(endpointConfiguration), StreamedServiceClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public StreamedServiceClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(StreamedServiceClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public StreamedServiceClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(StreamedServiceClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public StreamedServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<System.DateTime> UploadStreamAsync(System.IO.Stream data)
        {
            return base.Channel.UploadStreamAsync(data);
        }
        
        public System.Threading.Tasks.Task<string> GetDisconnectResultAsync()
        {
            return base.Channel.GetDisconnectResultAsync();
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        #if !NET6_0_OR_GREATER
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        #endif
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.NetNamedPipeBinding_IStreamedService))
            {
                System.ServiceModel.NetNamedPipeBinding result = new System.ServiceModel.NetNamedPipeBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.TransferMode = System.ServiceModel.TransferMode.Streamed;
                result.Security.Mode = System.ServiceModel.NetNamedPipeSecurityMode.None;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.NetNamedPipeBinding_IStreamedService))
            {
                return new System.ServiceModel.EndpointAddress("net.pipe://localhost/IISHost/StreamedService.svc");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            return StreamedServiceClient.GetBindingForEndpoint(EndpointConfiguration.NetNamedPipeBinding_IStreamedService);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
        {
            return StreamedServiceClient.GetEndpointAddress(EndpointConfiguration.NetNamedPipeBinding_IStreamedService);
        }
        
        public enum EndpointConfiguration
        {
            
            NetNamedPipeBinding_IStreamedService,
        }
    }
}