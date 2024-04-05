In order to start the project add it to the main project. To do this right click on Solution Add, Existing project. Select the pulled repo and click ok.

The main project is available on: https://github.com/pistii/KozossegiAPI

Before running the tests, check if the project's dependency is set to the main project. To see this right-click on to the unit test project's main folder, then add, project reference...

Select the main, then click ok.

Now the test cases can run. It may take a while to run all the test, but eventually if the main project is the latest one all the test cases should be successful.

In that case if it doesn't contains dependencies, it was used with EntityFrameworkCore.Inmemory 7.0.17. (greater version won't work)

This can be added by NuGet packages or command prompt by the following command:
```
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 7.0.17
```
