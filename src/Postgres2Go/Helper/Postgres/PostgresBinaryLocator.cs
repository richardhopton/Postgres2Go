﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Postgres2Go.Common;
using Postgres2Go.Helper.FileSystem;

namespace Postgres2Go.Helper.Postgres
{

    internal class PostgresBinaryLocator
    {
        internal const string DefaultWindowsSearchPattern = @"pg-dist\pgsql-*-windows64-binaries\bin";
        internal const string DefaultLinuxSearchPattern = "pg-dist/pgsql-*-linux-binaries/bin";
        
        internal static string NugetPackagesDirectoryPrefix = Path.Combine("packages", "Postgres2Go*");
        
        private string _binFolder = string.Empty;
        private readonly string _searchPattern;

        public PostgresBinaryLocator(string searchPatternOverride)
        {
            if (string.IsNullOrEmpty(searchPatternOverride))
            {
                switch (RecognizedOSPlatform.Determine())
                {
                    case RecognizedOSPlatformEnum.Linux:
                        _searchPattern = DefaultLinuxSearchPattern;
                        break;
                    case RecognizedOSPlatformEnum.OSX:
                        throw new UnsupportedPlatformException("Cannot locate Postgres binaries when running on OSX platform. OSX platform is not supported.");
                    case RecognizedOSPlatformEnum.Windows:
                        _searchPattern = DefaultWindowsSearchPattern;
                        break;
                    default:
#if NETSTANDARD2_0
                        throw new PostgresBinariesNotFoundException($"Unknow OS:{RuntimeInformation.OSDescription}");
#endif
                        throw new PostgresBinariesNotFoundException("Unknow OS. This library is only compatible with Microsoft Windows");
                        
                }
            }
            else
            {
                _searchPattern = searchPatternOverride;
            }
        }

        internal string Directory 
            => String.IsNullOrEmpty(_binFolder) 
                ? _binFolder = ResolveBinariesDirectory() 
                : _binFolder
                ;

        private string ResolveBinariesDirectory()
        {
            string searchPatternWithPackagesRootFolder = Path.Combine(NugetPackagesDirectoryPrefix, _searchPattern);

            var binariesFolder =
                FolderSearch.CurrentExecutingDirectory().FindFolderUpwards(searchPatternWithPackagesRootFolder) 
                ??
                FolderSearch.CurrentExecutingDirectory().FindFolderUpwards(_searchPattern);

            if (binariesFolder == null)
                throw new PostgresBinariesNotFoundException ($"Could not find Postgres binaries using the search pattern of {_searchPattern}. The Searching began in {FolderSearch.CurrentExecutingDirectory()}");

            return binariesFolder;
        }
    }
}
