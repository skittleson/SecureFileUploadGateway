using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using MindTouch.LambdaSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace SecureFileUploadGateway.GatewayToS3Function {
    public class Function : ALambdaFunction<APIGatewayProxyRequest, APIGatewayProxyResponse> {
        private ProcessData _processData;

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config) {
            _processData = new ProcessData(config, new AmazonS3Client());
            return Task.CompletedTask;
        }

        public override async Task<APIGatewayProxyResponse> ProcessMessageAsync(APIGatewayProxyRequest message, ILambdaContext context) {
            return await _processData.Save(message);
        }

        private void LogDictionary(string prefix, IDictionary<string, string> keyValues) {
            if (keyValues != null) {
                foreach (var keyValue in keyValues) {
                    LogInfo($"{prefix}.{keyValue.Key} = {keyValue.Value}");
                }
            }
        }

        
    }
}