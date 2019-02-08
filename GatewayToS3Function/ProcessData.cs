using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.S3;
using Amazon.S3.Model;
using MindTouch.LambdaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SecureFileUploadGateway.GatewayToS3Function {

    public class ProcessData {

        public async Task<APIGatewayProxyResponse> Save(APIGatewayProxyRequest message, LambdaConfig _config, IAmazonS3 _s3Client) {
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
                // LogInfo($"Different bucket requested: {bucketName}");
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

        public (string key, bool isValidKey, string message) BucketKeyFormat(string fileName, string path) {
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

        public string SerializeJson(object value) {
            var json = new JsonSerializer();
            using (var stream = new MemoryStream()) {
                json.Serialize(value, stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}