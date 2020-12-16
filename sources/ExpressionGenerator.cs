using System;
using System.Collections.Generic;
using System.Text;

// 15.12.2020 (23:49) by xterminal86
//
// Generate random expression based on the rules below. 
// Braces are randomized.
//
// 1) [O] = [+-*/^]
// 2) [E] = ([E] [O] [E]) | [1-9]
// 3) [F] = ([E] [O] [E])
//
// E.g.:
//
// F = ([E] [O] [E]) = (([E] [O] [E]) [O] [E]) = 
//                   = ((1 [O] 3) [O] [E])     =
//                   = ((1 + 3) [O] 4)         =
//                   = ((1 + 3) * 4)           
//
//                    (1 + 3) * 4         
//                    
//                         *
//                        / \
//                       E   E
//                      /    |
//       (1 + 3)       +     |
//                    / \    |
//                   E   E   4
//                   |   | 
//                   1   3    
//
//

namespace MathGen.sources
{
  class ExpressionGenerator
  {
    int _maxDepth = 1;

    bool _maxDepthReached = false;

#if DEBUG
    //Random _random = new Random(10);
    Random _random = new Random();
#else
    Random _random = new Random();
#endif

    List<string> _operators = new List<string>()
    {
      "+", "-", "*", "/", "^"
    };

    List<string> _terminals = new List<string>()
    {
      "1", "2", "3", "4", "5", "6", "7", "8", "9",

      // Because why not?
      "-1", "-2", "-3", "-4", "-5", "-6", "-7", "-8", "-9",
    };

    bool FiftyFifty()
    {
      return (_random.Next(0, 2) == 0);
    }

    string GetRandomCharacter(List<string> collection)
    {
      return collection[_random.Next(0, collection.Count)];
    }

