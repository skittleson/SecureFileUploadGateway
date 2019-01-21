using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecureFileUpload.Tests.Mocks
{
    public class MockLambdaContext : ILambdaContext {
        public MockLambdaContext() { }
        public string AwsRequestId => "1";
        public IClientContext ClientContext => null;
        public string FunctionName => "test";
        public string FunctionVersion => "0";
        public ICognitoIdentity Identity => null;
        public string InvokedFunctionArn => "";
        public ILambdaLogger Logger => null;
        public string LogGroupName => "";
        public string LogStreamName => "";
        public int MemoryLimitInMB => 0;
        public TimeSpan RemainingTime => new TimeSpan(100);
    }
}
