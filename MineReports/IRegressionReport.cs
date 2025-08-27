using DataMiner;

namespace MineReports;

public interface IRegressionReport
{
    public  List<string> ReportBuffer(bool notNaN, IEnumerable<Dust> orderedDusts);
    public static abstract IRegressionReport CreateInstance();

}

