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
            var configBucketName = _config.ReadText("SecureFileUploadGatewayBucket");
            if (!configBucketName.Contains(":::")) {
                return GatewayProxyResponse(new { Message = "An incorrect bucket arn" });
            }
            var bucketName = configBucketName.Split(":::")[1];
            if (message.QueryStringParameters.ContainsKey("bucket")
                && message.QueryStringParameters.TryGetValue("bucket", out string requestBucketName)) {
                if (string.IsNullOrEmpty(requestBucketName)) {
                    return GatewayProxyResponse(new { Message = "Request bucket name is empty. Remove query parameter for default bucket." });
                }
                LogInfo($"Different bucket requested: {bucketName}");
                bucketName = requestBucketName;
            }
            message.QueryStringParameters.TryGetValue("fileName", out string fileName);
            message.QueryStringParameters.TryGetValue("path", out string path);
            var (key, isValidKey, errorMessage) = BucketKeyFormat(fileName, path);
            if (!isValidKey) {
                return GatewayProxyResponse(new { Message = errorMessage });
            }
            var body = message.IsBase64Encoded ? Convert.FromBase64String(message.Body) : Encoding.ASCII.GetBytes(message.Body);
            var result = await _s3Client.PutObjectAsync(new PutObjectRequest {
                BucketName = bucketName,
                Key = key,
                InputStream = new MemoryStream(body)
            });
            return GatewayProxyResponse(new {
                Bucket = bucketName,
                Key = key,
                Message = ""
            }, (int)result.HttpStatusCode);
        }

        public static (string key, bool isValidKey, string message) BucketKeyFormat(string fileName, string path) {
            var isValid = !string.IsNullOrEmpty(fileName) &&
              fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
            if (!isValid) {
                return ("", false, "Missing query parameter: fileName");
            }
            return (fileName, true, "");
        }

        public APIGatewayProxyResponse GatewayProxyResponse(Object response, int statusCode = 500) {
            return new APIGatewayProxyResponse {
                Body = SerializeJson(response),
                Headers = new Dictionary<string, string> { ["Content-Type"] = "text/json" },
                StatusCode = statusCode
            };
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