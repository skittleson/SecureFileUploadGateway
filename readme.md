# Secure File Upload Gateway

A repeatable AWS serverless file uploader via api gateway. Built with the LambdaSharpTool.

Example:
`curl -H 'x-api-key:KEY' -H "Content-Type:application/octet-stream" --data-binary "@sample.jpg" https://SUBDOMAIN.execute-api.us-east-1.amazonaws.com/LATEST/FileUpload?fileName=sample.jpg`

## How to Use

- Setup the LambdaSharp tool: https://github.com/LambdaSharp/LambdaSharpTool
- Deploy to your tier `dotnet lash deploy --tier Sandbox`
- Grab the API key: `Amazon API Gateway > API Keys > Sandb-ApiKe-*`
- Assign api keys to usage plans.
- Enable binary support: Amazon API Gateway `APIs > Sandbox-SecureFileUploadGateway Module API > Settings`
  - Binary Media Types: `application/octet-stream`
  - Re-Deploy API: `Resources > Deploy API`. Select `LATEST`.

## Code Coverage

VS Code Extension: https://marketplace.visualstudio.com/items?itemName=ryanluker.vscode-coverage-gutters
Coverlet: https://github.com/tonerdo/coverlet

`make test`
