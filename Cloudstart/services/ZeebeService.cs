using dotenv.net.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Builder;
using System.Text.Json;

namespace Cloudstart.Services
{
    public interface IZeebeService
    {
        public Task<IDeployResponse> Deploy(string modelFilename);
        public Task<String> StartWorkflowInstance(string bpmProcessId);
        public Task<ITopology> Status();
    }
    public class ZeebeService : IZeebeService
    {
        private readonly IZeebeClient _client;
        private readonly ILogger<ZeebeService> _logger;

        public ZeebeService(IEnvReader envReader, ILogger<ZeebeService> logger)
        {
            _logger = logger;
            var authServer = envReader.GetStringValue("ZEEBE_AUTHORIZATION_SERVER_URL");
            var clientId = envReader.GetStringValue("ZEEBE_CLIENT_ID");
            var clientSecret = envReader.GetStringValue("ZEEBE_CLIENT_SECRET");
            var zeebeUrl = envReader.GetStringValue("ZEEBE_ADDRESS");
            char[] port =
            {
                '4', '3', ':'
            };
            var audience = zeebeUrl?.TrimEnd(port);

            _client =
                ZeebeClient.Builder()
                    .UseGatewayAddress(zeebeUrl)
                    .UseTransportEncryption()
                    .UseAccessTokenSupplier(
                        CamundaCloudTokenProvider.Builder()
                            .UseAuthServer(authServer)
                            .UseClientId(clientId)
                            .UseClientSecret(clientSecret)
                            .UseAudience(audience)
                            .Build())
                    .Build();
        }

        public Task<ITopology> Status()
        {
            return _client.TopologyRequest().Send();
        }

        public async Task<IDeployResponse> Deploy(string modelFilename)
        {
            try
            {
                var filename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "assets", modelFilename);
                var deployment = await _client.NewDeployCommand().AddResourceFile(filename).Send();
                var res = deployment.Processes[0];
                //var processInstance = await _client
                //.NewCreateProcessInstanceCommand()
                //.ProcessDefinitionKey(processDefinitionKey)
                //.Variables("{\"a\":\"123\"}")
                //.Send();
                _logger.LogInformation("Deployed BPMN Model: " + res?.BpmnProcessId +
                 " v." + res?.Version);
                return deployment;
            }
            catch(Exception e)
            {
                _logger.LogError("Error: " + e.ToString());
                throw;
            }
        }

        public async Task<String> StartWorkflowInstance(string bpmProcessId)
        {
            var instance = await _client.NewCreateProcessInstanceCommand()
                    .BpmnProcessId(bpmProcessId)
                    .LatestVersion()
                    .Send();
            return JsonSerializer.Serialize(instance);
        }
    }
}

