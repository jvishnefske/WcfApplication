using Grpc.Net.Client;
using Client.Grpc; // Namespace for generated gRPC client code (from .proto files)
using System; // Needed for Uri

namespace Client // Corrected namespace
{
    public static class ApiClient
    {
        private static readonly GrpcChannel _channel;
        public static ContactsService.ContactsServiceClient ContactsClient { get; private set; }
        public static LookupsService.LookupsServiceClient LookupsClient { get; private set; }

        // IMPORTANT: Adjust this address to where your ContactsApi gRPC endpoint is running.
        // For local development, it's often https://localhost:7001/
        private static readonly string _grpcAddress = "https://localhost:7001"; 

        static ApiClient()
        {
            // Create a gRPC channel
            _channel = GrpcChannel.ForAddress(_grpcAddress);
            
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
