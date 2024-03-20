using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using StorageService;
using Xunit;

namespace StorageServiceUnitTests
{
    public class FileWriterUnitTests
    {
        private FileWriter _systemUnderTest;

        public FileWriterUnitTests()
        {
            _systemUnderTest = new FileWriter();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Given_InvalidFilePath_When_AppendToFile_Then_ExceptionIsThrown(string filePath)
        {
            //act
            Func<Task> act = async () => await _systemUnderTest.AppendToFile(filePath, "input");

            //assert
            act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("filePath");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Given_InvalidInput_When_AppendToFile_Then_ExceptionIsThrown(string input)
        {
            //act
            Func<Task> act = async () => await _systemUnderTest.AppendToFile("filePath", input);

            //assert
            act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("input");
        }

        [Fact]
        public async Task Given_ValidInputAndDirectoryDoesNOTExist_When_AppendToFile_Then_DirectoryIsCreated()
        {
            // arrange
            var filePath = "dir1/dir2/file.txt";

            Directory.Exists("dir1/dir2").Should().BeFalse();

            //act
            await _systemUnderTest.AppendToFile(filePath, "input");

            //assert
            Directory.Exists("dir1/dir2").Should().BeTrue();

            Directory.Delete("dir1/dir2", true);
        }

        [Fact]
        public async Task Given_ValidInput_When_AppendToFile_Then_InputIsWrittenToTheFile()
        {
            // arrange
            var filePath = "dir3/dir4/file.txt";

            Directory.Exists("dir3/dir4").Should().BeFalse();
            var input = "some text";

            //act
            await _systemUnderTest.AppendToFile(filePath, input);

            //assert
            var readText = await File.ReadAllTextAsync(filePath);
            readText.Should().Be(input);

            Directory.Delete("dir3/dir4", true);
        }

        [Fact]
        public async Task Given_ConcurrentUsers_When_AppendToFile_Then_InputsAreWrittenToTheFile()
        {
            // arrange
            var filePath = "dir/file.txt";

            if (Directory.Exists("dir"))
            {
                Directory.Delete("dir", true);
            }

            Directory.Exists("dir").Should().BeFalse();

            var fileTasks = new List<Task>();
            var inputs = new List<string>
            {
                "input1",
                "input2",
                "input3",
                "input4",
                "input5"
            };

            inputs.ForEach(i =>
            {
                fileTasks.Add(_systemUnderTest.AppendToFile(filePath, i));
            });

            //act
            await Task.WhenAll(fileTasks);

            //assert
            var expectedFileContent = string.Join("", inputs);
            var readText = await File.ReadAllTextAsync(filePath);
            readText.Should().Be(expectedFileContent);
        }
    }
}