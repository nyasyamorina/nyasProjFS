using NUnit.Framework;
using System.Diagnostics;

namespace SimpleProjFSSample.Test;

class Helpers
{
    public Process? ProviderProcess { get; set; }
    public int WaitTimeoutInMs { get; set; }
    public List<EventWaitHandle> NotificationEvents { get; private set; }


    internal enum NotifyWaitHandleNames {
        FileOpened,
        NewFileCreated,
        FileOverwritten,
        PreDelete,
        PreRename,
        PreCreateHardlink,
        FileRenamed,
        HardlinkCreated,
        FileHandleClosedNoModification,
        FileHandleClosedFileModifiedOrDeleted,
        FilePreConvertToFull,
    }

    public Helpers(int waitTimeoutInMs)
    {
        WaitTimeoutInMs = waitTimeoutInMs;

        // Create the events that the notifications tests use.
        NotificationEvents = [];
        foreach (string eventName in Enum.GetNames<NotifyWaitHandleNames>()) {
            NotificationEvents.Add(new EventWaitHandle(false, EventResetMode.AutoReset, eventName));
        }
    }

    public void StartTestProvider()
    {
        StartTestProvider(out string sourceRoot, out string virtRoot);
    }

    public void StartTestProvider(out string sourceRoot, out string virtRoot)
    {
        GetRootNamesForTest(out sourceRoot, out virtRoot);

        // Get the provider name from the command line.
        var providerExe = TestContext.Parameters.Get("ProviderExe") ?? Path.Combine(AppContext.BaseDirectory, "SimpleProjFSSample.exe");

        // Create an event for the provider to signal once it is up and running.
        EventWaitHandle waitHandle = new(false, EventResetMode.AutoReset, "ProviderTestProceed");

        // Set up the provider process and start it.
        ProviderProcess = new Process();
        ProviderProcess.StartInfo.FileName = providerExe;

        string sourceArg = " --sourceroot " + sourceRoot;
        string virtRootArg = " --virtroot " + virtRoot;

        // Add all the arguments, as well as the "test mode" argument.
        ProviderProcess.StartInfo.Arguments = sourceArg + virtRootArg + " -t";
        ProviderProcess.StartInfo.UseShellExecute = true;

        ProviderProcess.Start();

        // Wait for the provider to signal the event.
        if (!waitHandle.WaitOne(WaitTimeoutInMs)) {
            throw new Exception("SimpleProjFSSample did not signal the ProviderTestProceed event in a timely manner.");
        }
    }

    public void StopTestProvider()
    {
        ProviderProcess?.CloseMainWindow();
    }

    // Makes name strings for the source and virtualization roots for a test, using the NUnit
    // WorkDirectory as the base.
    //
    // Example: Given a test method called "TestStuff", if the test's WorkDirectory is C:\MyTestDir,
    // the output is:
    //      sourceName:     C:\MyTestDir\TestStuff_source
    //      virtRootName:   C:\MyTestDir\TestStuff_virtRoot
    //
    // This method expects to be invoked while a test is running, either from the test case
    // itself or in a setup/teardown fixture for a test case.
    public void GetRootNamesForTest(out string sourceName, out string virtRootName)
    {
        string baseName = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            TestContext.CurrentContext.Test.MethodName ?? ""
        );

