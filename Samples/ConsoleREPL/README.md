# Console REPL

This sample uses the Power Fx core packages to implement a simple Read-Eval-Print-Loop in a console app.  It supports defining formulas that recalc automatically and creating and modifying variables with the Set function.

This version builds against Power Fx public nugets. 
For a daily version that builds directly in the Power Fx project, see:  
https://github.com/microsoft/Power-Fx/blob/main/src/tools/Repl/Program.cs

## Usage

After building, run ConsoleREPL from the command line.

At the prompt, enter `Help()` to display helpful information.  The available function list will vary as functions are added to the core and hosts can add their own functions too (as this sample does).

Use the `Set` function to create or change a variable's value.  Variables are typed based on the type of the expression that is fed into them and subsequent Set operations must reaffirm that type.  For example, `Set( Name, "Fred" )` defines the `Name` variable of type string and sets it to `Fred`.

Use the `Identifier = ...` to define a formula, similar to defining a Name in Excel.  A formula can be used like a variable, but it has two unique qualities:
- It can't be redefined after it is set, it is immutable.
- It will recalculate automatically as dependecency in its definition change.  
For example `Force = mass * acceleration` will recalculate the `Force` automatically if `mass` or `acceleration` change.

Use an expression on a line by itself to have it evalated.

Records are written with { } as in JSON but without quotes around the field name.  For example: `{ Name: "Joe", Age: 29 }`.

Tables are written with the Table function, records within.  For example: `Table( { Name: "Joe", Age: 29 }, { Name: "Sally", Age: 30 } )`.

There are no arrays in Power Fx, everything is expressed as a table.  But, one can define a single column table with a field name of `Value` with [ ] notation.  For example, `[1, 2, 3]` which is equivlant to `Table( { Value: 1 }, { Value: 2 }, { Value: 3 } )`.

## Examples

### Simple circular formulas

In this example, the formulas for Area and Circumference are defined.  Just as in a spreadsheet, as Radius changes these values are automatically recalculated.  Note that Pi is also a formula, a static one with no dependencies, making this an immutable value.

```
Microsoft Power Fx Console Formula REPL, Version 0.2
Enter Excel formulas.  Use "Help()" for details.

> Set( Radius, 10 )
Radius: 10

> Pi = 3.14159265359
Pi: 3.14159265359

> Area = Pi * Radius * Radius
Area: 314.15926535899996

> Set( Radius, 300 )
Radius: 300
Area: 282743.33882309997

> Circumference = 2 * Pi * Radius
Circumference: 1884.955592154

> Set( Radius, 40 )
Radius: 40
Area: 5026.548245743999
Circumference: 251.3274122872
```

### Working with a table

This example creates a simple table of Students and then uses the Filter and Sort functions to manipulate the table.

```
Microsoft Power Fx Console Formula REPL, Version 0.2
Enter Excel formulas.  Use "Help()" for details.

> Set( Students, Table( { Name: "Nick", Age: 21 }, { Name: "Lisa", Age: 24 }, { Name: "Sam", Age: 19 }, { Name: "Emma", Age: 25 } ) )
Students: Table({Name:"Nick", Age:21}, {Name:"Lisa", Age:24}, {Name:"Sam", Age:19}, {Name:"Emma", Age:25})

> Filter( Students, Age > 20 )
Table({Name:"Nick", Age:21}, {Name:"Lisa", Age:24}, {Name:"Emma", Age:25})

> Filter( Students, "i" in Name )
Table({Name:"Nick", Age:21}, {Name:"Lisa", Age:24})

> Sort( Filter( Students, "i" in Name ), Name )
Table({Name:"Lisa", Age:24}, {Name:"Nick", Age:21})
```

## How does it work?

Input lines are categorized and processed:

- `Set( <identifier>, <expression> )` defines or updates a variable.  It evaluates the expression with `engine.Eval` and then passes the result to `engine.UpdateVariable`.
- `<identifier> = <expression>` defines a formula.  It is passed to `engine.SetFormula` directly.  The helper function `OnUpdate` is called if the value changes due to a dependency in *expression* changing.
- `<expression>` evaluates an expression.  It is passed to `engine.Eval` directly.  The helper function `PrintResult` recursively creates a string representation for the result.  Since the Boolean returing expression `x = y` conflicts with a formula definition, it needs to be wrapped in parens as `(x = y)`
- `Help()` is a custom host function that is added in with `engine.AddFunction`.  It prints some helpful notes and calls `engine.GetAllFunctions` for the list of available functions (always growing and can be augmented by the host).
- `Reset()` is another custom host function that creates a new engine `RecalcEngine` instance.  All variable and formula definitions are lost.
- `Exit()` is another custom host function that closes the console application.
