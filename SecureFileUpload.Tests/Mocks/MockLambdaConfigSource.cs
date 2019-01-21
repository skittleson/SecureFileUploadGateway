using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MindTouch.LambdaSharp.ConfigSource;

namespace SecureFileUpload.Tests.Mocks
{
    public class MockLambdaConfigSource : ILambdaConfigSource {
        public Dictionary<string, string> KeyValueConfig;
        public MockLambdaConfigSource() {
            KeyValueConfig = new Dictionary<string, string>();
        }
        public MockLambdaConfigSource(Dictionary<string, string> keyValueConfig) {
            KeyValueConfig = keyValueConfig;
        }
        public ILambdaConfigSource Open(string key) => this;
        public string Read(string key) => KeyValueConfig[key];
        public IEnumerable<string> ReadAllKeys() => KeyValueConfig.Select(x => x.Key);
    }
}
