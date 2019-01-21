# Secure File Upload Gateway

A repeatable AWS serverless file uploader via api gateway. Built with the LambdaSharpTool: https://github.com/LambdaSharp/LambdaSharpTool .   A curl command to submit data in base64.

Example:
`(openssl base64 < sample.jpg) | curl -H 'x-api-key:KEY' --data @- https://DOMAIN.amazonaws.com/LATEST/FileUpload?fileName=sample.jpg`

```
--data-binary <data>
  (HTTP) This posts data exactly as specified with no extra processing whatsoever.
  If you start the data with the letter @, the rest should be a filename.  Data is
  posted in a similar manner as --data-ascii does, except that newlines are preserved
  and conversions are never done.

  If this option is used several times, the ones following the first will append data
  as described in -d, --data.
```

## Setup
- Deploy to your tier `dotnet lash deploy --tier Sandbox`
- Grab the API key: `Amazon API Gateway > API Keys> Sandb-ApiKe-*`



## Code Coverage
VS Code Extension: https://marketplace.visualstudio.com/items?itemName=ryanluker.vscode-coverage-gutters
Coverlet: https://github.com/tonerdo/coverlet

`make test`

