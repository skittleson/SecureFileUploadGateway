using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using MindTouch.LambdaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace SecureFileUploadGateway.GatewayToS3Function {
    public class Function : ALambdaFunction<APIGatewayProxyRequest, APIGatewayProxyResponse> {
        private IAmazonS3 _s3Client;
        private LambdaConfig _config { get; set; }

        // Required for mocking in XUnit
        public Function(MindTouch.LambdaSharp.LambdaFunctionConfiguration config, IAmazonS3 s3Client) : base(config) {
            _s3Client = s3Client;
        }

        // Required to run as a lambda function
        public Function() { }

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config) {
            _config = config;
            _s3Client = _s3Client ?? new AmazonS3Client();
            if (_config == null) {
                throw new Exception("Missing config");
            }
            return Task.CompletedTask;
        }

        public override async Task<APIGatewayProxyResponse> ProcessMessageAsync(APIGatewayProxyRequest message, ILambdaContext context) {
            return await (new ProcessData()).Save(message, _config, _s3Client);
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