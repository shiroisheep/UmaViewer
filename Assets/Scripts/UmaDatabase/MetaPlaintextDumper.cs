using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class MetaPlaintextDumper
{
    private const string DLL = "sqlite3mc_x64";

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr sqlite3_backup_init(
        IntPtr pDest,
        string zDestName,
        IntPtr pSource,
        string zSourceName
    );

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern int sqlite3_backup_step(
        IntPtr pBackup,
        int nPage
    );

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern int sqlite3_backup_finish(
        IntPtr pBackup
    );

    public static void Dump(IntPtr srcDb, string outputPath)
    {
        if (srcDb == IntPtr.Zero)
            throw new ArgumentException("Source DB handle is null");

        if (File.Exists(outputPath))
        {
            Debug.Log("[MetaPlaintextDumper] meta_dec.db already exists");
            return;
        }

        IntPtr dstDb = IntPtr.Zero;
        IntPtr backup = IntPtr.Zero;

        try
        {
            dstDb = Sqlite3MC.Open(outputPath);

            backup = sqlite3_backup_init(dstDb, "main", srcDb, "main");
            if (backup == IntPtr.Zero)
                throw new Exception("sqlite3_backup_init failed");

            sqlite3_backup_step(backup, -1);
            sqlite3_backup_finish(backup);
            backup = IntPtr.Zero;

            Debug.Log($"[MetaPlaintextDumper] meta dumped to {outputPath}");
        }
        finally
        {
            if (backup != IntPtr.Zero)
                sqlite3_backup_finish(backup);

            if (dstDb != IntPtr.Zero)
                Sqlite3MC.Close(dstDb);
        }
    }
}
