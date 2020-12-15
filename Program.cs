using System;
using MathGen.sources;

namespace MathGen
{
  class Program
  {
    static void Main(string[] args)
    {
      int maxDepth = 3;

#if RELEASE
      if (args.Length == 0)
      {
        string exeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        Console.WriteLine($"Usage: {exeName} <DEPTH>");
        return;
      }

      if (!int.TryParse(args[0], out maxDepth))
      {
        Console.WriteLine("<DEPTH> must be an integer");
        return;
      }

      if (maxDepth <= 0)
      {
        Console.WriteLine("<DEPTH> must be greater than zero");
        return;
      }
#endif

      Console.WriteLine($"Maximum depth: {maxDepth}\n");

      Main m = new Main(maxDepth);
      m.Run();
    }
  }
}
