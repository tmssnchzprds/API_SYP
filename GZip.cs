using System;
using System.Activities;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace APP_SYP
{
    public class GZipActivity : CodeActivity
    {
        public enum Command
        {
            Nothing,
            Help,
            Compress,
            Decompress
        }

        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> SourceFile { get; set; }

        [Category("Input")]
        [RequiredArgument]
        public InArgument<Command> CommandType { get; set; }

        [Category("Output")]
        public OutArgument<string> TargetFile { get; set; }

        [Category("Input")]
        public InArgument<int> CompressionLevel { get; set; } = 9;

        protected override void Execute(CodeActivityContext context)
        {
            string source = SourceFile.Get(context);
            string target;
            Command command = CommandType.Get(context);
            int level = CompressionLevel.Get(context);

            try
            {
                if (command == Command.Compress)
                {
                    target = source + ".gz";
                    using (FileStream inputFileStream = File.OpenRead(source))
                    using (FileStream outputFileStream = File.Create(target))
                    {
                        GZip.Compress(inputFileStream, outputFileStream, true, level);
                    }
                    Console.WriteLine($"Compressing {source} to {target} at level {level}");
                }
                else if (command == Command.Decompress)
                {
                    target = Path.GetFileNameWithoutExtension(source);
                    using (FileStream inputFileStream = File.OpenRead(source))
                    using (FileStream outputFileStream = File.Create(target))
                    {
                        GZip.Decompress(inputFileStream, outputFileStream, true);
                    }
                    Console.WriteLine($"Decompressing {source} to {target}");
                }
                else
                {
                    throw new InvalidOperationException("Invalid command");
                }

                TargetFile.Set(context, target);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
