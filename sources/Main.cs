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
  class Main
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

    bool IsTerminal()
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

      currentNode.SurroundWithBraces = (_random.Next(0, 2) == 0);

      currentNode.LeftE  = new ExpressionNode(currentNode);
      currentNode.RightE = new ExpressionNode(currentNode);

      currentNode.LeftE.Type  = NodeType.LEFT;
      currentNode.RightE.Type = NodeType.RIGHT;

      currentNode.LeftE.Depth  = currentDepth + 1;
      currentNode.RightE.Depth = currentDepth + 1;

      _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.LeftE, currentDepth + 1));
      _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.RightE, currentDepth + 1));

      // It doesn't seem to have any effect on the outcome for _maxDepth > 2
      /*       
      if (_random.Next(0, 2) == 0)
      {
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.LeftE, currentDepth + 1));
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.RightE, currentDepth + 1));
      }
      else
      {
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.RightE, currentDepth + 1));
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(currentNode.LeftE, currentDepth + 1));
      }
      */
    }

    Stack<KeyValuePair<ExpressionNode, int>> _toProcess = new Stack<KeyValuePair<ExpressionNode, int>>();
    void Process()
    {
      var pair = _toProcess.Pop();

      ExpressionNode currentNode = pair.Key;
      int currentDepth = pair.Value;

      _maxDepthReached = (currentDepth >= _maxDepth);

      if (!_maxDepthReached)
      {
        // If there are no items to process 
        // (e.g. while unwinding root on first step we terminated left branch)
        // and maximum depth not yet reached, 
        // we force generate new expression.
        //
        if (_toProcess.Count == 0)
        {
          CreateNewExpression(currentNode, currentDepth);
        }
        else
        {
          // Standard behaviour otherwise
          if (IsTerminal())
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
        // If we hit the maximum depth already, terminate the expression
        currentNode.Character = GetRandomCharacter(_terminals);
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

      // For _maxDepth = 2 in case when you have terminated branch on first step, 
      // the position of braced expression depends on order of starting elements 
      // pushed onto the stack.
      //
      // I.e. if root is '^' then 
      //
      // (1 + 2) ^ 3 or 1 + (2 ^ 3)
      //
      // depends on whether RightE or LeftE was pushed onto the stack last.
      //
      // For example, we start from '^', then we push LeftE, RightE onto the stack.
      // In Process() RightE will be popped from the stack.
      // Now, suppose it is terminated into '6'. 
      // Then, according to the code, next item from the stack (LeftE) will be forced 
      // to become an expression, because we need to guarantee maximum depth 
      // for at least one branch.
      // And since braces can only surround an expression, we will never get
      // situation when there is a number on the left and braced expression on the right.
      //
      // Same thing will happen if the order of initial elements pushed is reversed,
      // but the whole situation will be reversed as well, i.e. we will never get
      // number on the right and braced expression on the left.
      //
      // So to avoid this, we randomize pushing of starting elements.
      //
      if (_random.Next(0, 2) == 0)
      {
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(root.LeftE, 1));
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(root.RightE, 1));

      }
      else
      {
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(root.RightE, 1));
        _toProcess.Push(new KeyValuePair<ExpressionNode, int>(root.LeftE, 1));
      }

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

    public Main(int maxDepth)
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
