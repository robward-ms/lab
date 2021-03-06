﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.Restier.Scaffolding
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNet.Scaffolding;

    public class RestierConfigScaffolder : ConfigScaffolder<ODataFrameworkDependency>
    {
        private const string TemplateName = "config.MapRestierRoute<DbApi<{0}>>(\"{1}\", \"api/{1}\", null);";
        private const string AnnotationLine = "// Restier routes";
        private const string Instruction = "// Add the following code in Register method (after MapHttpAttributeRoutes before MapHttpRoute) for Restier routes.";
        private const string UsingLine = "using Microsoft.Restier.EntityFramework;\r\nusing Microsoft.Restier.WebApi;";
        private const string ContextPostfix = "context";
        private const string AfterString1 = "public static void Register(";
        private const string AfterString2 = ".MapHttpAttributeRoutes(";

        public RestierConfigScaffolder(CodeGenerationContext context, CodeGeneratorInformation information)
            : base(context, information)
        {
        }

        protected internal override void Scaffold()
        {
            ModifyConfigFile(GeneratedAppendLine(Model.DataContextType.TypeName, Model.DataContextType.ShortTypeName));

            // We want to record SQM data even on failure
            Framework.RecordControllerTelemetryOptions(Context, Model);
        }

        protected static string GeneratedAppendLine(string contextTypeName, string contextShortName)
        {
            if (string.IsNullOrEmpty(contextTypeName))
            {
                throw new ArgumentException("contextTypeName cannot be null or empty", "contextTypeName");
            }

            if (string.IsNullOrEmpty(contextShortName))
            {
                throw new ArgumentException("contextShortName cannot be null or empty", "contextShortName");
            }

            string name = contextShortName;

            if (name.ToLower(CultureInfo.InvariantCulture).EndsWith(ContextPostfix, StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - ContextPostfix.Length);
            }

            return String.Format(CultureInfo.InvariantCulture, TemplateName, contextTypeName, name);
        }

        protected void ModifyConfigFile(string appendLine)
        {
            if (string.IsNullOrEmpty(appendLine))
            {
                throw new ArgumentException("appendLine cannot be null or empty", "appendLine");
            }

            if (string.IsNullOrEmpty(Model.ConfigFileFullPath))
            {
                throw new ArgumentException("configFilePath");
            }

            var allLines = File.ReadAllLines(Model.ConfigFileFullPath).ToList();
            int insertPos = -1;

            for (int i = 0; i < allLines.Count; ++i)
            {
                if (insertPos == -1 && allLines[i].Contains(AfterString1) && i + 1 != allLines.Count)
                {
                    insertPos = i + 2;
                }
                else if (insertPos != -1 && allLines[i].Contains(AfterString2))
                {
                    insertPos = i + 1;
                }
            }

            if (insertPos != -1)
            {
                allLines.Insert(insertPos, appendLine);
                allLines.Insert(insertPos, AnnotationLine);
            }
            else
            {
                allLines.Insert(0, "// " + appendLine);
                allLines.Insert(0, Instruction);
            }
            allLines.Insert(0, UsingLine);

            File.WriteAllLines(Model.ConfigFileFullPath, allLines.ToArray());
        }
    }
}
