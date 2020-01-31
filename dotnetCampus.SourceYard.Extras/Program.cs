using System;
using System.Collections.Generic;
using CommandLine;
using dotnetCampus.SourceYard.Cli;
using dotnetCampus.SourceYard.Extras.ApplyFlow;

namespace dotnetCampus.SourceYard.Extras
{
    class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ApplyOptions>(args)
                .WithParsed(ApplyAndReturnExitCode)
                .WithNotParsed(HandleParseError);
        }

        private static void ApplyAndReturnExitCode(ApplyOptions options)
        {
            new TransformCodeFlow().Apply(options);
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            var logger = new Logger();
            foreach (var temp in errors)
            {
                logger.Error(temp.ToString());
            }
        }
    }
}
