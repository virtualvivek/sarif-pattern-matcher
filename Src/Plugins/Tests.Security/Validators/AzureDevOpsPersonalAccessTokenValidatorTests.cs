﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.PatternMatcher.Plugins.Security
{
    public class AzureDevOpsPersonalAccessTokenValidatorTests
    {
        internal class TestCase
        {
            public string Title;
            public string Input;
            public string ExpectedValidationState;
            public bool PerformDynamicValidation;
            public string FailureLevel;
        }

        internal static TestCase[] s_coreTestCases = new[]
        {
            new TestCase
            {
                Title = "NoMatch due to invalid PAT CRC, no dynamic validation requested.",
                Input = "deadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdead",
                ExpectedValidationState = "NoMatch",
                FailureLevel = "Error",
            },
            new TestCase
            {
                Title = "NoMatch due to invalid PAT CRC, dynamic validation requested.",
                Input = "deadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdead",
                ExpectedValidationState = "NoMatch",
                FailureLevel = "Warning",
                PerformDynamicValidation = true
            }
        };

        [Fact]
        public void AzureDevOpsPersonalAccessTokenValidator_IsValidBasic()
        {
            var failedTestCases = new List<string>();

            foreach (TestCase testCase in s_coreTestCases)
            {
                bool performDynamicValidation = testCase.PerformDynamicValidation;
                string failureLevel = testCase.FailureLevel;

                string state = AzureDevOpsPersonalAccessTokenValidator.IsValid(
                    testCase.Input,
                    null,
                    ref performDynamicValidation,
                    ref failureLevel);

                string title = testCase.Title;

                Verify(state == testCase.ExpectedValidationState, title, failedTestCases);

                // The core ADO PAT validator does not perform any dynamic checking
                Verify(performDynamicValidation == false, title, failedTestCases);

                Verify(failureLevel == testCase.FailureLevel, title, failedTestCases);
            }

            failedTestCases.Should().BeEmpty();
        }

        private void Verify(bool condition, string title, List<string> failedTestCases)
        {
            if (!condition)
            {
                failedTestCases.Add(title);
            }
        }

        [Fact]
        public void AzureDevOpsPersonalAccessTokenValidator_CheckInvalidInput()
        {
            string[] invalidInputs = new[] { "a", string.Empty };

            foreach (string input in invalidInputs)
            {
                bool performDynamicValidation = false;
                string failureLevel = "error";
                Assert.Throws<ArgumentException>(() => AzureDevOpsPersonalAccessTokenValidator.IsValid(input, null, ref performDynamicValidation, ref failureLevel));
            }
        }
    }
}
