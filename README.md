# Regular Expression Engine

This project was created as part of my work in CSCI425 Compiler Design at Colorado School of Mines

## Features
1. Parse a regular expression into an NFA
2. Convert an NFA into a DFA
3. Optimize generated DFAs
4. Test if a specific string matches a Regular Expression (After converting to a DFA)
5. (TODO) Generate the set of all possible matches to a Regular Expression (With specified recursion limit for Kleene Closures)

## Limitations (That I plan on fixing)
1. There is no support for suppressing control characters by doing something like `\(`, `\*`, `\\`
2. There is no support for character sets such as `[a-z]`, `[0-9]`
3. Optimize only removes indistinguishable states, it will remove dead or unreachable states in the future

## Limitations (That I don't plan on fixing)
1. There is no support for any fancy regular expression features such as lookahed or lookbehind (`foo(?=bar)`)

### Disclaimer
No one should ever actually use this, C# provides a much nicer regular expression engine. This was built to test my understanding of course material and help comlete assignments.
