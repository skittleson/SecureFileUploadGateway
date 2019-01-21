using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using MindTouch.LambdaSharp;
using System;
using System.Collections.Generic;
using System.IO;
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
            var response = new APIGatewayProxyResponse {
                Body = SerializeJson(new { Message = "Unknown Error" }),
                Headers = new Dictionary<string, string> { ["Content-Type"] = "text/json" },
                StatusCode = 500
            };
            var bucketName = _config.ReadText("SecureFileUploadGatewayBucket");
            if (!bucketName.Contains(":::")) {
                response.Body = SerializeJson(new { Message = "An incorrect bucket arn" });
                return response;
            }
            bucketName = bucketName.Split(":::")[1];
            message.QueryStringParameters.TryGetValue("fileName", out string s3Key);
            if (String.IsNullOrEmpty(s3Key)) {
                response.Body = SerializeJson(new {
                    Message = "Missing query parameter: fileName",
                });
                return response;
            }
            //message.QueryStringParameters.TryGetValue("path", out string path);
            var s3Request = new PutObjectRequest {
                BucketName = bucketName,
                Key = s3Key,
                InputStream = new MemoryStream(Convert.FromBase64String(message.Body)),
            };
            var result = await _s3Client.PutObjectAsync(s3Request);
            response.StatusCode = (int)result.HttpStatusCode;
            response.Body = SerializeJson(new {
                Bucket = bucketName,
                Key = s3Key,
                Message = "Upload succeed!"
            });
            return response;
        }
    }
}