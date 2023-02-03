using RegExEngine;

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

