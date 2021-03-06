using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using FluentTc.Engine;
using FluentTc.Exceptions;
using FluentTc.Tests.Locators;
using JetBrains.TeamCity.ServiceMessages.Write.Special;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace FluentTc.Tests.Engine
{
    [TestFixture]
    public class BuildParametersTests
    {
        [Test]
        public void Constructor_PropertiesFileIsNull_NoExceptionThrown()
        {
            // Arrange
            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(null);

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action action =
                () =>
                    new BuildParameters(teamCityBuildPropertiesFileRetriever, A.Fake<ITeamCityWriterFactory>(),
                        A.Fake<IPropertiesFileParser>());

            // Assert
            action.ShouldNotThrow<Exception>();
        }

        [Test]
        public void GetParameterValue_MissingParameter_MissingBuildParameterExceptionThrown()
        {
            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(@"C:\properties.file.txt");

            var propertiesFileParser = A.Fake<IPropertiesFileParser>();
            A.CallTo(() => propertiesFileParser.ParsePropertiesFile(@"C:\properties.file.txt"))
                .Returns(new Dictionary<string, string>());

            var buildParameters = new BuildParameters(teamCityBuildPropertiesFileRetriever, A.Fake<ITeamCityWriterFactory>(),
                propertiesFileParser);

            // Act
            string result;
            Action action = () => result = buildParameters.GetBuildParameter<string>("missing.param");

            // Assert
            action.ShouldThrow<MissingBuildParameterException>()
                .WithMessage("Build parameter missing.param is missing. It needs to be added from TeamCity");
        }

        [Test]
        public void GetParameterValue_TeamcityBuildConfName_ValueReturned()
        {
            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(@"C:\properties.file.txt");

            var dictionary = new Dictionary<string, string> {{"teamcity.buildConfName", "FluentTc"}};

            var propertiesFileParser = A.Fake<IPropertiesFileParser>();
            A.CallTo(() => propertiesFileParser.ParsePropertiesFile(@"C:\properties.file.txt"))
                .Returns(dictionary);

            var buildParameters = new BuildParameters(teamCityBuildPropertiesFileRetriever, A.Fake<ITeamCityWriterFactory>(),
                propertiesFileParser);

            // Act
            string buildConfName = buildParameters.TeamcityBuildConfName;

            // Assert
            buildConfName.Should().Be("FluentTc");
        }

        [Test]
        public void TryGetParameterValue_TeamcityBuildConfName_ValueReturned()
        {
            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(@"C:\properties.file.txt");

            var dictionary = new Dictionary<string, string> {{"teamcity.buildConfName", "FluentTc"}};

            var propertiesFileParser = A.Fake<IPropertiesFileParser>();
            A.CallTo(() => propertiesFileParser.ParsePropertiesFile(@"C:\properties.file.txt"))
                .Returns(dictionary);

            var buildParameters = new BuildParameters(teamCityBuildPropertiesFileRetriever, A.Fake<ITeamCityWriterFactory>(),
                propertiesFileParser);

            // Act
            string buildConfName;
            var valueExists = buildParameters.TryGetBuildParameter("teamcity.buildConfName", out buildConfName);

            // Assert
            valueExists.Should().BeTrue();
            buildConfName.Should().Be("FluentTc");
        }

        [Test]
        public void TryGetParameterValue_ValueDoesNotExists_False()
        {
            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(@"C:\properties.file.txt");

            var propertiesFileParser = A.Fake<IPropertiesFileParser>();
            A.CallTo(() => propertiesFileParser.ParsePropertiesFile(@"C:\properties.file.txt"))
                .Returns(new Dictionary<string, string>());

            var buildParameters = new BuildParameters(teamCityBuildPropertiesFileRetriever, A.Fake<ITeamCityWriterFactory>(),
                propertiesFileParser);

            // Act
            string buildConfName;
            var valueExists = buildParameters.TryGetBuildParameter("teamcity.buildConfName", out buildConfName);

            // Assert
            valueExists.Should().BeFalse();
        }

        [Test]
        public void SetParameterValue_GetParameterValue_ValueThatWasSetReturned()
        {
            var teamCityWriter = A.Fake<ITeamCityWriter>();

            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(null);

            var teamCityWriterFactory = A.Fake<ITeamCityWriterFactory>();
            A.CallTo(() => teamCityWriterFactory.CreateTeamCityWriter()).Returns(teamCityWriter);

            var buildParameters = new BuildParameters(teamCityBuildPropertiesFileRetriever, teamCityWriterFactory,
                A.Fake<IPropertiesFileParser>());

            // Act
            buildParameters.SetBuildParameter("param1", "newValue");
            var parameterValue = buildParameters.GetBuildParameter<string>("param1");

            // Assert
            parameterValue.Should().Be("newValue");
        }

        [Test]
        public void SetParameterValue_TeamCityMode_ExceptionThrown()
        {
            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns("propertiesFile.txt");

            var propertiesFileParser = A.Fake<IPropertiesFileParser>();
            A.CallTo(() => propertiesFileParser.ParsePropertiesFile("propertiesFile.txt"))
                .Returns(new Dictionary<string, string>());

            var buildParameters = new BuildParameters(teamCityBuildPropertiesFileRetriever,
                A.Fake<ITeamCityWriterFactory>(), propertiesFileParser);
            Action action = () => buildParameters.SetBuildParameter("param1", "newValue");

            // Assert
            action.ShouldThrow<MissingBuildParameterException>();
        }

        [Test]
        public void SetParameterValue_NotTeamCityMode_ValueSet()
        {
            // Arrange
            var teamCityWriter = A.Fake<ITeamCityWriter>();

            var teamCityBuildPropertiesFileRetriever = A.Fake<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(null);

            var teamCityWriterFactory = A.Fake<ITeamCityWriterFactory>();
            A.CallTo(() => teamCityWriterFactory.CreateTeamCityWriter()).Returns(teamCityWriter);

            var buildParameters = new BuildParameters(teamCityBuildPropertiesFileRetriever, teamCityWriterFactory,
                A.Fake<IPropertiesFileParser>());
            
            // Act
            buildParameters.SetBuildParameter("param1", "newValue");
            var parameterValue = buildParameters.GetBuildParameter<string>("param1");

            // Assert
            parameterValue.Should().Be("newValue");
            A.CallTo(() => teamCityWriter.WriteBuildParameter("param1", "newValue")).MustHaveHappened();
        }

        [Test]
        public void ResolveBuildParameters_BuildParameters_NotNull()
        {
            // Arrange
            var bootstrapper = new Bootstrapper();

            // Act
            var buildParameters = bootstrapper.Get<IBuildParameters>();

            // Assert
            buildParameters.Should().NotBeNull();
        }

        [Test]
        public void IsTeamCityMode_PropertiesFileIsNull_False()
        {
            // Arrange
            var fixture = Auto.Fixture();
            var teamCityBuildPropertiesFileRetriever = fixture.Freeze<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns(null);

            var parameters = fixture.Create<BuildParameters>();

            // Act
            var isTeamCityMode = parameters.IsTeamCityMode;

            // Assert
            isTeamCityMode.Should().BeFalse();
        }

        [Test]
        public void IsTeamCityMode_PropertiesFileIsNotNull_True()
        {
            // Arrange
            var fixture = Auto.Fixture();
            var teamCityBuildPropertiesFileRetriever = fixture.Freeze<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns("Some content");

            var parameters = fixture.Create<BuildParameters>();

            // Act
            var isTeamCityMode = parameters.IsTeamCityMode;

            // Assert
            isTeamCityMode.Should().BeTrue();
        }

        [Test]
        public void IsPersonal_ParameterMissing_False()
        {
            // Arrange
            var fixture = Auto.Fixture();
            var teamCityBuildPropertiesFileRetriever = fixture.Freeze<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns("some file content");

            var parameters = fixture.Create<BuildParameters>();

            // Act
            var isTeamCityMode = parameters.IsPersonal;

            // Assert
            isTeamCityMode.Should().BeFalse();
        }

        [Test]
        public void IsPersonal_ParameterExistsWithValueTrue_True()
        {
            // Arrange
            var fixture = Auto.Fixture();
            var teamCityBuildPropertiesFileRetriever = fixture.Freeze<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns("some file content");

            var propertiesFileParser = fixture.Freeze<IPropertiesFileParser>();
            var dictionary = new Dictionary<string, string> {{"build.is.personal", "true"}};

            A.CallTo(() => propertiesFileParser.ParsePropertiesFile(A<string>._))
                .Returns(dictionary);

            var parameters = fixture.Create<BuildParameters>();

            // Act
            var isTeamCityMode = parameters.IsPersonal;

            // Assert
            isTeamCityMode.Should().BeTrue();
        }

        [Test]
        public void IsPersonal_ParameterExistsWithValueFalse_False()
        {
            // Arrange
            var fixture = Auto.Fixture();
            var teamCityBuildPropertiesFileRetriever = fixture.Freeze<ITeamCityBuildPropertiesFileRetriever>();
            A.CallTo(() => teamCityBuildPropertiesFileRetriever.GetTeamCityBuildPropertiesFilePath()).Returns("some file content");

            var propertiesFileParser = fixture.Freeze<IPropertiesFileParser>();
            var dictionary = new Dictionary<string, string> { { "build.is.personal", "false" } };
            A.CallTo(() => propertiesFileParser.ParsePropertiesFile(A<string>._))
                .Returns(dictionary);

            var parameters = fixture.Create<BuildParameters>();

            // Act
            var isTeamCityMode = parameters.IsPersonal;

            // Assert
            isTeamCityMode.Should().BeFalse();
        }
    }
}