# TripCalculatorAPI

C# Web API for calculating expenses for a group of users. API accepts a JSON string containing a list of Users, with each user containing a name and a list of decimal expenses. Returns a JSON string containing which user needs to pay another user, and how much, to make all expenses equal to within one cent.

# To Run

Download or clone repository. Restoring of NuGet packages should be part of first build, but if not, Right click the solution file and click 'Restore NuGet Packages.' After packages have been restored, run the application. Application runs in IIS Express, so should not require any additional permissions.

After built and running, use a Web API test harness to send messages to the API. Currently, the TripCalculator has a URI of localhost/api/TripCalculator. The only method exposed is the default POST method, with JSON data expected in the HTTP request's Body.

Tested locally using <a href="https://www.getpostman.com/">Postman</a>

# Unit Tests
Additionally, API can be tested with the Unit Test project, TripCalculatorAPI.Tests. The TripCalculatorControllerTest class contains all unit tests needed for the TripCalculatorAPI. All tests should be available after the application is successfully built.

# Domain and Interface

Domain: Trip Calculator

Interface: Web API

Platform: .NET

Language: C#
