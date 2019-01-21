
using Amazon;
using Amazon.KeyManagementService;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using MindTouch.LambdaSharp;
using Moq;
using System;
using System.Net;
using System.Threading;

namespace SecureFileUpload.Tests.Mocks
{
    public static class MockAmazonServices {

        public static LambdaFunctionConfiguration MockLambdaFunctionConfiguration(MindTouch.LambdaSharp.ConfigSource.ILambdaConfigSource lambdaConfigSource = null) {
            var sqsClientMock = new Mock<AmazonSQSClient>(FallbackCredentialsFactory.GetCredentials(true), new AmazonSQSConfig() { RegionEndpoint = RegionEndpoint.USEast1 });
            var kmsClientMock = new Mock<AmazonKeyManagementServiceClient>(FallbackCredentialsFactory.GetCredentials(true), new AmazonKeyManagementServiceConfig() { RegionEndpoint = RegionEndpoint.USEast1 });
            return new LambdaFunctionConfiguration() {
                SqsClient = sqsClientMock.Object,
                KmsClient = kmsClientMock.Object,
                EnvironmentSource = lambdaConfigSource ?? new MockLambdaConfigSource(),
                UtcNow = () => DateTime.UtcNow
            };
        }

        public static Mock<AmazonS3Client> MockS3Client() {
            var client = new Mock<AmazonS3Client>(FallbackCredentialsFactory.GetCredentials(true), new AmazonS3Config() { RegionEndpoint = RegionEndpoint.USEast1 });
            client.Setup(x => x.PutObjectAsync(It.IsAny<Amazon.S3.Model.PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Amazon.S3.Model.PutObjectResponse(){
                    HttpStatusCode = (HttpStatusCode)200
                });
            return client;
        }
    }
}
