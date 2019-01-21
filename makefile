
deploy:
	dotnet lash deploy --tier Sandbox

teardown:

test:
	dotnet test -p:Exclude=[xunit.*]* -p:CollectCoverage=true -p:CoverletOutputFormat=lcov -p:CoverletOutput=./lcov.info SecureFileUpload.Tests/SecureFileUpload.Tests.csproj

upload:
	(openssl base64 < sample.jpg) | curl -H 'x-api-key:KEY' \
	--data @- https://DOMAIN.amazonaws.com/LATEST/FileUpload?fileName=sample.jpg

