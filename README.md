# Largest Files Finder

A fast, secure, and easy-to-use command-line tool for Windows to find the largest files on your drives. Discover what's taking up your disk space with a single command.

## About The Project

Ever wonder where all your disk space has gone? **Largest Files Finder** is a simple utility built to answer that question. It scans your hard drives and USB devices to identify the biggest space hogs, helping you manage your storage more effectively.

It's designed with a "security-first" mindset, requiring no administrator privileges and using modern, safe APIs. It runs as a single executable with zero dependencies, making it the perfect portable tool for any toolkit.

### Core Features

  * **Comprehensive Scanning**: Enumerates all fixed drives (SSDs, HDDs) and removable drives to find the top 50 largest files on each.
  * **Dual Output Formats**: Generates a report as a clean, human-readable text file or as a structured `JSON` file for use in scripts and other applications.
  * **Portable & Simple**: Delivered as a single `.exe` file. No installation, no dependencies, no .NET runtime required on the host machine. Just download and run.
  * **Safe & Secure**: Built with a "least privilege" approach. It does not require administrator rights and gracefully handles "Access Denied" errors without stopping the scan.
  * **Modern and Performant**: Utilizes modern .NET 8 features for efficient, fast, and robust file enumeration, including full support for long file paths.

## Getting Started

Getting started is as simple as downloading the latest release.

1.  Navigate to the **Releases** page.
2.  Download the `LargestFilesFinder.exe` file from the latest release's **Assets** section.
3.  That's it\! The tool is ready to use.

## Usage

This is a command-line tool. You will need to run it from PowerShell or the Windows Command Prompt (CMD).

1.  Open your terminal in the same directory where you saved `LargestFilesFinder.exe`.
2.  Run one of the commands below.

-----

#### To generate a text report (`largest_files.txt`):

```powershell
.\LargestFilesFinder.exe
```

#### To generate a JSON report (`largest_files.json`):

```powershell
.\LargestFilesFinder.exe --json
```

After the command finishes, the report file will be created in the same folder as the executable.

### Example Output

\<details\>
\<summary\>\<strong\>Click to see largest\_files.txt example\</strong\>\</summary\>

```text
Largest 50 files on C::
   2.89 GB – C:\Users\Media\video-project.mp4
   1.52 GB – C:\Virtual Machines\Windows11.vhdx
   980.5 MB – C:\Program Files\Game\assets.pak
   ...

Largest 50 files on D::
   10.2 GB – D:\backups\archive-2024.zip
   4.11 GB – D:\steam\steamapps\common\SomeGame\data.pak
   ...
```

\</details\>

\<details\>
\<summary\>\<strong\>Click to see largest\_files.json example\</strong\>\</summary\>

```json
{
  "C:": [
    {
      "Path": "C:\\Users\\Media\\video-project.mp4",
      "Size": 3103784960
    },
    {
      "Path": "C:\\Virtual Machines\\Windows11.vhdx",
      "Size": 1632087572
    }
  ],
  "D:": [
    {
      "Path": "D:\\backups\\archive-2024.zip",
      "Size": 10952166604
    },
    {
      "Path": "D:\\steam\\steamapps\\common\\SomeGame\\data.pak",
      "Size": 4412854272
    }
  ]
}
```

\</details\>

-----

## Building From Source

If you prefer to build the project yourself, you will need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

Clone the repository and run the following command from the root directory to create a trimmed, self-contained, single-file executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

The final executable will be located in the `bin/Release/net8.0/win-x64/publish/` directory.
