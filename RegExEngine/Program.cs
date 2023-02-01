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