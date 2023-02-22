namespace RegExEngine;

public class NFA
{
    public int numStates;
    public List<bool> acceptingStates;
    public List<HashSet<int>> lambdaTransitions;
    public List<Dictionary<char, HashSet<int>>> letterTransitions;

    private int initNewState(bool accepting = false)
    {
        int newState = numStates;
        numStates++;
        letterTransitions.Add(new Dictionary<char, HashSet<int>>());
        lambdaTransitions.Add(new HashSet<int>());
        acceptingStates.Add(accepting);
        return newState;
    }
    
    public NFA(string regularExpression)
    {
        this.acceptingStates = new List<bool>();
        this.lambdaTransitions = new List<HashSet<int>>();
        this.letterTransitions = new List<Dictionary<char, HashSet<int>>>();
        numStates = 0;
        
        Stack<KeyValuePair<int, int?>> scope = new Stack<KeyValuePair<int, int?>>();
        int workingNode = initNewState();

        Console.WriteLine($"Attempting to parse \"{regularExpression}\" into an NFA");
        
        scope.Push(new KeyValuePair<int, int?>(workingNode, null));
        
        for (int i = 0; i < regularExpression.Length; i++)
        {
            char currentChar = regularExpression[i];
            char? nextChar = null;
            if (i != regularExpression.Length - 1)
            {
                nextChar = regularExpression[i + 1];
            }
            
            //We now start parsing
            if (currentChar == '(')
            {
                int newState = initNewState();
                scope.Push(new KeyValuePair<int, int?>(newState, null));
                lambdaTransitions[workingNode].Add(newState);
                workingNode = newState;
            }
            else if (currentChar == ')')
            {
                if (scope.Count == 1)
                {
                    throw new FormatException("Found ) with no matching (");
                }
                
                KeyValuePair<int, int?> scopeState = scope.Pop();
                int endState = scopeState.Value ?? initNewState();
                if (nextChar == '*')
                {
                    lambdaTransitions[endState].Add(scopeState.Key);
                    lambdaTransitions[scopeState.Key].Add(endState);
                    i++; //Skip * in parsing
                }
                else if (nextChar == '+')
                {
                    lambdaTransitions[endState].Add(scopeState.Key);
                    i++; //skip + in the parsing
                }

                lambdaTransitions[workingNode].Add(endState);
                workingNode = endState;
            }
            else if (currentChar == '|')
            {
                var currentScope = scope.Pop();
                int endState = currentScope.Value ?? initNewState();
                scope.Push(new KeyValuePair<int, int?>(currentScope.Key, endState));

                lambdaTransitions[workingNode].Add(endState);
                workingNode = currentScope.Key;
            }
            else if (currentChar is '*' or '+')
            {
                throw new FormatException("Encountered an unexpected * or +");
            }
            else
            {
                int newState = initNewState();

                if (nextChar == '*')
                {
                    lambdaTransitions[workingNode].Add(newState);
                    letterTransitions[newState][currentChar] = new HashSet<int>();
                    letterTransitions[newState][currentChar].Add(newState);
                    i++;
                }
                else if (nextChar == '+')
                {
                    if (!letterTransitions[workingNode].ContainsKey(currentChar))
                        letterTransitions[workingNode][currentChar] = new HashSet<int>();
                    letterTransitions[workingNode][currentChar].Add(newState);
                    
                    letterTransitions[newState][currentChar] = new HashSet<int>();
                    letterTransitions[newState][currentChar].Add(newState);
                    i++;
                }
                else
                {
                    if (!letterTransitions[workingNode].ContainsKey(currentChar))
                        letterTransitions[workingNode][currentChar] = new HashSet<int>();
                    letterTransitions[workingNode][currentChar].Add(newState);
                }

                workingNode = newState;
            }
        }

        if (scope.Count != 1)
        {
            throw new FormatException("Found ( with no matching )");
        }

        var finalScope = scope.Pop();

        if (finalScope.Value != null)
        {
            lambdaTransitions[workingNode].Add(finalScope.Value ?? workingNode);
            acceptingStates[finalScope.Value ?? workingNode] = true;
        }
        else
        {
            acceptingStates[workingNode] = true;
            Console.WriteLine("Finished Constructing NFA with no errors");
        }
    }

    public NFA(List<string> lines, out List<char> alphabet)
    {
        Dictionary<int, int> mapping = new Dictionary<int, int>();
        int currentState = 0;

        string[] firstLine = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
        char lambdaChar = firstLine[1][0];
        alphabet = new List<char>();
        for (int i = 2; i < firstLine.Length; i++)
        {
            alphabet.Add(firstLine[i][0]);
        }

        numStates = int.Parse(firstLine[0]);
        this.acceptingStates = new List<bool>(numStates);
        this.lambdaTransitions = new List<HashSet<int>>(numStates);
        this.letterTransitions = new List<Dictionary<char, HashSet<int>>>(numStates);
        for (int i = 0; i < numStates; i++)
        {
            acceptingStates.Add(false);
            lambdaTransitions.Add(new HashSet<int>());
            letterTransitions.Add(new Dictionary<char, HashSet<int>>());
        }

        for (int i = 1; i < lines.Count; i++)
        {
            if(lines[i].Length == 0) continue;
            
            string[] split = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int from = int.Parse(split[1]);
            int to = int.Parse(split[2]);

            if(!mapping.ContainsKey(from)){
                mapping[from] = currentState;
                from = currentState;
                currentState++;
            }
            else{
                from = mapping[from];
            }

            if(!mapping.ContainsKey(to)){
                mapping[to] = currentState;
                to = currentState;
                currentState++;
            }
            else{
                to = mapping[to];
            }


            acceptingStates[from] = split[0] == "+";
            for (int j = 3; j < split.Length; j++)
            {
                char c = split[j][0];
                if (c == lambdaChar)
                {
                    lambdaTransitions[from].Add(to);
                }
                else
                {
                    if (!letterTransitions[from].ContainsKey(c))
                    {
                        letterTransitions[from][c] = new HashSet<int>();
                    }

                    letterTransitions[from][c].Add(to);
                }
            }
        }
    }
    public void Print()
    {
        for (int i = 0; i < numStates; i++)
        {
            Console.Write($"State {i}: ");
            if(i == 0) Console.Write("Start, ");
            Console.Write(acceptingStates[i] ? "Accepting\t " : "Not Accepting\t ");
            Console.Write($"Lambda Transitions: {{{String.Join(", ", lambdaTransitions[i])}}}\t ");
            Console.Write("Letter Transitions: {");
            
            int j = 0;
            int count = letterTransitions[i].Count - 1;
            foreach (var state in letterTransitions[i])
            {
                Console.Write($"{{{state.Key}, {String.Join(", ", state.Value)}}}");
                if(j < count) Console.Write(", ");
                j++;
            }
            Console.WriteLine("}");
        }
    }
}