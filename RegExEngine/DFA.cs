using System.Diagnostics;
using System.Text;

namespace RegExEngine;

public class DFA
{
    private int numStates = 0;
    private List<bool> acceptingStates = new List<bool>();
    private Dictionary<string, int> stateSets = new Dictionary<string, int>();
    private List<SortedDictionary<char, int>> transitions = new List<SortedDictionary<char, int>>();
    private HashSet<char> alphabet = new HashSet<char>();


    private bool stateExists(SortedSet<int> stateSet)
    {
        return stateSets.ContainsKey(String.Join(' ', stateSet));
    }
    private int initState(SortedSet<int> stateSet, NFA nfa)
    {
        int stateNum = numStates;
        numStates++;
        bool isAccepting = false;
        foreach (int i in stateSet)
            if (nfa.acceptingStates[i])
            {
                isAccepting = true;
                break;
            }
        acceptingStates.Add(isAccepting);
        stateSets[String.Join(' ', stateSet)] = stateNum;
        transitions.Add(new SortedDictionary<char, int>());
        return stateNum;
    }

    public int initBlankState(bool isAccepting = false)
    {
        int stateNum = numStates;
        numStates++;
        acceptingStates.Add(isAccepting);
        transitions.Add(new SortedDictionary<char, int>());
        return stateNum;
    }

    private SortedSet<int> FollowLambda(SortedSet<int> currentStates, NFA nfa)
    {
        Stack<int> searchPath = new Stack<int>();
        foreach (int i in currentStates) searchPath.Push(i);

        while (searchPath.Count != 0)
        {
            int transition = searchPath.Pop();
            foreach (int state in nfa.lambdaTransitions[transition])
            {
                if (!currentStates.Contains(state))
                {
                    currentStates.Add(state);
                    searchPath.Push(state);
                }
            }
        }

        return currentStates;
    }

    private SortedSet<int> FollowChar(char c, SortedSet<int> currentStates, NFA nfa)
    {
        SortedSet<int> newStates = new SortedSet<int>();
        foreach (int s in currentStates)
        {
            if (!nfa.letterTransitions[s].ContainsKey(c)) continue;

            foreach (var transition in nfa.letterTransitions[s][c])
            {
                newStates.Add(transition);
            }
        }

        return newStates;
    }

    private DFA() { }

    public DFA(NFA nfa)
    {
        for (int i = 0; i < nfa.numStates; i++)
        {
            foreach (var t in nfa.letterTransitions[i])
            {
                alphabet.Add(t.Key);
            }
        }

        Stack<SortedSet<int>> searchList = new Stack<SortedSet<int>>();
        SortedSet<int> startingState = new SortedSet<int>() { 0 };
        startingState = FollowLambda(startingState, nfa);
        initState(startingState, nfa);
        searchList.Push(startingState);

        while (searchList.Count != 0)
        {
            var stateList = searchList.Pop();
            int stateId = stateSets[String.Join(' ', stateList)];
            foreach (char c in alphabet)
            {
                SortedSet<int> transitionTo = FollowLambda(FollowChar(c, stateList, nfa), nfa);
                if (transitionTo.Count == 0) continue;

                int transitionToState;
                if (!stateExists(transitionTo))
                {
                    transitionToState = initState(transitionTo, nfa);
                    searchList.Push(transitionTo);
                }
                else
                {
                    transitionToState = stateSets[String.Join(' ', transitionTo)];
                }

                transitions[stateId].Add(c, transitionToState);
            }
        }
    }

    public Dictionary<int, int> getMergableStates()
    {
        Dictionary<string, int> seenStates = new Dictionary<string, int>();
        Dictionary<int, int> statesToMerge = new Dictionary<int, int>();

        for (int i = 0; i < numStates; i++)
        {
            string tRepresentation = $"{(acceptingStates[i] ? "true" : "false")} ";
            foreach (var t in transitions[i])
            {
                tRepresentation += $"{t.Key}:{t.Value} ";

            }
            if (!seenStates.ContainsKey(tRepresentation))
            {
                seenStates[tRepresentation] = i;
            }
            else
            {
                statesToMerge[i] = seenStates[tRepresentation];
            }
        }

        return statesToMerge;
    }

    public (bool status, DFA? merged) mergeStates()
    {
        var mergableStates = getMergableStates();
        if (mergableStates.Count == 0) return (false, null);

        DFA after = new DFA();
        foreach (char c in this.alphabet) after.alphabet.Add(c);

        Dictionary<int, int> mapping = new Dictionary<int, int>();

        for (int i = 0; i < this.numStates; i++)
        {
            if (mergableStates.ContainsKey(i))
            {
                continue;
            }

            int thisNode;
            if (mapping.ContainsKey(i))
            {
                thisNode = mapping[i];
            }
            else
            {
                thisNode = after.initBlankState(acceptingStates[i]);
                mapping[i] = thisNode;
            }

            foreach (var t in this.transitions[i])
            {
                int transitionTo = t.Value;
                if (mergableStates.ContainsKey(transitionTo))
                {
                    transitionTo = mergableStates[transitionTo];
                }

                if (mapping.ContainsKey(transitionTo))
                {
                    transitionTo = mapping[transitionTo];
                }
                else
                {
                    int newNode = after.initBlankState(acceptingStates[transitionTo]);
                    mapping[transitionTo] = newNode;
                    transitionTo = newNode;
                }
                
                after.transitions[thisNode].Add(t.Key, transitionTo);
            }
        }

        return (true, after);
    }

    public DFA Optimize()
    {
        DFA merged = this;
        (bool state, DFA? altered) mergeState = (false, null);
        do
        {
            mergeState = merged.mergeStates();
            if (mergeState.state) merged = mergeState.altered!;
        } while (mergeState.state);

        return merged;
    }

    public void Print(bool showMapping = true)
    {
        for (int i = 0; i < numStates; i++)
        {
            Console.Write($"State {i}: ");
            if (i == 0) Console.Write("Start, ");
            Console.Write(acceptingStates[i] ? "Accepting\t " : "Not Accepting\t ");
            Console.Write("Transitions: {");

            int j = 0;
            int count = transitions[i].Count - 1;
            foreach (var state in transitions[i])
            {
                Console.Write($"{{{state.Key}, {String.Join(", ", state.Value)}}}");
                if (j < count) Console.Write(", ");
                j++;
            }
            Console.WriteLine("}");
        }

        if (!showMapping) return;

        Console.WriteLine("\n------\nNFA->DFA mapping");
        foreach (var m in stateSets)
        {
            Console.WriteLine($"{{{m.Key.Replace(" ", ", ")}}} -> State {m.Value}");
        }
    }

    public void OutputToFile(string fileName, List<char> alphabetOrder)
    {
        var file = File.OpenWrite(fileName);
        for (int i = 0; i < numStates; i++)
        {
            string line = "";
            line += acceptingStates[i] ? "+ " : "- ";
            line += i.ToString() + ' ';
            foreach (char c in alphabetOrder)
            {
                if (transitions[i].ContainsKey(c))
                {
                    line += $"{transitions[i][c]} ";
                }
                else
                {
                    line += "E ";
                }
            }

            line = line.TrimEnd();
            line += '\n';
            file.Write(Encoding.ASCII.GetBytes(line), 0, line.Length);
        }
        file.Flush();
        file.Close();
    }
}