@echo off
dotnet test "%~dp0tests\ImageProcessingEngine.Tests\ImageProcessingEngine.Tests.csproj" --verbosity normal
