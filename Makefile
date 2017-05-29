dependencies:
	sudo apt-get install nunit-console || sudo dnf install nunit nuget
	nuget restore CloudstrypeArray/CloudstrypeArray.sln

build: dependencies
	xbuild /p:Configuration=Debug CloudstrypeArray/CloudstrypeArray.sln

test: build
	nunit-console CloudstrypeArray/Tests/bin/Debug/Tests.dll

travis:
	xbuild /p:Configuration=Debug CloudstrypeArray/CloudstrypeArray.sln
	nunit-console CloudstrypeArray/Tests/bin/Debug/Tests.dll
