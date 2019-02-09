using Amazon.Lambda.APIGatewayEvents;
using MindTouch.LambdaSharp;
using SecureFileUpload.Tests.Mocks;
using SecureFileUploadGateway.GatewayToS3Function;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SecureFileUpload.Tests {
    public class GatewayToS3FunctionTests {
        public MockLambdaConfigSource LambdaConfigSource { get; }

        public GatewayToS3FunctionTests() {
            LambdaConfigSource = new MockLambdaConfigSource(new Dictionary<string, string>() {
                ["SecureFileUploadGatewayBucket"] = "testArn:::bucketName"
            });
        }

        public ProcessData MockLambdaFunctionInstance(LambdaConfig lambdaConfig = null) {
            return new ProcessData(lambdaConfig ?? new LambdaConfig(LambdaConfigSource), MockAmazonServices.MockS3Client().Object);
        }

        [Fact]
        public async Task CanSaveFileToS3() {
            var mockApiRequest = new APIGatewayProxyRequest {
                Body = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                QueryStringParameters = new Dictionary<string, string>() {
                                         {"fileName","test.jpg" }
                                }
            };
            var result = await MockLambdaFunctionInstance().Save(mockApiRequest);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("{\"Bucket\":\"bucketName\",\"Key\":\"test.jpg\",\"Message\":\"\"}", result.Body);
        }

        [Fact]
        public async Task CanSaveFileToDifferentS3Bucket()
        {
            var mockApiRequest = new APIGatewayProxyRequest
            {
                Body = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                QueryStringParameters = new Dictionary<string, string>() {
                                         {"fileName","test.jpg" },
                                         {"bucket","different-bucket" }
                                }
            };
            var result = await MockLambdaFunctionInstance().Save(mockApiRequest);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("{\"Bucket\":\"different-bucket\",\"Key\":\"test.jpg\",\"Message\":\"\"}", result.Body);
        }

        [Fact]
        public async Task CanFailValidationToSaveInDifferentS3Bucket()
        {
            var mockApiRequest = new APIGatewayProxyRequest
            {
                Body = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                QueryStringParameters = new Dictionary<string, string>() {
                                         {"fileName","test.jpg" },
                                         {"bucket","" }
                                }
            };
            var result = await MockLambdaFunctionInstance().Save(mockApiRequest);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("{\"Message\":\"Request bucket name is empty. Remove query parameter for default bucket.\"}", result.Body);
        }

        [Fact]
        public async Task CanFailForMissingFileName() {
            var mockApiRequest = new APIGatewayProxyRequest {
                Body = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                QueryStringParameters = new Dictionary<string, string>()
            };
            var result = await MockLambdaFunctionInstance().Save(mockApiRequest);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("{\"Message\":\"Missing query parameter: fileName\"}", result.Body);
        }

        [Fact]
        public async Task CanFailIfNotBase64() {
            var instance = MockLambdaFunctionInstance();
            var task = instance.Save(new APIGatewayProxyRequest {
                Body = "this is test !",
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>() {
                                         {"fileName","test.jpg" }
                                }
            });
            var ex = Assert.Throws<AggregateException>(() => task.Result);
            Assert.Contains("base 64", ex.Message);
        }

        [Fact]
        public async Task CanResponseAsBadRequestForInvalidBucket() {
            var mockApiRequest = new APIGatewayProxyRequest();
            var instance = MockLambdaFunctionInstance(new LambdaConfig(new MockLambdaConfigSource(new Dictionary<string, string>() { { "SecureFileUploadGatewayBucket", "bad bucket arn" } })));
            var result = await instance.Save(mockApiRequest);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("{\"Message\":\"An incorrect bucket arn\"}", result.Body);
        }
    }
}