        sourceName = baseName + "_source";
        virtRootName = baseName + "_virtRoot";
    }

    public void CreateRootsForTest(out string sourceName, out string virtRootName)
    {
        GetRootNamesForTest(out sourceName, out virtRootName);

        DirectoryInfo sourceInfo = new(sourceName);
        if (!sourceInfo.Exists) {
            sourceInfo.Create();
        }

        // The provider create the virtualization root.
    }

    // Creates a file in the source so that it is projected into the virtualization root.
    // Returns the full path to the virtual file.
    public string CreateVirtualFile(string fileName, string fileContent)
    {
        GetRootNamesForTest(out string sourceRoot, out string virtRoot);

        string sourceFileName = Path.Combine(sourceRoot, fileName);
        FileInfo sourceFile = new(sourceFileName);

        if (!sourceFile.Exists) {
            DirectoryInfo ancestorPath = new(Path.GetDirectoryName(sourceFile.FullName) ?? "");
            if (!ancestorPath.Exists) {
                ancestorPath.Create();
            }

            using StreamWriter sw = sourceFile.CreateText();
            sw.Write(fileContent);
        }

        // Tell our caller what the path to the virtual file is.
        return Path.Combine(virtRoot, fileName);
    }

    // Creates a file in the source so that it is projected into the virtualization root.
    // Returns the full path to the virtual file.
    public string CreateVirtualFile(string fileName)
    {
        return CreateVirtualFile(fileName, "Virtual");
    }

    // Creates a symlink in the source to another file in the source so that it is projected into the virtualization root.
    // Returns the full path to the virtual symlink.
    public string CreateVirtualSymlink(string fileName, string targetName, bool useRootedPaths = false)
    {
        GetRootNamesForTest(out string sourceRoot, out string virtRoot);
        string sourceSymlinkName = Path.Combine(sourceRoot, fileName);
        string sourceTargetName = useRootedPaths ? Path.Combine(sourceRoot, targetName) : targetName;

        if (!File.Exists(sourceSymlinkName)) {
            CreateSymlinkAndAncestor(sourceSymlinkName, sourceTargetName, true);
        }

        return Path.Combine(virtRoot, fileName);
    }

    // Creates a symlink in the source to another directory in the source so that it is projected into the virtualization root.
    // Returns the full path to the virtual symlink.
    public string CreateVirtualSymlinkDirectory(string symlinkDirectoryName, string targetName, bool useRootedPaths = false)
    {
        GetRootNamesForTest(out string sourceRoot, out string virtRoot);
        string sourceSymlinkName = Path.Combine(sourceRoot, symlinkDirectoryName);
        string sourceTargetName = useRootedPaths ? Path.Combine(sourceRoot, targetName) : targetName;

        if (!Directory.Exists(sourceSymlinkName)) {
            CreateSymlinkAndAncestor(sourceSymlinkName, sourceTargetName, false);
        }

        return Path.Combine(virtRoot, symlinkDirectoryName);
    }

    // Create a file in the virtualization root (i.e. a non-projected or "full" file).
    // Returns the full path to the full file.
    public string CreateFullFile(string fileName, string fileContent)
    {
        GetRootNamesForTest(out string sourceRoot, out string virtRoot);

        string fullFileName = Path.Combine(virtRoot, fileName);
        FileInfo fullFile = new(fullFileName);

        if (!fullFile.Exists) {
            DirectoryInfo ancestorPath = new(Path.GetDirectoryName(fullFile.FullName) ?? "");
            if (!ancestorPath.Exists) {
                ancestorPath.Create();
            }

            using StreamWriter sw = fullFile.CreateText();
            sw.Write(fileContent);
        }

        return fullFileName;
    }

    // Create a file in the virtualization root (i.e. a non-projected or "full" file).
    // Returns the full path to the full file.
    public string CreateFullFile(string fileName)
    {
        return CreateFullFile(fileName, "Full");
    }

    public string ReadFileInVirtRoot(string fileName)
    {
        GetRootNamesForTest(out string sourceRoot, out string virtRoot);

        string destFileName = Path.Combine(virtRoot, fileName);
        FileInfo destFile = new FileInfo(destFileName);
        string fileContent;
        using (StreamReader sr = destFile.OpenText()) {
            fileContent = sr.ReadToEnd();
        }

        return fileContent;
    }

    public string ReadReparsePointTargetInVirtualRoot(string symlinkFileName)
    {
        GetRootNamesForTest(out string sourceRoot, out string virtRoot);
        string fullSymlinkName = Path.Combine(virtRoot, symlinkFileName);

        if (!FileSystemApi.TryGetReparsePointTarget(fullSymlinkName, out string? reparsePointTarget)) {
            throw new Exception($"Failed to get a reparse point of {fullSymlinkName}.");
        }

        return reparsePointTarget;
    }

    public FileStream OpenFileInVirtRoot(string fileName, FileMode mode)
    {
        GetRootNamesForTest(out string sourceRoot, out string virtRoot);

        string destFileName = Path.Combine(virtRoot, fileName);
        FileStream stream = File.Open(destFileName, mode);

        return stream;
    }

    private readonly Random random = new();
    public string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private void CreateSymlinkAndAncestor(string sourceSymlinkName, string sourceTargetName, bool isFile)
    {
        DirectoryInfo ancestorPath = new(Path.GetDirectoryName(sourceSymlinkName) ?? "");
        if (!ancestorPath.Exists) {
            ancestorPath.Create();
        }

        if (!FileSystemApi.TryCreateSymbolicLink(sourceSymlinkName, sourceTargetName, isFile)) {
            throw new Exception($"Failed to create directory symlink {sourceSymlinkName}.");
        }
    }
}