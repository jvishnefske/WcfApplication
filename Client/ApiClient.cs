using Grpc.Net.Client;
using System; // Needed for Uri
using ContactsApi.Grpc; // Namespace for generated gRPC client code (from .proto files)
using System.Configuration; // ADD THIS USING for ConfigurationManager

namespace Client // Corrected namespace
{
    public static class ApiClient
    {
        private static readonly GrpcChannel _channel;
        public static ContactsService.ContactsServiceClient ContactsClient { get; private set; }
        public static LookupsService.LookupsServiceClient LookupsClient { get; private set; }

        static ApiClient()
        {
            // Read gRPC address from App.config
            string? grpcAddress = ConfigurationManager.AppSettings["GrpcApiAddress"];
            if (string.IsNullOrEmpty(grpcAddress))
            {
                throw new InvalidOperationException("GrpcApiAddress is not configured in App.config.");
            }

            // Create a gRPC channel
            _channel = GrpcChannel.ForAddress(grpcAddress);
            
            // Create gRPC clients using the channel
            ContactsClient = new ContactsService.ContactsServiceClient(_channel);
            LookupsClient = new LookupsService.LookupsServiceClient(_channel);
        }

        public static void Shutdown()
        {
            _channel.Dispose();
        }
    }
}
