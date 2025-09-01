using DataMiner;

namespace MineReports;

public interface IRegressionReport
{
    public List<string> ReportBuffer( IEnumerable<Dust> orderedDusts);
    public static abstract IRegressionReport CreateInstance();

}

