// Decompiled with JetBrains decompiler
// Type: Trainer_RE5GE.Mem
// Assembly: TrainerRE5GE, Version=3.8.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CD5FA155-FA4C-4D3D-B8A0-6BA32EA9513C
// Assembly location: C:\Users\AMD A6\Desktop\PC Games\Tools\Trainers\Resident Evil 5 Gold Edition Trainer.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Resident_Evil_5_Mod_Tool_by_TylerMods
{
  public class Mem
  {
    public Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();
    private const int PROCESS_CREATE_THREAD = 2;
    private const int PROCESS_QUERY_INFORMATION = 1024;
    private const int PROCESS_VM_OPERATION = 8;
    private const int PROCESS_VM_WRITE = 32;
    private const int PROCESS_VM_READ = 16;
    private const uint MEM_COMMIT = 4096;
    private const uint MEM_RESERVE = 8192;
    private const uint PAGE_READWRITE = 4;
    public static IntPtr pHandle;
    public Process procs;
    private ProcessModule mainModule;

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
    private static extern bool _CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    public static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32", SetLastError = true)]
    internal static extern int WaitForSingleObject(IntPtr handle, int milliseconds);

    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public bool OpenGameProcess(int procID)
    {
      if (procID == 0)
        return false;
      this.procs = Process.GetProcessById(procID);
      if (!this.procs.Responding)
        return false;
      Mem.pHandle = Mem.OpenProcess(2035711U, 1, procID);
      this.mainModule = this.procs.MainModule;
      this.getModules();
      return true;
    }

    public void getModules()
    {
      if (this.procs == null)
        return;
      this.modules.Clear();
      foreach (ProcessModule module in (ReadOnlyCollectionBase) this.procs.Modules)
      {
        if (module.ModuleName != "" && module.ModuleName != null && !this.modules.ContainsKey(module.ModuleName))
          this.modules.Add(module.ModuleName, module.BaseAddress);
      }
    }

    public void setFocus()
    {
      Mem.SetForegroundWindow(this.procs.MainWindowHandle);
    }

    public int getProcIDFromName(string name)
    {
      foreach (Process process in Process.GetProcesses())
      {
        if (process.ProcessName == name)
          return process.Id;
      }
      return 0;
    }

    public string LoadCode(string name, string file)
    {
      StringBuilder lpReturnedString = new StringBuilder(1024);
      if (file != "")
      {
        int privateProfileString = (int) Mem.GetPrivateProfileString("codes", name, "", lpReturnedString, (uint) file.Length, file);
      }
      else
        lpReturnedString.Append(name);
      return lpReturnedString.ToString();
    }

    private int LoadIntCode(string name, string path)
    {
      int int32 = Convert.ToInt32(this.LoadCode(name, path), 16);
      if (int32 >= 0)
        return int32;
      return 0;
    }

    public void ThreadStartClient(object obj)
    {
      using (NamedPipeClientStream pipeClientStream = new NamedPipeClientStream("EQTPipe"))
      {
        if (!pipeClientStream.IsConnected)
          pipeClientStream.Connect();
        using (StreamWriter streamWriter = new StreamWriter((Stream) pipeClientStream))
        {
          if (!streamWriter.AutoFlush)
            streamWriter.AutoFlush = true;
          streamWriter.WriteLine("warp");
        }
      }
    }

    private UIntPtr LoadUIntPtrCode(string name, string path = "")
    {
      string str1 = this.LoadCode(name, path);
      string str2 = str1.Substring(str1.IndexOf('+') + 1);
      if (string.IsNullOrEmpty(str2))
        return (UIntPtr) 0U;
      int num1 = 0;
      if (Convert.ToInt32(str2, 16) > 0)
        num1 = Convert.ToInt32(str2, 16);
      UIntPtr num2;
      if (str1.Contains("base") || str1.Contains("main"))
        num2 = (UIntPtr) ((ulong) ((int) this.mainModule.BaseAddress + num1));
      else if (!str1.Contains("base") && !str1.Contains("main") && str1.Contains("+"))
      {
        string[] strArray = str1.Split('+');
        if (this.modules.Count == 0 || !this.modules.ContainsKey(strArray[0]))
          this.getModules();
        num2 = (UIntPtr) ((ulong) ((int) this.modules[strArray[0]] + num1));
      }
      else
        num2 = (UIntPtr) ((ulong) num1);
      return num2;
    }

    public string CutString(string str)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in str)
      {
        if ((int) ch >= 32 && (int) ch <= 126)
          stringBuilder.Append(ch);
        else
          break;
      }
      return stringBuilder.ToString();
    }

    public string sanitizeString(string str)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in str)
      {
        if ((int) ch >= 32 && (int) ch <= 126)
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public IntPtr AoBScan(uint min, int length, string code, string file = "")
    {
      string[] strArray = this.LoadCode(code, file).Split(' ');
      byte[] btPattern = new byte[strArray.Length];
      string strMask = "";
      int index = 0;
      foreach (string s in strArray)
      {
        if (s == "??")
        {
          btPattern[index] = byte.MaxValue;
          strMask += "?";
        }
        else
        {
          btPattern[index] = byte.Parse(s, NumberStyles.HexNumber);
          strMask += "x";
        }
        ++index;
      }
      return new Mem.SigScan(this.procs, new UIntPtr(min), length).FindPattern(btPattern, strMask, 0);
    }

    public float readFloat(string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      UIntPtr lpBaseAddress = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      if (!Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return 0.0f;
      float num = (float) Math.Round((double) BitConverter.ToSingle(lpBuffer, 0), 2);
      if ((double) num < -99999.0 || (double) num > 99999.0)
        return 0.0f;
      return num;
    }

    public string readString(string code, string file = "")
    {
      byte[] numArray = new byte[32];
      this.getCode(code, file, 4);
      UIntPtr lpBaseAddress = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, numArray, (UIntPtr) 32U, IntPtr.Zero))
        return Encoding.UTF8.GetString(numArray);
      return "";
    }

    public int readUIntPtr(UIntPtr code)
    {
      byte[] lpBuffer = new byte[4];
      if (Mem.ReadProcessMemory(Mem.pHandle, code, lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public int readInt(string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      UIntPtr lpBaseAddress = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public uint readUInt(string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      UIntPtr lpBaseAddress = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return BitConverter.ToUInt32(lpBuffer, 0);
      return 0;
    }

    public int read2ByteMove(string code, int moveQty, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      UIntPtr lpBaseAddress = UIntPtr.Add(this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file), moveQty);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 2U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public int readIntMove(string code, int moveQty, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      UIntPtr lpBaseAddress = UIntPtr.Add(this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file), moveQty);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public ulong readUIntMove(string code, string file, int moveQty)
    {
      byte[] lpBuffer = new byte[8];
      UIntPtr lpBaseAddress = UIntPtr.Add(this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 8) : this.LoadUIntPtrCode(code, file), moveQty);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 8U, IntPtr.Zero))
        return BitConverter.ToUInt64(lpBuffer, 0);
      return 0;
    }

    public int read2Byte(string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      UIntPtr lpBaseAddress = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 2U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public int readByte(string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      UIntPtr lpBaseAddress = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      if (Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public int readPByte(UIntPtr address, string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      if (Mem.ReadProcessMemory(Mem.pHandle, address + this.LoadIntCode(code, file), lpBuffer, (UIntPtr) 1U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public float readPFloat(UIntPtr address, string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      if (Mem.ReadProcessMemory(Mem.pHandle, address + this.LoadIntCode(code, file), lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return (float) Math.Round((double) BitConverter.ToSingle(lpBuffer, 0), 2);
      return 0.0f;
    }

    public int readPInt(UIntPtr address, string code, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      if (Mem.ReadProcessMemory(Mem.pHandle, address + this.LoadIntCode(code, file), lpBuffer, (UIntPtr) 4U, IntPtr.Zero))
        return BitConverter.ToInt32(lpBuffer, 0);
      return 0;
    }

    public string readPString(UIntPtr address, string code, string file = "")
    {
      byte[] numArray = new byte[32];
      if (Mem.ReadProcessMemory(Mem.pHandle, address + this.LoadIntCode(code, file), numArray, (UIntPtr) 32U, IntPtr.Zero))
        return this.CutString(Encoding.ASCII.GetString(numArray));
      return "";
    }

    public bool writeMemory(string code, string type, string write, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      int num = 4;
      UIntPtr lpBaseAddress = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      byte[] numArray;
      if (type == "float")
      {
        lpBuffer = BitConverter.GetBytes(Convert.ToSingle(write));
        num = 4;
      }
      else if (type == "int")
      {
        lpBuffer = BitConverter.GetBytes(Convert.ToInt32(write));
        num = 4;
      }
      else if (type == "byte")
      {
        numArray = new byte[1];
        lpBuffer = BitConverter.GetBytes(Convert.ToInt32(write));
        num = 1;
      }
      else if (type == "string")
      {
        numArray = new byte[write.Length];
        lpBuffer = Encoding.UTF8.GetBytes(write);
        num = write.Length;
      }
      return Mem.WriteProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) ((ulong) num), IntPtr.Zero);
    }

    public bool writeMove(string code, string type, string write, int moveQty, string file = "")
    {
      byte[] lpBuffer = new byte[4];
      int num = 4;
      UIntPtr pointer = this.LoadCode(code, file).Contains(",") ? this.getCode(code, file, 4) : this.LoadUIntPtrCode(code, file);
      byte[] numArray;
      if (type == "float")
      {
        lpBuffer = BitConverter.GetBytes(Convert.ToSingle(write));
        num = 4;
      }
      else if (type == "int")
      {
        lpBuffer = BitConverter.GetBytes(Convert.ToInt32(write));
        num = 4;
      }
      else if (type == "byte")
      {
        numArray = new byte[1];
        lpBuffer = BitConverter.GetBytes(Convert.ToInt32(write));
        num = 1;
      }
      else if (type == "string")
      {
        numArray = new byte[write.Length];
        lpBuffer = Encoding.UTF8.GetBytes(write);
        num = write.Length;
      }
      UIntPtr lpBaseAddress = UIntPtr.Add(pointer, moveQty);
      return Mem.WriteProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) ((ulong) num), IntPtr.Zero);
    }

    public void writeUIntPtr(string code, byte[] write, string file = "")
    {
      Mem.WriteProcessMemory(Mem.pHandle, this.LoadUIntPtrCode(code, file), write, (UIntPtr) ((ulong) write.Length), IntPtr.Zero);
    }

    public void writeByte(UIntPtr code, byte[] write, int size)
    {
      Mem.WriteProcessMemory(Mem.pHandle, code, write, (UIntPtr) ((ulong) size), IntPtr.Zero);
    }

    private UIntPtr getCode(string name, string path, int size = 4)
    {
      string str1 = this.LoadCode(name, path);
      if (str1 == "")
        return UIntPtr.Zero;
      string source = str1;
      if (str1.Contains("+"))
        source = str1.Substring(str1.IndexOf('+') + 1);
      byte[] lpBuffer = new byte[size];
      if (source.Contains<char>(','))
      {
        List<int> intList = new List<int>();
        string str2 = source;
        char[] chArray = new char[1]{ ',' };
        foreach (string str3 in str2.Split(chArray))
          intList.Add(Convert.ToInt32(str3, 16));
        int[] array = intList.ToArray();
        if (str1.Contains("base") || str1.Contains("main"))
          Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr) ((ulong) ((int) this.mainModule.BaseAddress + array[0])), lpBuffer, (UIntPtr) ((ulong) size), IntPtr.Zero);
        else if (!str1.Contains("base") && !str1.Contains("main") && str1.Contains("+"))
        {
          IntPtr module = this.modules[str1.Split('+')[0]];
          Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr) ((ulong) ((int) module + array[0])), lpBuffer, (UIntPtr) ((ulong) size), IntPtr.Zero);
        }
        else
          Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr) ((ulong) array[0]), lpBuffer, (UIntPtr) ((ulong) size), IntPtr.Zero);
        uint uint32 = BitConverter.ToUInt32(lpBuffer, 0);
        UIntPtr lpBaseAddress = (UIntPtr) 0U;
        for (int index = 1; index < array.Length; ++index)
        {
          lpBaseAddress = new UIntPtr(uint32 + Convert.ToUInt32(array[index]));
          Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr) ((ulong) size), IntPtr.Zero);
          uint32 = BitConverter.ToUInt32(lpBuffer, 0);
        }
        return lpBaseAddress;
      }
      int int32 = Convert.ToInt32(source, 16);
      if (str1.Contains("base") || str1.Contains("main"))
        Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr) ((ulong) ((int) this.mainModule.BaseAddress + int32)), lpBuffer, (UIntPtr) ((ulong) size), IntPtr.Zero);
      else if (!str1.Contains("base") && !str1.Contains("main") && str1.Contains("+"))
      {
        IntPtr module = this.modules[str1.Split('+')[0]];
        Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr) ((ulong) ((int) module + int32)), lpBuffer, (UIntPtr) ((ulong) size), IntPtr.Zero);
      }
      else
        Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr) ((ulong) int32), lpBuffer, (UIntPtr) ((ulong) size), IntPtr.Zero);
      UIntPtr num = new UIntPtr(BitConverter.ToUInt32(lpBuffer, 0));
      BitConverter.ToUInt32(lpBuffer, 0);
      return num;
    }

    public void closeProcess()
    {
      Mem.CloseHandle(Mem.pHandle);
    }

    //public unsafe void InjectDLL(string strDLLName)
    //{
    //  foreach (ProcessModule module in (ReadOnlyCollectionBase) this.procs.Modules)
    //  {
    //    if (module.ModuleName.StartsWith("inject", StringComparison.InvariantCultureIgnoreCase))
    //      return;
    //  }
    //  if (!this.procs.Responding)
    //    return;
    //  int num1 = strDLLName.Length + 1;
    //  IntPtr num2 = Mem.VirtualAllocEx(Mem.pHandle, (IntPtr) ((void*) null), (uint) num1, 12288U, 4U);
    //  IntPtr num3;
    //  Mem.WriteProcessMemory(Mem.pHandle, num2, strDLLName, (UIntPtr) ((ulong) num1), out num3);
    //  UIntPtr procAddress = Mem.GetProcAddress(Mem.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
    //  IntPtr remoteThread = Mem.CreateRemoteThread(Mem.pHandle, (IntPtr) ((void*) null), 0U, procAddress, num2, 0U, out num3);
    //  switch (Mem.WaitForSingleObject(remoteThread, 10000))
    //  {
    //    case 128:
    //    case 258:
    //      Mem.CloseHandle(remoteThread);
    //      break;
    //    default:
    //      Mem.VirtualFreeEx(Mem.pHandle, num2, (UIntPtr) 0U, 32768U);
    //      Mem.CloseHandle(remoteThread);
    //      break;
    //  }
    //}

    public class SigScan
    {
      private byte[] m_vDumpedRegion;
      private Process m_vProcess;
      private UIntPtr m_vAddress;
      private int m_vSize;

      public Process Process
      {
        get
        {
          return this.m_vProcess;
        }
        set
        {
          this.m_vProcess = value;
        }
      }

      public UIntPtr Address
      {
        get
        {
          return this.m_vAddress;
        }
        set
        {
          this.m_vAddress = value;
        }
      }

      public int Size
      {
        get
        {
          return this.m_vSize;
        }
        set
        {
          this.m_vSize = value;
        }
      }

      public SigScan()
      {
        this.m_vProcess = (Process) null;
        this.m_vAddress = UIntPtr.Zero;
        this.m_vSize = 0;
        this.m_vDumpedRegion = (byte[]) null;
      }

      public SigScan(Process proc, UIntPtr addr, int size)
      {
        this.m_vProcess = proc;
        this.m_vAddress = addr;
        this.m_vSize = size;
      }

      [DllImport("kernel32.dll", SetLastError = true)]
      private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

      private bool DumpMemory()
      {
        try
        {
          if (this.m_vProcess == null || this.m_vProcess.HasExited || (this.m_vAddress == UIntPtr.Zero || this.m_vSize == 0))
            return false;
          this.m_vDumpedRegion = new byte[this.m_vSize];
          int lpNumberOfBytesRead;
          return Mem.SigScan.ReadProcessMemory(this.m_vProcess.Handle, this.m_vAddress, this.m_vDumpedRegion, this.m_vSize, out lpNumberOfBytesRead) && lpNumberOfBytesRead == this.m_vSize;
        }
        catch (Exception ex)
        {
          return false;
        }
      }

      private bool MaskCheck(int nOffset, IEnumerable<byte> btPattern, string strMask)
      {
        return !btPattern.Where<byte>((Func<byte, int, bool>) ((t, x) =>
        {
          if ((int) strMask[x] != 63 && (int) strMask[x] == 120)
            return (int) t != (int) this.m_vDumpedRegion[nOffset + x];
          return false;
        })).Any<byte>();
      }

      public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
      {
        try
        {
          if ((this.m_vDumpedRegion == null || this.m_vDumpedRegion.Length == 0) && !this.DumpMemory() || strMask.Length != btPattern.Length)
            return IntPtr.Zero;
          for (int nOffset1 = 0; nOffset1 < this.m_vDumpedRegion.Length; ++nOffset1)
          {
            if (this.MaskCheck(nOffset1, (IEnumerable<byte>) btPattern, strMask))
              return new IntPtr((int) (uint) this.m_vAddress + (nOffset1 + nOffset));
          }
          return IntPtr.Zero;
        }
        catch (Exception ex)
        {
          return IntPtr.Zero;
        }
      }

      public void ResetRegion()
      {
        this.m_vDumpedRegion = (byte[]) null;
      }
    }
  }
}
