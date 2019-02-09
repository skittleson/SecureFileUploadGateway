
deploy:
	dotnet lash deploy --tier Sandbox

test:
	dotnet test -p:Exclude=[xunit.*]* -p:CollectCoverage=true -p:CoverletOutputFormat=lcov -p:CoverletOutput=./lcov.info SecureFileUpload.Tests/SecureFileUpload.Tests.csproj

upload:
	curl -H 'x-api-key:KEY' -H "Content-Type:application/octet-stream" --data-binary "@sample.jpg" https://SUBDOMAIN.execute-api.us-east-1.amazonaws.com/LATEST/FileUpload?fileName=sample.jpg

postInstall:
	aws apigateway get-usage-plans | python -c "import sys, json; print(json.load(sys.stdin)['items'][0]['id'])"


