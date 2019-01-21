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

        public async Task<Function> MockLambdaFunctionInstance(LambdaConfig lambdaConfig = null) {
            var instance = new Function(MockAmazonServices.MockLambdaFunctionConfiguration(), MockAmazonServices.MockS3Client().Object);
            await instance.InitializeAsync(lambdaConfig ?? new LambdaConfig(LambdaConfigSource));
            return instance;
        }

        // This is only for testing the mocking setup for xunit. Disregard in production
        [Fact]
        public async Task CanInitialize() {
            Function instance = null;
            try {
                instance = new Function();
                Assert.True(false);
            } catch (Exception ex) {
                Assert.Contains("RegionEndpoint", ex.GetBaseException().Message);
            }
            instance = new Function(MockAmazonServices.MockLambdaFunctionConfiguration(), MockAmazonServices.MockS3Client().Object);
            try {
                await instance.InitializeAsync(null);
                Assert.True(false);
            } catch (Exception ex) {
                Assert.Contains("Missing config", ex.Message);
            }
            await instance.InitializeAsync(new LambdaConfig(LambdaConfigSource));
            Assert.True(true);
        }

        [Fact]
        public async Task CanSaveFileToS3() {
            var mockApiRequest = new APIGatewayProxyRequest {
                Body = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                QueryStringParameters = new Dictionary<string, string>() {
                                         {"fileName","test.jpg" }
                                }
            };
            var instance = await MockLambdaFunctionInstance();
            var result = await instance.ProcessMessageAsync(mockApiRequest, new MockLambdaContext());
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
            var instance = await MockLambdaFunctionInstance();
            var result = await instance.ProcessMessageAsync(mockApiRequest, new MockLambdaContext());
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
            var instance = await MockLambdaFunctionInstance();
            var result = await instance.ProcessMessageAsync(mockApiRequest, new MockLambdaContext());
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("{\"Message\":\"Request bucket name is empty. Remove query parameter for default bucket.\"}", result.Body);
        }

        [Fact]
        public async Task CanFailForMissingFileName() {
            var mockApiRequest = new APIGatewayProxyRequest {
                Body = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                QueryStringParameters = new Dictionary<string, string>()
            };
            var instance = await MockLambdaFunctionInstance();
            var result = await instance.ProcessMessageAsync(mockApiRequest, new MockLambdaContext());
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("{\"Message\":\"Missing query parameter: fileName\"}", result.Body);
        }

        [Fact]
        public async Task CanFailIfNotBase64() {
            var instance = await MockLambdaFunctionInstance();
            var task = instance.ProcessMessageAsync(new APIGatewayProxyRequest {
                Body = "this is test !",
                QueryStringParameters = new Dictionary<string, string>() {
                                         {"fileName","test.jpg" }
                                }
            }, new MockLambdaContext());
            var ex = Assert.Throws<AggregateException>(() => task.Result);
            Assert.Contains("base 64", ex.Message);
        }

        [Fact]
        public async Task CanResponseAsBadRequestForInvalidBucket() {
            var mockApiRequest = new APIGatewayProxyRequest();
            var instance = await MockLambdaFunctionInstance(new LambdaConfig(new MockLambdaConfigSource(new Dictionary<string, string>() { { "SecureFileUploadGatewayBucket", "bad bucket arn" } })));
            var result = await instance.ProcessMessageAsync(mockApiRequest, new MockLambdaContext());
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("{\"Message\":\"An incorrect bucket arn\"}", result.Body);
        }
    }
}
