using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Server;

public class PcManager
{
    private Dictionary<int, PcInfo> _pcs = new Dictionary<int, PcInfo>();
    public int ControlledPcIndex { get; private set; } = -1;

    public void SetControlledPcIndex(int index)
    {
        ControlledPcIndex = index;
        Console.WriteLine($"Now controlling PC with index: {ControlledPcIndex}");
        //Log.Instance.FileLog(Log.LogId.ITEM, Log.LogLevel.TRC_DATA, $"Now controlling PC with index: {ControlledPcIndex}");
    }

    public PcInfo GetControlledPc()
    {
        return GetPc(ControlledPcIndex);
    }

    public void UpdatePc(PcInfoBr pcInfo)
    {
        if (!_pcs.ContainsKey(pcInfo.Index))
        {
            _pcs[pcInfo.Index] = new PcInfo();
        }
        var pc = _pcs[pcInfo.Index];
        bool changed = false;

        if (pc.Index != pcInfo.Index)
        {
            Console.WriteLine($"PC {pcInfo.Index}: Index changed from {pc.Index} to {pcInfo.Index}");
            pc.Index = pcInfo.Index;
            changed = true;
        }

        if (changed && pcInfo.Index == ControlledPcIndex)
        {
            Console.WriteLine($"Updated controlled PC: {pc}");
        }
    }

    public void UpdatePc(PcInfo pcInfo)
    {
        if (!_pcs.ContainsKey(pcInfo.Index))
        {
            _pcs[pcInfo.Index] = new PcInfo();
        }
        var pc = _pcs[pcInfo.Index];
        bool changed = false;

        if (pc.Index != pcInfo.Index)
        {
            Console.WriteLine($"PC {pcInfo.Index}: Index changed from {pc.Index} to {pcInfo.Index}");
            pc.Index = pcInfo.Index;
            changed = true;
        }

        if (changed && pcInfo.Index == ControlledPcIndex)
        {
            Console.WriteLine($"Updated controlled PC: {pc}");
        }
    }

    public PcInfo GetPc(int index)
    {
        return _pcs.TryGetValue(index, out var pc) ? pc : null;
    }

    public void RemovePc(int index)
    {
        _pcs.Remove(index);
    }

    public void Clear()
    {
        _pcs.Clear();
    }

    public IEnumerable<PcInfo> GetAllPcs()
    {
        return _pcs.Values;
    }
}