    void CreateNewExpression(ExpressionNode currentNode, int currentDepth)
    {      
      currentNode.Character = GetRandomCharacter(_operators);

      currentNode.SurroundWithBraces = FiftyFifty();

      currentNode.LeftE  = new ExpressionNode(currentNode);
      currentNode.RightE = new ExpressionNode(currentNode);

      currentNode.LeftE.Type  = NodeType.LEFT;
      currentNode.RightE.Type = NodeType.RIGHT;

      currentNode.LeftE.Depth  = currentDepth + 1;
      currentNode.RightE.Depth = currentDepth + 1;

      _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.LeftE, currentDepth + 1));
      _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.RightE, currentDepth + 1));      
    }

    Stack<KeyValuePair<ExpressionNode, int>> _toProcess = new Stack<KeyValuePair<ExpressionNode, int>>();
    void Process()
    {
      var pair = _toProcess.Pop();

      ExpressionNode currentNode = pair.Key;
      int currentDepth = pair.Value;

      // currentDepth is the depth of current node, which may be still
      // less than _maxDepth. In such case we still could either generate an expression
      // or terminate the node.
      //
      // For example, we start from '^'. Consider _maxDepth = 2.
      //
      //          ^         (depth 0)
      //    LeftE   RightE  (depth 1)
      //
      // Then we push LeftE, RightE onto the stack. (see Run())
      // At frist step RightE will be popped from the stack.
      // Now, suppose it is terminated into '6'. 
      //
      //          ^         (depth 0)
      //    LeftE   6       (depth 1)
      //
      // Then, according to the code, next item from the stack (LeftE) will be forced 
      // to become an expression, because we need to guarantee maximum depth 
      // for at least one branch.
      // And since braces can only surround an expression, we will never get
      // situation when there is a number on the left and braced expression on the right.
      //
      // On the other hand, if RightE was made into expression on the first step,
      //
      //           ^         (depth 0)
      //    LeftE    *       (depth 1)
      //           3   4     (depth 2)
      //                
      // after we unwind the stack container back to get the LeftE that was pushed
      // after root node was created, its depth is still less than _maxDepth
      // and it still can either become a terminal or an expression.
      //
      // To ensure this scenario we use the following one-time flip check

      if (!_maxDepthReached && (currentDepth >= _maxDepth))
      {
        _maxDepthReached = true;
      }

      if (!_maxDepthReached)
      {
        // If there are no items to process 
        // (e.g. while unwinding root on first step we terminated left branch)
        // and maximum depth is not yet reached, we force generate new expression.
        //
        if (_toProcess.Count == 0)
        {
          CreateNewExpression(currentNode, currentDepth);
        }
        else
        {
          // Standard behaviour otherwise
          if (FiftyFifty())
          {
            currentNode.Character = GetRandomCharacter(_terminals);
          }
          else
          {
            CreateNewExpression(currentNode, currentDepth);
          }
        }
      }
      else
      {
        // In case current node's depth is less than _maxDepth, but
        // _maxDepth was reached nonetheless, we still should maintain
        // standard behaviour of node generation
        if (currentDepth < _maxDepth)
        {
          if (FiftyFifty())
          {
            CreateNewExpression(currentNode, currentDepth);
          }
          else
          {
            currentNode.Character = GetRandomCharacter(_terminals);
          }
        }
        else
        {
          // Otherwise just terminate it
          currentNode.Character = GetRandomCharacter(_terminals);
        }
      }
    }

    public void Run()
    {     
      ExpressionNode root = new ExpressionNode();

      root.Type = NodeType.ROOT;
      root.Depth = 0;
      root.Character = GetRandomCharacter(_operators);
      
      root.LeftE  = new ExpressionNode(root);
      root.RightE = new ExpressionNode(root);

      root.LeftE.Type  = NodeType.LEFT;
      root.RightE.Type = NodeType.RIGHT;

      root.RightE.Depth = 1;
      root.LeftE.Depth  = 1;
                 
      _toProcess.Push(new KeyValuePair<ExpressionNode, int>(root.LeftE, 1));
      _toProcess.Push(new KeyValuePair<ExpressionNode, int>(root.RightE, 1));

      while (_toProcess.Count != 0)
      {
        Process();
      }

#if DEBUG
      PrintTree(root);
#endif

      FoldTree(root);

      Console.WriteLine($"Resulting expression:\n{root.Character}\n");
    }

    void FoldTree(ExpressionNode tree)
    {
      Stack<ExpressionNode> toProcess = new Stack<ExpressionNode>();

      toProcess.Push(tree);

      while (toProcess.Count != 0)
      {
        if (_maxDepth <= 1)
        {
          break;
        }

        var node = toProcess.Pop();
                
        if (!node.IsFinal())
        {
          // In case of, e.g.,
          //    
          //     +
          //    / \
          //   *   5
          //  / \
          // 3  4
          //
          // Node '+' is not final, so 5 gets pushed on to the stack.
          // When it will be popped sometime in the future,
          // we will go to the 'else' conditional branch, where will will crash and burn
          // on trying to access LeftE and RIghtE, since they're null.
          // So we either don't do anything here, if they're null, or conditionalize
          // everything in the 'else' branch. 
          //
          // I preferred the former to keep code kinda pretty and concise.
          //
          if (node.LeftE != null)
          {
            toProcess.Push(node.LeftE);
          }

          if (node.RightE != null)
          {
            toProcess.Push(node.RightE);
          }
        }
        else
        {          
          string resExp = $"{node.LeftE.Character} {node.Character} {node.RightE.Character}";
          node.Character = (node.SurroundWithBraces ? $"({resExp})" : resExp);
          node.LeftE = null;
          node.RightE = null;
        }

        // If tree is not folded yet, repeat the process
        if (toProcess.Count == 0 && !tree.IsFinal())
        {
          toProcess.Push(tree);
        }        
      }

      // Finally, fold the root node
      tree.Character = $"{tree.LeftE.Character} {tree.Character} {tree.RightE.Character}";
    }
        
    void PrintTree(ExpressionNode tree)
    {
      Stack<ExpressionNode> toPrint = new Stack<ExpressionNode>();

      toPrint.Clear();

      toPrint.Push(tree);

      Console.WriteLine("");

      while (toPrint.Count != 0)
      {
        var item = toPrint.Pop();
                
        string depth = new string('_', item.Depth);
        Console.WriteLine($"{depth}{item.Character} ({item.Type})");

        if (item.LeftE != null && item.RightE != null)
        {          
          toPrint.Push(item.LeftE);
          toPrint.Push(item.RightE);
        }        
      }

      Console.WriteLine("\n");
    }

    public ExpressionGenerator(int maxDepth)
    {
      _maxDepth = maxDepth;
    }
  }                

  enum NodeType
  {
    ROOT = 0,
    LEFT,
    RIGHT,
    UNDEFINED
  }

  class ExpressionNode
  {    
    public ExpressionNode(ExpressionNode parent = null)
    {
      Parent = parent;
    }

    // 'Final expression' means that this node contains only terminal symbols    
    // in LeftE and RightE, e.g.:
    //
    //   +  <- (this is considered to be final node)
    //  / \
    // 4   3
    //
    public bool IsFinal()
    {
      return (LeftE != null && LeftE.IsTerminal() 
           && RightE != null && RightE.IsTerminal());
    }

    public bool IsTerminal()
    {
      return (LeftE == null && RightE == null);
    }

    public string   Character   = string.Empty;
    public int      Depth       = 0;
    public NodeType Type        = NodeType.UNDEFINED;

    public bool     SurroundWithBraces = false;

    public ExpressionNode Parent = null;
    public ExpressionNode LeftE  = null;
    public ExpressionNode RightE = null;    
  }
}
