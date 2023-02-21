﻿namespace RegExEngine;

class Program
{

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            noArgs();
            Environment.Exit(0);
        }


        List<string> inputFile = new List<string>();
        try
        {
            inputFile = File.ReadLines(args[0]).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(1);
        }

        if (inputFile.Count == 0)
        {
            Environment.Exit(1);
        }

        NFA nfa = new NFA(inputFile, out List<char> alphabet);

        DFA dfa = new DFA(nfa);
        dfa.Print();
        dfa = dfa.Optimize();
        dfa.Print(false);
        
        dfa.OutputToFile(args[1], alphabet);
    }

    public static void noArgs()
    {
        Console.WriteLine("Enter the Regular Expression to parse:");
        string _regEx = Console.ReadLine() ?? "";

        NFA? nfa = null;
        try
        {
            nfa = new NFA(_regEx);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine("Terminating parser");
            Environment.Exit(-1);
        }

        nfa.Print();

        Console.WriteLine("\n\n------\nNow attempting to convert NFA to DFA");

        DFA? dfa = null;
        try
        {
            dfa = new DFA(nfa);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine("Terminating parser");
            Console.Error.WriteLine(e.StackTrace);
            Environment.Exit(-1);
        }

        dfa.Print();

        Console.WriteLine("\n\nNow attempting to merge states");
    
        DFA merged = dfa.Optimize();
        merged.Print(false);
    }
    
}
