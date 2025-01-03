using System.IO;

namespace DbbInstaGenerator.Interfaces;

public interface IShareService
{
    public void Share(MemoryStream inStream);

    public void ShareB(MemoryStream inStream);
}