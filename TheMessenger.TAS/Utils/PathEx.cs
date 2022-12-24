using System.IO;
using System.Linq;

namespace TheMessenger.TAS.Utils; 

public static class PathEx {
    public static string Combine(string path, params string[] otherPaths) {
        return otherPaths.Aggregate(path, Path.Combine);
    }
}