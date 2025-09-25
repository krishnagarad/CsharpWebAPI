To run this application you will be needing an SQL Server instance. Follow below steps to run the application.

1. Open the application in Visual Studio 2022.
2. Go to appsettings.config and populate the required values for
   - AppSettings:Token with value `MySuperSecreteKeyAndRandomlyGeneratedThatLooksJustAwesomeButItIsNotThisNeedstobeVeryLong!!!111oneeleven'
   - ConnectionStrings:DefaultConnection (Pointing to your SQL Server instance) ex: `Server={Your Sql Server instance};Database=EventCalendar;Trusted_Connection=True;TrustServerCertificate=true`
   - ConnectionString:Redis (Point connection to your Redis instance) Ex: `localhost:6379`
   - CivicPlusAuth:ClientId (Client Id of CivicPlus API)
   - CivicPlusAuth:ClientSecret (Client Secrets of CivicPlus API)
   - CivicPlusService:BaseUrl (Base URL of CivicPlus API ending with ClientId)
3. Once all the values are populated in appsettings.json or your own user secrets.json you need to build the application.
4. After application build open your package manager console and run `update database` to create new sql server database on you instance and apply the changes.
5. After database created you can hit run application. This will open the swagger overlay API for CivicPlus API's.
6. To test API you need to register first so hit register endpoint in swagger and register there with username and password.
7. Once registered use the login api with registered username and password which will generate the JWT token. Copy this JWT and click on Authorize button at top. Enter this token into given box `whithout Bearer` and click Authorize.
8. Once you are authorized you can now interact with Events API's.
